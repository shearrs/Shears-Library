using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shears.Editor
{
    [InitializeOnLoad]
    public class PrefabChildHider
    {
        private static readonly Dictionary<GameObject, List<GameObject>> hiddenChildren = new();

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
            if (!ShearsSettings.instance.HidePrefabChildren)
            {
                if (hiddenChildren.Count == 0)
                    return;

                foreach (var key in hiddenChildren.Keys)
                {
                    var list = hiddenChildren[key];

                    foreach (var child in list)
                    {
                        if (child != null)
                            child.hideFlags = HideFlags.None;
                    }
                }

                hiddenChildren.Clear();

                return;
            }

            var gameObject = EditorUtility.EntityIdToObject(entityID) as GameObject;

            if (gameObject == null)
                return;

            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                if (hiddenChildren.TryGetValue(gameObject, out var list))
                {
                    foreach (var child in list)
                    {
                        if (child != null)
                            child.hideFlags = HideFlags.None;
                    }

                    list.Clear();
                }

                return;
            }

            if (PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject) == gameObject)
            {
                if (!hiddenChildren.TryGetValue(gameObject, out var list))
                {
                    list = new List<GameObject>();
                    hiddenChildren.Add(gameObject, list);
                }

                list.Clear();

                for (int i = 0; i < gameObject.transform.childCount; i++)
                {
                    var child = gameObject.transform.GetChild(i).gameObject;

                    if (child != null)
                        child.hideFlags = HideFlags.HideInHierarchy;

                    list.Add(child);
                }
            }
        }
    }
}
