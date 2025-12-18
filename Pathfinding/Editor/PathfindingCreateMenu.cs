using Shears.Editor;
using UnityEditor;
using UnityEngine;

namespace Shears.Pathfinding.Editor
{
    public static class PathfindingCreateMenu
    {
        const int myPrio = 5;

        [MenuItem(CreateMenuUtility.LIBRARY_PATH + "/Path Grid", priority = CreateMenuUtility.LIBRARY_PRIORITY)]
        private static void CreatePathGrid()
        {
            var gameObject = new GameObject("Path Grid");
            gameObject.AddComponent<PathGrid>();

            CreateMenuUtility.ParentToSelection(gameObject);
        }
    }
}
