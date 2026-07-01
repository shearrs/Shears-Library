using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shears.Editor
{
    [InitializeOnLoad]
    public class PrefabChildHider
    {
        static PrefabChildHider()
        {
            EditorApplication.hierarchyWindowItemOnGUI += HideChildren;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneLoaded;
            PrefabStage.prefabStageOpened += OnPrefabOpened;
            PrefabStage.prefabStageClosing += OnPrefabClosed;
        }

        ~PrefabChildHider()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= HideChildren;
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneLoaded;
            PrefabStage.prefabStageOpened -= OnPrefabOpened;
            PrefabStage.prefabStageClosing -= OnPrefabClosed;
        }

        private static void OnSceneLoaded(Scene oldScene, Scene newScene)
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnPrefabOpened(PrefabStage stage)
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnPrefabClosed(PrefabStage stage)
        {
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void HideChildren(int entityID, Rect selectionRect)
        {
            var gameObject = EditorUtility.EntityIdToObject(entityID) as GameObject;

            if (gameObject == null)
                return;

            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                if (
                    gameObject.transform.childCount > 0
                    && gameObject.transform.GetChild(0).hideFlags == HideFlags.None
                )
                    return;

                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i).gameObject;

                    if (child != null)
                        child.hideFlags = HideFlags.None;
                }

                return;
            }

            if (PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject) == gameObject)
            {
                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i).gameObject;

                    if (child != null)
                        child.hideFlags = HideFlags.HideInHierarchy;
                }
            }
        }
    }
}
