using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shears.HitDetection.Editor
{
    public class HitDetectionContextMenu : MonoBehaviour
    {
        [MenuItem("GameObject/Shears Library/Hit Detection/Hit Body 3D", secondaryPriority = 100)]
        private static void CreateHitBody3D()
        {
            var gameObject = new GameObject("Hit Body 3D");
            gameObject.AddComponent<HitBody3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem("GameObject/Shears Library/Hit Detection/Shapes/Hit Box 3D")]
        private static void CreateHitBox3D()
        {
            var gameObject = new GameObject("Hit Box 3D");
            gameObject.AddComponent<HitBox3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem("GameObject/Shears Library/Hit Detection/Shapes/Hit Sphere")]
        private static void CreateHitSphere()
        {
            var gameObject = new GameObject("Hit Sphere");
            gameObject.AddComponent<HitSphere>();

            ParentToSelection(gameObject);
        }

        [MenuItem("GameObject/Shears Library/Hit Detection/Hurt Body 3D")]
        private static void CreateHurtBody3D()
        {
            var gameObject = new GameObject("Hurt Body 3D");
            gameObject.AddComponent<HurtBody3D>();

            ParentToSelection(gameObject);
        }

        private static void ParentToSelection(GameObject gameObject)
        {
            var selection = Selection.activeGameObject;
            Transform parent = null;

            if (selection != null)
                parent = selection.transform;
            else
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();

                if (stage != null)
                    parent = stage.prefabContentsRoot.transform;
            }

            gameObject.transform.SetParent(parent, false);
            Selection.activeGameObject = gameObject;
        }
    }
}
