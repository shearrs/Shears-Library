using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Shears
{
    public class RenderScaler : MonoBehaviour
    {
        [SerializeField]
        private Vector2Int targetResolution = new(512, 512);

        void Update()
        {
            // Calculate the ratio between your desired low-res and actual screen size
            float scaleX = (float)targetResolution.x / Screen.width;
            float scaleY = (float)targetResolution.y / Screen.height;

            // Use the smaller ratio to ensure it fits the desired pixel boundary
            float finalScale = Mathf.Min(scaleX, scaleY);

            // Sets the global URP resolution scale for lighting and targets cleanly
            UniversalRenderPipeline.asset.upscalingFilter = UpscalingFilterSelection.Point;
            UniversalRenderPipeline.asset.renderScale = finalScale;
        }
    }
}
