using System;
using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Grids.Editor
{
    public class GridNodeDataCacheInspector : VisualElement
    {
        private readonly SerializedProperty cacheProperty;
        private readonly SerializedProperty dataProperty;
        private readonly VisualElement dataContainer;
        private readonly Button addButton;

        public GridNodeDataCacheInspector(SerializedProperty cacheProperty)
        {
            this.cacheProperty = cacheProperty;
            dataProperty = cacheProperty.FindPropertyRelative("data");

            dataContainer = CreateDataContainer();
            addButton = CreateAddButton();

            this.AddStyleSheet(ShearsGridStyles.EditorWindowStyleSheet);
            this.AddAll(dataContainer, addButton);
        }

        private VisualElement CreateDataContainer()
        {
            var container = new VisualElement() { name = "Data Container" };
            container.AddToClassList(ShearsGridStyles.NodeDataContainerClass);

            // for each node data, create a property drawer
            void updateData(SerializedProperty dataProperty)
            {
                container.Clear();

                if (dataProperty.arraySize == 0)
                {
                    var emptyLabel = new Label("No Data");
                    emptyLabel.AddToClassList(ShearsGridStyles.NodeDataEmptyLabel);
                    container.Add(emptyLabel);

                    return;
                }

                for (int i = 0; i < dataProperty.arraySize; i++)
                {
                    var dataProp = dataProperty.GetArrayElementAtIndex(i);
                    var field = new GridNodeDataInspector(dataProp);

                    if (i == 0)
                        field.AddToClassList(ShearsGridStyles.NodeDataCapClass);

                    if (i == dataProperty.arraySize - 1)
                        field.AddToClassList(ShearsGridStyles.NodeDataEndClass);

                    field.RemoveRequested += RemoveNodeData;
                    field.MoveUpRequested += MoveNodeDataUp;
                    field.MoveDownRequested += MoveNodeDataDown;

                    container.Add(field);
                }
            }

            container.TrackPropertyValue(dataProperty, updateData);
            updateData(dataProperty);

            return container;
        }

        private Button CreateAddButton()
        {
            var addButton = new Button() { name = "Add Node Data Button", text = "Add Data" };
            addButton.AddToClassList(ShearsGridStyles.NodeDataAddButtonClass);
            addButton.clicked += () => OpenAddMenu(addButton.worldBound);

            return addButton;
        }

        private void OpenAddMenu(Rect rect)
        {
            var dropdown = new TypeDropdown(
                TypeSelectionMode.Inheritance,
                typeof(GridNodeData),
                AddNodeData,
                CanAddDataType,
                new()
            );
            dropdown.Show(rect, 300);
        }

        private bool CanAddDataType(Type type)
        {
            if (type.GetConstructor(Type.EmptyTypes) == null)
                return false;

            if (type.IsDefined(typeof(DisallowMultipleComponent), true))
            {
                var cache = cacheProperty.boxedValue as GridNodeDataCache;

                if (cache.TryGetData(type, out _))
                    return false;
            }

            return true;
        }

        private void AddNodeData(SerializableType type)
        {
            var instance = Activator.CreateInstance(type);

            dataProperty.InsertArrayElementAtIndex(dataProperty.arraySize);
            dataProperty.GetArrayElementAtIndex(dataProperty.arraySize - 1).managedReferenceValue =
                instance;
            dataProperty.serializedObject.ApplyModifiedProperties();
        }

        private void RemoveNodeData(SerializedProperty nodeDataProperty)
        {
            for (int i = 0; i < dataProperty.arraySize; i++)
            {
                var property = dataProperty.GetArrayElementAtIndex(i);

                if (SerializedProperty.EqualContents(property, nodeDataProperty))
                {
                    dataProperty.DeleteArrayElementAtIndex(i);
                    dataProperty.serializedObject.ApplyModifiedProperties();

                    return;
                }
            }
        }

        private void MoveNodeDataUp(SerializedProperty nodeDataProperty)
        {
            for (int i = 0; i < dataProperty.arraySize; i++)
            {
                var property = dataProperty.GetArrayElementAtIndex(i);

                if (SerializedProperty.EqualContents(property, nodeDataProperty))
                {
                    if (i == 0)
                        return;

                    var swapProperty = dataProperty.GetArrayElementAtIndex(i - 1);

                    (property.managedReferenceValue, swapProperty.managedReferenceValue) = (
                        swapProperty.managedReferenceValue,
                        property.managedReferenceValue
                    );

                    swapProperty.serializedObject.ApplyModifiedProperties();

                    return;
                }
            }
        }

        private void MoveNodeDataDown(SerializedProperty nodeDataProperty)
        {
            for (int i = 0; i < dataProperty.arraySize; i++)
            {
                var property = dataProperty.GetArrayElementAtIndex(i);

                if (SerializedProperty.EqualContents(property, nodeDataProperty))
                {
                    if (i == dataProperty.arraySize - 1)
                        return;

                    var swapProperty = dataProperty.GetArrayElementAtIndex(i + 1);

                    (property.managedReferenceValue, swapProperty.managedReferenceValue) = (
                        swapProperty.managedReferenceValue,
                        property.managedReferenceValue
                    );

                    swapProperty.serializedObject.ApplyModifiedProperties();

                    return;
                }
            }
        }
    }
}
