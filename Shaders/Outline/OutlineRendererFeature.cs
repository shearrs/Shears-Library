using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Shears.Shaders
{
    public class OutlineRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private Shader _whiteMaskShader;

        [SerializeField]
        private Shader _jumpFloodShader;

        [SerializeField]
        private List<OutlineGroup> _groups = new();

        private readonly List<OutlineRenderPass> _passes = new();

        public override void Create()
        {
            _passes.Clear();

            for (int i = 0; i < _groups.Count; i++)
                _passes.Add(new());
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var group in _groups)
                group.Dispose();

            _passes.Clear();
        }

        private void OnValidate()
        {
            foreach (var group in _groups)
            {
                if (group.InjectionPoint == RenderPassEvent.BeforeRendering)
                    group.InitializeDefaultValues();
            }
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            if (
                _whiteMaskShader == null
                || _jumpFloodShader == null
                || renderingData.cameraData.renderType == CameraRenderType.Overlay
            )
                return;

            while (_passes.Count > _groups.Count)
                _passes.RemoveAt(_passes.Count - 1);

            while (_passes.Count < _groups.Count)
                _passes.Add(new());

            for (int i = 0; i < _passes.Count; i++)
            {
                var pass = _passes[i];

                pass.Group = _groups[i];
                pass.Group.WhiteMaskShader = _whiteMaskShader;
                pass.Group.JumpFloodShader = _jumpFloodShader;
                pass.Group.UpdateMaterialProperties(renderingData.cameraData);

                pass.renderPassEvent = pass.Group.InjectionPoint;
                renderer.EnqueuePass(pass);
            }
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

            public OutlineGroup Group { get; set; }

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

            public OutlineRenderPass()
            {
                ConfigureInput(ScriptableRenderPassInput.Color);
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData
            )
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
                    Group.JumpFloodMaterial,
                    INITIALIZE_BUFFER_PASS
                );
                renderGraph.AddBlitPass(initParams, "Initialize Jump Flood Buffer");

                int jumpCount = Mathf.CeilToInt(Mathf.Log(Group.OutlineWidth + 1.0f, 2.0f)) - 1;
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
                        data.JumpFloodMaterial = Group.JumpFloodMaterial;
                        data.FloodTexture0 = floodTexture0;
                        builder.SetRenderAttachment(floodTexture1, 0);
                        builder.UseTexture(floodTexture0, AccessFlags.Read);
                        builder.AllowGlobalStateModification(true);

                        builder.SetRenderFunc(
                            static (PassData data, RasterGraphContext ctx) =>
                            {
                                ctx.cmd.SetGlobalVector(
                                    AXIS_WIDTH_ID,
                                    new Vector2(data.JumpWidth, 0)
                                );
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
                        data.JumpFloodMaterial = Group.JumpFloodMaterial;
                        data.FloodTexture1 = floodTexture1;
                        builder.SetRenderAttachment(floodTexture0, 0);
                        builder.UseTexture(floodTexture1, AccessFlags.Read);
                        builder.AllowGlobalStateModification(true);

                        builder.SetRenderFunc(
                            static (PassData data, RasterGraphContext ctx) =>
                            {
                                ctx.cmd.SetGlobalVector(
                                    AXIS_WIDTH_ID,
                                    new Vector2(0, data.JumpWidth)
                                );
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
                    data.JumpFloodMaterial = Group.JumpFloodMaterial;
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
                drawingSettings.overrideMaterial = Group.WhiteMaskMaterial;

                var filteringSettings = new FilteringSettings(
                    RenderQueueRange.all,
                    Group.LayerMask,
                    Group.RenderLayerMask
                );

                drawingSettings.overrideMaterialPassIndex = STENCIL_PASS;
                var listParams = new RendererListParams(
                    renderingData.cullResults,
                    drawingSettings,
                    filteringSettings
                );

                stencilHandle = renderGraph.CreateRendererList(listParams);

                drawingSettings.overrideMaterialPassIndex = WHITE_MASK_PASS;
                listParams = new RendererListParams(
                    renderingData.cullResults,
                    drawingSettings,
                    filteringSettings
                );

                maskHandle = renderGraph.CreateRendererList(listParams);
            }
        }
    }
}
