using UnityEngine;

namespace Shears
{
    public static class CameraUtil
    {
        public static Vector3 ScreenPointToPlanePosition(
            this Camera camera, Vector3 screenPoint, 
            Vector3 planeNormal, Vector3 planePosition
        )
        {
            Plane plane = new(planeNormal, planePosition);
            Ray ray = camera.ScreenPointToRay(screenPoint);

            plane.Raycast(ray, out float enter);

            return ray.GetPoint(enter);
        }
    }
}
