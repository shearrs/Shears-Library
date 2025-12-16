using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shears.HitDetection.Editor
{
    public class HitDetectionContextMenu : MonoBehaviour
    {
        [MenuItem("GameObject/Shears Library/Hit Detection/Hit Body 3D", priority = 2, secondaryPriority = 1)]
        private static void CreateHitBody3D()
        {
            var gameObject = new GameObject("Hit Body 3D");
            gameObject.AddComponent<HitBody3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem("GameObject/Shears Library/Hit Detection/Hurt Body 3D", priority = 2, secondaryPriority = 2)]
        private static void CreateHurtBody3D()
        {
            var gameObject = new GameObject("Hurt Body 3D");
            gameObject.AddComponent<HurtBody3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem("GameObject/Shears Library/Hit Detection/Hit Box 3D", priority = 2, secondaryPriority = 100)]
        private static void CreateHitBox3D()
        {
            var gameObject = new GameObject("Hit Box 3D");
            gameObject.AddComponent<HitBox3D>();

            ParentToSelection(gameObject);
        }

        [MenuItem("GameObject/Shears Library/Hit Detection/Hit Sphere", priority = 2, secondaryPriority = 101)]
        private static void CreateHitSphere()
        {
            var gameObject = new GameObject("Hit Sphere");
            gameObject.AddComponent<HitSphere>();

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

            Undo.RegisterCreatedObjectUndo(gameObject, $"Create {gameObject.name}");
        }
    }
}
