using Shears.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shears.HitDetection.Editor
{
    public static class HitDetectionCreateMenu
    {
        [MenuItem(CreateMenuUtility.LIBRARY_PATH + "/Hit Detection/Hit Body 3D", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 1)]
        private static void CreateHitBody3D()
        {
            var gameObject = new GameObject("Hit Body 3D");
            gameObject.AddComponent<HitBody3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem(CreateMenuUtility.LIBRARY_PATH + "/Hit Detection/Hurt Body 3D", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 2)]
        private static void CreateHurtBody3D()
        {
            var gameObject = new GameObject("Hurt Body 3D");
            gameObject.AddComponent<HurtBody3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem(CreateMenuUtility.LIBRARY_PATH + "/Hit Detection/Hit Box 3D", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 100)]
        private static void CreateHitBox3D()
        {
            var gameObject = new GameObject("Hit Box 3D");
            gameObject.AddComponent<HitBox3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem(CreateMenuUtility.LIBRARY_PATH + "/Hit Detection/Hit Sphere", priority = CreateMenuUtility.LIBRARY_PRIORITY, secondaryPriority = 101)]
        private static void CreateHitSphere()
        {
            var gameObject = new GameObject("Hit Sphere");
            gameObject.AddComponent<HitSphere>();

            ParentToSelection(gameObject);
        }
        
        private static void ParentToSelection(GameObject gameObject)
        {
            CreateMenuUtility.ParentToSelection(gameObject);
        }
    }
}
