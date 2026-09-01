using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class OutlineRendererFeature : ScriptableRendererFeature
{
    private static readonly int OUTLINE_COLOR_ID = Shader.PropertyToID("_OutlineColor");
    private static readonly int OUTLINE_WIDTH_ID = Shader.PropertyToID("_OutlineWidth");

    [SerializeField]
    private Settings _settings = new();

    private OutlineRenderPass _pass;

    [Serializable]
    private class Settings
    {
        [field: Header("Settings")]
        [field: SerializeField, Min(0.0f)]
        public float OutlineWidth { get; private set; } = 4.0f;

        [field: SerializeField]
        public Color OutlineColor { get; private set; } = Color.white;

        [field: Header("Setup")]
        [field: SerializeField]
        public RenderPassEvent InjectionPoint { get; set; } =
            RenderPassEvent.AfterRenderingTransparents;

        [field: SerializeField]
        public LayerMask LayerMask { get; set; } = -1;

        [field: SerializeField]
        public RenderingLayerMask RenderLayerMask { get; set; } =
            RenderingLayerMask.defaultRenderingLayerMask;

        [field: SerializeField]
        public Shader WhiteMaskShader { get; set; }

        [field: SerializeField]
        public Shader JumpFloodShader { get; set; }

        [NonSerialized]
        public Material JumpFloodMaterial;

        public bool IsValid()
        {
            return WhiteMaskShader != null && JumpFloodShader != null;
        }

        public void ReallocateMaterialsIfNeeded()
        {
            ReallocateMaterialIfNeeded(ref JumpFloodMaterial, JumpFloodShader);
        }

        private void ReallocateMaterialIfNeeded(ref Material material, Shader shader)
        {
            if (material != null && material.shader != shader)
            {
                CoreUtils.Destroy(material);
                material = null;
            }

            if (material == null && shader != null)
                material = CoreUtils.CreateEngineMaterial(shader);
        }
    }

    public override void Create()
    {
        _pass = new(_settings);
    }

    protected override void Dispose(bool disposing)
    {
        if (_settings != null && _settings.JumpFloodMaterial != null)
            CoreUtils.Destroy(_settings.JumpFloodMaterial);
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData
    )
    {
        if (!_settings.IsValid() || renderingData.cameraData.renderType == CameraRenderType.Overlay)
            return;

        _pass.renderPassEvent = _settings.InjectionPoint;
        _settings.ReallocateMaterialsIfNeeded();

        _settings.JumpFloodMaterial.SetFloat(OUTLINE_WIDTH_ID, _settings.OutlineWidth);
        _settings.JumpFloodMaterial.SetColor(OUTLINE_COLOR_ID, _settings.OutlineColor);

        renderer.EnqueuePass(_pass);
    }

    private class OutlineRenderPass : ScriptableRenderPass
    {
        private const int STENCIL_PASS = 0;
        private const int WHITE_MASK_PASS = 1;
        private const int INITIALIZE_BUFFER_PASS = 0;
        private const int JUMP_FLOOD_PASS = 1;
        private const int OUTLINE_PASS = 2;
        private static readonly ShaderTagId UNIVERSAL_FORWARD = new("UniversalForward");
        private static readonly int AXIS_WIDTH_ID = Shader.PropertyToID("_AxisWidth");

        private readonly Settings _settings;

        private class PassData
        {
            public Material JumpFloodMaterial { get; set; }
            public RendererListHandle StencilListHandle { get; set; }
            public RendererListHandle MaskListHandle { get; set; }
            public TextureHandle SourceTexture { get; set; }
            public TextureHandle MaskTexture { get; set; }
            public TextureHandle FloodTexture0 { get; set; }
            public TextureHandle FloodTexture1 { get; set; }
            public float JumpWidth { get; set; }
        }

        public OutlineRenderPass(Settings settings)
        {
            _settings = settings;

            ConfigureInput(ScriptableRenderPassInput.Color);
            ConfigureInput(ScriptableRenderPassInput.Depth);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            var renderingData = frameData.Get<UniversalRenderingData>();
            var cameraData = frameData.Get<UniversalCameraData>();
            var lightData = frameData.Get<UniversalLightData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            var source = resourceData.cameraColor;
            var depth = resourceData.cameraDepth;

            if (!source.IsValid() || !depth.IsValid())
                return;

            var sourceDesc = source.GetDescriptor(renderGraph);

            CreateTextures(
                renderGraph,
                sourceDesc,
                out var maskTexture,
                out var floodTexture0,
                out var floodTexture1
            );

            CreateRendererLists(
                renderGraph,
                renderingData,
                cameraData,
                lightData,
                out var stencilListHandle,
                out var maskListHandle
            );

            using (
                var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Jump Flood Stencil",
                    out var data
                )
            )
            {
                data.StencilListHandle = stencilListHandle;

                builder.UseRendererList(stencilListHandle);
                builder.SetRenderAttachmentDepth(depth, AccessFlags.ReadWrite);

                builder.SetRenderFunc(
                    static (PassData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawRendererList(data.StencilListHandle);
                    }
                );
            }

            using (
                var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Jump Flood Mask",
                    out var data
                )
            )
            {
                data.MaskListHandle = maskListHandle;

                builder.UseRendererList(maskListHandle);
                builder.SetRenderAttachment(maskTexture, 0);

                builder.SetRenderFunc(
                    static (PassData data, RasterGraphContext ctx) =>
                    {
                        ctx.cmd.DrawRendererList(data.MaskListHandle);
                    }
                );
            }

            var initParams = new RenderGraphUtils.BlitMaterialParameters(
                maskTexture,
                floodTexture0,
                _settings.JumpFloodMaterial,
                INITIALIZE_BUFFER_PASS
            );
            renderGraph.AddBlitPass(initParams, "Initialize Jump Flood Buffer");

            int jumpCount = Mathf.CeilToInt(Mathf.Log(_settings.OutlineWidth + 1.0f, 2.0f)) - 1;
            for (int i = jumpCount; i >= 0; i--)
            {
                float jumpWidth = Mathf.Pow(2, i) + 0.5f;

                using (
                    var builder = renderGraph.AddRasterRenderPass<PassData>(
                        "Jump Flood Outline Horizontal",
                        out var data
                    )
                )
                {
                    data.JumpWidth = jumpWidth;
                    data.JumpFloodMaterial = _settings.JumpFloodMaterial;
                    data.FloodTexture0 = floodTexture0;
                    builder.SetRenderAttachment(floodTexture1, 0);
                    builder.UseTexture(floodTexture0, AccessFlags.Read);
                    builder.AllowGlobalStateModification(true);

                    builder.SetRenderFunc(
                        static (PassData data, RasterGraphContext ctx) =>
                        {
                            ctx.cmd.SetGlobalVector(AXIS_WIDTH_ID, new Vector2(data.JumpWidth, 0));
                            Blitter.BlitTexture(
                                ctx.cmd,
                                data.FloodTexture0,
                                new(1, 1, 0, 0),
                                data.JumpFloodMaterial,
                                JUMP_FLOOD_PASS
                            );
                        }
                    );
                }

                using (
                    var builder = renderGraph.AddRasterRenderPass<PassData>(
                        "Jump Flood Outline Vertical",
                        out var data
                    )
                )
                {
                    data.JumpWidth = jumpWidth;
                    data.JumpFloodMaterial = _settings.JumpFloodMaterial;
                    data.FloodTexture1 = floodTexture1;
                    builder.SetRenderAttachment(floodTexture0, 0);
                    builder.UseTexture(floodTexture1, AccessFlags.Read);
                    builder.AllowGlobalStateModification(true);

                    builder.SetRenderFunc(
                        static (PassData data, RasterGraphContext ctx) =>
                        {
                            ctx.cmd.SetGlobalVector(AXIS_WIDTH_ID, new Vector2(0, data.JumpWidth));
                            Blitter.BlitTexture(
                                ctx.cmd,
                                data.FloodTexture1,
                                new(1, 1, 0, 0),
                                data.JumpFloodMaterial,
                                JUMP_FLOOD_PASS
                            );
                        }
                    );
                }
            }

            using (
                var builder = renderGraph.AddRasterRenderPass<PassData>(
                    "Jump Flood Outline",
                    out var data
                )
            )
            {
                data.JumpFloodMaterial = _settings.JumpFloodMaterial;
                data.FloodTexture0 = floodTexture0;

                builder.SetRenderAttachment(source, 0);
                builder.SetRenderAttachmentDepth(depth, AccessFlags.ReadWrite);
                builder.UseTexture(floodTexture0, AccessFlags.Read);

                builder.SetRenderFunc(
                    static (PassData data, RasterGraphContext ctx) =>
                    {
                        Blitter.BlitTexture(
                            ctx.cmd,
                            data.FloodTexture0,
                            new Vector4(1, 1, 0, 0),
                            data.JumpFloodMaterial,
                            OUTLINE_PASS
                        );
                    }
                );
            }
        }

        private void CreateTextures(
            RenderGraph renderGraph,
            TextureDesc sourceDesc,
            out TextureHandle maskTexture,
            out TextureHandle floodTexture0,
            out TextureHandle floodTexture1
        )
        {
            sourceDesc.clearBuffer = true;
            sourceDesc.clearColor = new(0, 0, 0, 0);
            sourceDesc.filterMode = FilterMode.Point;

            sourceDesc.name = "Mask Texture";
            sourceDesc.colorFormat = GraphicsFormat.R8_UNorm;
            maskTexture = renderGraph.CreateTexture(sourceDesc);

            sourceDesc.name = "Flood Texture 0";
            sourceDesc.colorFormat = GraphicsFormat.R16G16_SNorm;
            floodTexture0 = renderGraph.CreateTexture(sourceDesc);

            sourceDesc.name = "Flood Texture 1";
            floodTexture1 = renderGraph.CreateTexture(sourceDesc);
        }

        private void CreateRendererLists(
            RenderGraph renderGraph,
            UniversalRenderingData renderingData,
            UniversalCameraData cameraData,
            UniversalLightData lightData,
            out RendererListHandle stencilHandle,
            out RendererListHandle maskHandle
        )
        {
            var sortingCriteria = cameraData.defaultOpaqueSortFlags;
            var drawingSettings = CreateDrawingSettings(
                UNIVERSAL_FORWARD,
                renderingData,
                cameraData,
                lightData,
                sortingCriteria
            );
            drawingSettings.overrideShader = _settings.WhiteMaskShader;

            var filteringSettings = new FilteringSettings(
                RenderQueueRange.all,
                _settings.LayerMask,
                _settings.RenderLayerMask
            );

            drawingSettings.overrideShaderPassIndex = STENCIL_PASS;
            var listParams = new RendererListParams(
                renderingData.cullResults,
                drawingSettings,
                filteringSettings
            );

            stencilHandle = renderGraph.CreateRendererList(listParams);

            drawingSettings.overrideShaderPassIndex = WHITE_MASK_PASS;
            listParams = new RendererListParams(
                renderingData.cullResults,
                drawingSettings,
                filteringSettings
            );

            maskHandle = renderGraph.CreateRendererList(listParams);
        }
    }
}
