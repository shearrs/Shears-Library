using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Shears.Shaders
{
    public class DitherRendererFeature : ScriptableRendererFeature
    {
        [SerializeField]
        private RenderPassEvent injectionPoint;

        [SerializeField]
        private Material material;

        private DownscalePass downscalePass;

        public override void Create()
        {
            downscalePass = new() { renderPassEvent = injectionPoint };
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData
        )
        {
            if (material == null)
                return;

            downscalePass.Setup(material);
            renderer.EnqueuePass(downscalePass);
        }

        private class DownscalePass : ScriptableRenderPass
        {
            private Material material;

            public void Setup(Material material)
            {
                this.material = material;

                ConfigureInput(ScriptableRenderPassInput.Color);
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData
            )
            {
                var resourceData = frameData.Get<UniversalResourceData>();

                var cameraColor = resourceData.activeColorTexture;

                if (!cameraColor.IsValid())
                    return;

                var cameraDesc = renderGraph.GetTextureDesc(cameraColor);

                var ditherDesc = new TextureDesc(cameraDesc.width, cameraDesc.height)
                {
                    name = "Downscaled Render",
                    colorFormat = cameraDesc.colorFormat,
                    depthBufferBits = 0,
                    clearBuffer = false,
                    filterMode = FilterMode.Point,
                };

                var ditherTexture = renderGraph.CreateTexture(ditherDesc);
                var ditherParams = new RenderGraphUtils.BlitMaterialParameters(
                    cameraColor,
                    ditherTexture,
                    material,
                    0
                );
                renderGraph.AddBlitPass(ditherParams, "Dither Render");
                renderGraph.AddBlitPass(
                    ditherTexture,
                    cameraColor,
                    Vector2.one,
                    Vector2.zero,
                    filterMode: RenderGraphUtils.BlitFilterMode.ClampNearest,
                    passName: "Blit Dither Back"
                );
            }
        }
    }
}
