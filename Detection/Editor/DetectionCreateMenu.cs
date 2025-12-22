using Shears.Editor;
using UnityEditor;
using UnityEngine;

namespace Shears.Detection.Editor
{
    public static class DetectionCreateMenu
    {
        private const string PATH_3D = CreateMenuUtility.LIBRARY_PATH + "/Area Detection/3D";

        [MenuItem(PATH_3D + "/Box Detector", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 100)]
        private static void CreateBoxDetector3D()
        {
            var gameObject = new GameObject("Box Detector 3D");
            gameObject.AddComponent<BoxDetector3D>();

            CreateMenuUtility.ParentToSelection(gameObject);
        }

        [MenuItem(PATH_3D + "/Sphere Detector", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 101)]
        private static void CreateSphereDetector()
        {
            var gameObject = new GameObject("Sphere Detector");
            gameObject.AddComponent<SphereDetector>();

            CreateMenuUtility.ParentToSelection(gameObject);
        }


        [MenuItem(PATH_3D + "/Ray Detector", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 102)]
        private static void CreateRayDetector()
        {
            var gameObject = new GameObject("Ray Detector 3D");
            gameObject.AddComponent<RayDetector3D>();

            CreateMenuUtility.ParentToSelection(gameObject);
        }

        [MenuItem(PATH_3D + "/Sphere Cast Detector", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 103)]
        private static void CreateSphereCastDetector()
        {
            var gameObject = new GameObject("Sphere Cast Detector");
            gameObject.AddComponent<SphereCastDetector>();

            CreateMenuUtility.ParentToSelection(gameObject);
        }
    }
}
