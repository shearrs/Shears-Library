using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shears.Editor
{
    public static class CreateMenuUtility
    {
        public const int LIBRARY_PRIORITY = 2;
        public const string LIBRARY_PATH = "GameObject/Shears Library";

        public static void ParentToSelection(GameObject gameObject)
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
