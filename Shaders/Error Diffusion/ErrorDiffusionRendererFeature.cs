using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Shears.Shaders
{
    public class ErrorDiffusionRendererFeature : ScriptableRendererFeature
    {
        private const int NUMBER_OF_GROUPS = 32;

        [SerializeField]
        private ComputeShader ditherShader;

        [SerializeField]
        private Texture3D colorPalette;

        private DitherPass ditherPass;

        public override void Create()
        {
            ditherPass = new DitherPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing,
            };
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            if (ditherShader == null)
            {
                Debug.LogWarning("Must assign a compute shader.");
                return;
            }
            else if (colorPalette == null)
            {
                Debug.LogWarning("Must assign a color palette.");
                return;
            }
            else if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogWarning(
                    "Current system does not support compute shaders, can't add this render pass."
                );
                return;
            }

            ditherPass.Setup(ditherShader, colorPalette);
            renderer.EnqueuePass(ditherPass);
        }

        private class DitherPass : ScriptableRenderPass
        {
            private ComputeShader shader;
            private Texture3D palette;
            private int ditherKernel;
            private TextureHandle resultTexture;
            private TextureHandle errorTexture;
            private BufferHandle workgroupRiderBuffer;
            private BufferHandle workgroupProgressBuffer;

            private class PassData
            {
                public ComputeShader Shader { get; set; }
                public TextureHandle Palette { get; set; }
                public int DitherKernel { get; set; }
                public TextureHandle SourceTexture { get; set; }
                public TextureHandle ResultTexture { get; set; }
                public TextureHandle ErrorTexture { get; set; }
                public BufferHandle WorkgroupRiderBuffer { get; set; }
                public BufferHandle WorkgroupProgressBuffer { get; set; }
            }

            public void Setup(ComputeShader shader, Texture3D palette)
            {
                this.shader = shader;
                this.palette = palette;

                ditherKernel = shader.FindKernel("Dither");
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData
            )
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                var sourceTexture = resourceData.activeColorTexture;

                var resultDesc = sourceTexture.GetDescriptor(renderGraph);
                resultDesc.name = "Dither Result";
                resultDesc.enableRandomWrite = true;
                resultDesc.msaaSamples = MSAASamples.None;

                resultTexture = renderGraph.CreateTexture(resultDesc);

                var errorDesc = new TextureDesc(resultDesc.width, resultDesc.height)
                {
                    name = "Dither Error Diffusion",
                    msaaSamples = MSAASamples.None,
                    enableRandomWrite = true,
                    format = resultDesc.format,
                    clearBuffer = true,
                    clearColor = Color.clear,
                };
                errorTexture = renderGraph.CreateTexture(errorDesc);

                var paletteRTHandle = RTHandles.Alloc(palette);
                var paletteHandle = renderGraph.ImportTexture(paletteRTHandle);

                var workgroupRiderDesc = new BufferDesc()
                {
                    name = "Workgroup Rider",
                    stride = sizeof(uint),
                    count = 1,
                    target = GraphicsBuffer.Target.Structured,
                };

                workgroupRiderBuffer = renderGraph.CreateBuffer(workgroupRiderDesc);

                var workgroupProgressDesc = new BufferDesc()
                {
                    name = "Workgroup Progress",
                    stride = sizeof(uint),
                    count = NUMBER_OF_GROUPS,
                    target = GraphicsBuffer.Target.Structured,
                };

                workgroupProgressBuffer = renderGraph.CreateBuffer(workgroupProgressDesc);

                using (
                    var builder = renderGraph.AddComputePass<PassData>(
                        "Dither Compute Pass",
                        out var passData
                    )
                )
                {
                    passData.Shader = shader;
                    passData.Palette = paletteHandle;
                    passData.DitherKernel = ditherKernel;
                    passData.SourceTexture = sourceTexture;
                    passData.ResultTexture = resultTexture;
                    passData.ErrorTexture = errorTexture;
                    passData.WorkgroupRiderBuffer = workgroupRiderBuffer;
                    passData.WorkgroupProgressBuffer = workgroupProgressBuffer;

                    builder.UseTexture(passData.SourceTexture, AccessFlags.Read);
                    builder.UseTexture(passData.ResultTexture, AccessFlags.Write);
                    builder.UseTexture(passData.Palette, AccessFlags.Read);
                    builder.UseTexture(passData.ErrorTexture, AccessFlags.ReadWrite);
                    builder.UseBuffer(passData.WorkgroupRiderBuffer, AccessFlags.ReadWrite);
                    builder.UseBuffer(passData.WorkgroupProgressBuffer, AccessFlags.ReadWrite);

                    builder.SetRenderFunc(
                        static (PassData data, ComputeGraphContext ctx) =>
                        {
                            ctx.cmd.SetBufferData(data.WorkgroupRiderBuffer, new uint[1]);
                            ctx.cmd.SetBufferData(
                                data.WorkgroupProgressBuffer,
                                new uint[NUMBER_OF_GROUPS]
                            );

                            ctx.cmd.SetComputeTextureParam(
                                data.Shader,
                                data.Shader.FindKernel("ClearError"),
                                "_Source",
                                data.SourceTexture
                            );
                            ctx.cmd.SetComputeTextureParam(
                                data.Shader,
                                data.Shader.FindKernel("ClearError"),
                                "_ErrorValues",
                                data.ErrorTexture
                            );

                            ctx.cmd.SetComputeTextureParam(
                                data.Shader,
                                data.DitherKernel,
                                "_Source",
                                data.SourceTexture
                            );
                            ctx.cmd.SetComputeTextureParam(
                                data.Shader,
                                data.DitherKernel,
                                "_Result",
                                data.ResultTexture
                            );
                            ctx.cmd.SetComputeTextureParam(
                                data.Shader,
                                data.DitherKernel,
                                "_ColorPalette",
                                data.Palette
                            );
                            ctx.cmd.SetComputeTextureParam(
                                data.Shader,
                                data.DitherKernel,
                                "_ErrorValues",
                                data.ErrorTexture
                            );
                            ctx.cmd.SetComputeBufferParam(
                                data.Shader,
                                data.DitherKernel,
                                "_WorkgroupRider",
                                data.WorkgroupRiderBuffer
                            );
                            ctx.cmd.SetComputeBufferParam(
                                data.Shader,
                                data.DitherKernel,
                                "_WorkgroupProgress",
                                data.WorkgroupProgressBuffer
                            );

                            ctx.cmd.DispatchCompute(
                                data.Shader,
                                data.Shader.FindKernel("ClearError"),
                                1,
                                1,
                                1
                            );

                            ctx.cmd.DispatchCompute(
                                data.Shader,
                                data.DitherKernel,
                                NUMBER_OF_GROUPS,
                                1,
                                1
                            );

                            RTHandles.Release(data.Palette);
                        }
                    );
                }

                resourceData.cameraColor = resultTexture;
            }
        }
    }
}
