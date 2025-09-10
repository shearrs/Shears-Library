using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Editor.WindowTools
{
    public class RandomScaleField : VisualElement
    {
        private bool isUniform;
        private readonly FloatField minScaleField;
        private readonly FloatField maxScaleField;
        private readonly Vector3Field minVectorScaleField;
        private readonly Vector3Field maxVectorScaleField;

        public RandomScaleField()
        {
            this.AddStyleSheet(ShearsStyles.InspectorStyles);
            AddToClassList(WindowToolsStyles.RandomScaleFieldClass);

            style.marginTop = 2;

            var titleContainer = new VisualElement();
            titleContainer.AddToClassList(ShearsStyles.DarkContainerClass);
            titleContainer.AddToClassList("randomScaleTitleContainer");

            var title = new Label("Scale");

            var uniformToggle = new Toggle("Uniform");
            uniformToggle.RegisterValueChangedCallback(OnUniformScaleToggled);

            titleContainer.AddAll(title, uniformToggle);

            var scaleContainer = new VisualElement();
            scaleContainer.AddToClassList(ShearsStyles.DarkContainerClass);
            scaleContainer.AddToClassList("randomScaleContainer");

            minScaleField = new("Minimum Scale")
            {
                value = 0.5f
            };

            maxScaleField = new("Maximum Scale")
            {
                value = 1.5f
            };

            minScaleField.style.display = DisplayStyle.None;
            maxScaleField.style.display = DisplayStyle.None;

            minVectorScaleField = new("Minimum Scale")
            {
                value = new(0.5f, 0.5f, 0.5f)
            };

            maxVectorScaleField = new("Maximum Scale")
            {
                value = new(1.5f, 1.5f, 1.5f)
            };

            scaleContainer.AddAll(minScaleField, maxScaleField, minVectorScaleField, maxVectorScaleField);

            var buttonContainer = new VisualElement();
            buttonContainer.AddToClassList(ShearsStyles.DarkContainerClass);
            buttonContainer.AddToClassList("randomScaleButtonContainer");

            var applyButton = new Button(OnRandomScaleButtonClicked)
            {
                text = "Apply"
            };

            buttonContainer.Add(applyButton);

            this.AddAll(titleContainer, scaleContainer, buttonContainer);
        }

        private void OnUniformScaleToggled(ChangeEvent<bool> evt)
        {
            isUniform = evt.newValue;

            if (isUniform)
            {
                minVectorScaleField.style.display = DisplayStyle.None;
                maxVectorScaleField.style.display = DisplayStyle.None;

                minScaleField.style.display = DisplayStyle.Flex;
                maxScaleField.style.display = DisplayStyle.Flex;
            }
            else
            {
                minVectorScaleField.style.display = DisplayStyle.Flex;
                maxVectorScaleField.style.display = DisplayStyle.Flex;

                minScaleField.style.display = DisplayStyle.None;
                maxScaleField.style.display = DisplayStyle.None;
            }
        }

        private void OnRandomScaleButtonClicked()
        {
            var selection = Selection.gameObjects;

            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Random Scale");
            int groupID = Undo.GetCurrentGroup();

            foreach (var gameObject in selection)
            {
                Vector3 scale;

                if (isUniform)
                {
                    float scaleValue = Random.Range(minScaleField.value, maxScaleField.value);
                    scale = new(scaleValue, scaleValue, scaleValue);
                }
                else
                    scale = VectorUtil.RandomRange(minVectorScaleField.value, maxVectorScaleField.value);

                Undo.RecordObject(gameObject.transform, "Random Scale");
                gameObject.transform.localScale = scale;
            }

            Undo.CollapseUndoOperations(groupID);
        }
    }
}
