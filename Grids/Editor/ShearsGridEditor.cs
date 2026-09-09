using Shears.Editor;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Shears.Grids.Editor
{
    [CustomEditor(typeof(ShearsGrid))]
    public class ShearsGridEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            EditorApplication.delayCall += () =>
            {
                if (
                    target is ShearsGrid
                    && Selection.activeGameObject != null
                    && Selection.activeGameObject.TryGetComponent(out ShearsGrid _)
                )
                    ToolManager.SetActiveTool<ShearsGridTool>();
            };
        }

        [MenuItem("CONTEXT/" + nameof(ShearsGrid) + "/Sync Definitions")]
        private static void SyncDefinitions(MenuCommand command)
        {
            var grid = command.context as ShearsGrid;

            foreach (var node in grid.Nodes)
                node.SyncDefinition(grid);
            EditorUtility.SetDirty(grid);
        }

        [MenuItem("CONTEXT/" + nameof(ShearsGrid) + "/Crop to Content")]
        private static void CropToContent(MenuCommand command)
        {
            var grid = command.context as ShearsGrid;

            Undo.RegisterFullObjectHierarchyUndo(grid.gameObject, "Crop to Content");
            grid.CropToContent();
            EditorUtility.SetDirty(grid);
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement() { name = "Shears Grid Editor" };

            var logProp = serializedObject.FindProperty("logLevels");
            var sizeProp = serializedObject.FindProperty("size");

            ValidateSize(sizeProp);

            var scriptField = VisualElementEditorUtil.CreateScriptField(serializedObject);
            var logField = new PropertyField(logProp);
            var sizeField = new PropertyField(sizeProp);

            root.AddAll(scriptField, logField, sizeField);

            return root;
        }

        private void ValidateSize(SerializedProperty sizeProp)
        {
            var size = sizeProp.vector3IntValue;
            bool sizeChanged = false;

            if (size.x <= 0)
            {
                size.x = 1;
                sizeChanged = true;
            }
            if (size.y <= 0)
            {
                size.y = 1;
                sizeChanged = true;
            }
            if (size.z <= 0)
            {
                size.z = 1;
                sizeChanged = true;
            }

            if (sizeChanged)
                sizeProp.vector3IntValue = size;

            var nodesProp = serializedObject.FindProperty("nodes");
            var originalArraySize = nodesProp.arraySize;

            if (originalArraySize == size.x * size.y * size.z)
            {
                if (sizeChanged)
                    serializedObject.ApplyModifiedPropertiesWithoutUndo();

                return;
            }

            for (int z = 0; z < size.z; z++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    for (int x = 0; x < size.x; x++)
                    {
                        int index = (z * size.y * size.x) + (y * size.x) + x;
                        GridNode originalNode = null;
                        GridNode node;

                        if (index < originalArraySize)
                            originalNode =
                                nodesProp.GetArrayElementAtIndex(index).boxedValue as GridNode;

                        if (originalNode != null)
                            node = originalNode;
                        else
                            node = new(x, y, z);

                        if (index >= originalArraySize)
                            nodesProp.InsertArrayElementAtIndex(index);

                        nodesProp.GetArrayElementAtIndex(index).boxedValue = node;
                    }
                }
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
