using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor.WindowTools
{
    public class RandomRotateField : VisualElement
    {
        private readonly Vector3Field minRotationField;
        private readonly Vector3Field maxRotationField;

        public RandomRotateField()
        {
            this.AddStyleSheet(ShearsStyles.InspectorStyles);
            AddToClassList(WindowToolsStyles.RandomScaleFieldClass);
            
            var titleContainer = new VisualElement();
            titleContainer.AddToClassList(ShearsStyles.DarkContainerClass);
            titleContainer.AddToClassList("rotationTitleContainer");

            var title = new Label("Rotate");

            titleContainer.Add(title);

            var scaleContainer = new VisualElement();
            scaleContainer.AddToClassList(ShearsStyles.DarkContainerClass);
            scaleContainer.AddToClassList("rotationContainer");

            minRotationField = new("Minimum Rotation")
            {
                value = new(-180, -180, -180)
            };

            maxRotationField = new("Maximum Rotation")
            {
                value = new(180, 180, 180)
            };

            scaleContainer.AddAll(minRotationField, maxRotationField);

            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(ShearsStyles.DarkContainerClass);
            buttonContainer.AddToClassList("rotationButtonContainer");

            var applyButton = new Button(OnRandomRotationButtonClicked)
            {
                text = "Apply"
            };

            buttonContainer.Add(applyButton);

            this.AddAll(titleContainer, scaleContainer, buttonContainer);
        }

        private void OnRandomRotationButtonClicked()
        {
            var selection = Selection.gameObjects;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Random Rotate");
            int groupID = Undo.GetCurrentGroup();

            foreach (var gameObject in selection)
            {
                Vector3 eulerRotation = VectorUtil.RandomRange(minRotationField.value, maxRotationField.value);

                Undo.RecordObject(gameObject.transform, "Random Rotate");
                gameObject.transform.localRotation = Quaternion.Euler(eulerRotation);
            }

            Undo.CollapseUndoOperations(groupID);
        }
    }
}
