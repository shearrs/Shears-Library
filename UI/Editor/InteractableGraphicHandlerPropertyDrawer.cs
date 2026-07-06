using System.Collections.Generic;
using Shears.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.UI.Editor
{
    [CustomPropertyDrawer(typeof(InteractableGraphicHandler))]
    public class InteractableGraphicHandlerPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            var fields = VisualElementEditorUtil.CreateDefaultFields(property);

            root.Add(fields);

            var serializedObject = property.serializedObject;

            if (serializedObject.targetObject is not Component component)
                return root;

            var gameObject = component.gameObject;
            var targetContainer = new VisualElement();
            targetContainer.AddStyleSheet(ShearsStyles.InspectorStyles);
            targetContainer.AddToClassList(ShearsStyles.LightContainerClass);

            root.TrackPropertyValue(
                property,
                _ => UpdateTargets(root, targetContainer, gameObject, property)
            );

            UpdateTargets(root, targetContainer, gameObject, property);

            return root;
        }

        private void GetTargets(Transform transform, List<Object> targets)
        {
            if (transform.TryGetComponent(out UIImage image))
                targets.Add(image);
            else if (transform.TryGetComponent(out UIText text))
                targets.Add(text);
            else if (transform.TryGetComponent(out UITextGUI textGUI))
                targets.Add(textGUI);
            else if (transform.TryGetComponent(out Renderer renderer))
                targets.Add(renderer);

            GetTargetsRecursive(transform, targets);
        }

        private void GetTargetsRecursive(Transform transform, List<Object> targets)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (child.TryGetComponent(out UIImage image))
                    targets.Add(image);
                else if (child.TryGetComponent(out UIText text))
                    targets.Add(text);
                else if (child.TryGetComponent(out UITextGUI textGUI))
                    targets.Add(textGUI);
                else if (child.TryGetComponent(out Renderer renderer))
                    targets.Add(renderer);

                GetTargetsRecursive(child, targets);
            }
        }

        private void UpdateTargets(
            VisualElement root,
            VisualElement container,
            GameObject gameObject,
            SerializedProperty property
        )
        {
            var targets = new List<Object>();
            var graphicsProp = property.FindPropertyRelative("graphics");
            GetTargets(gameObject.transform, targets);

            for (int i = 0; i < graphicsProp.arraySize; i++)
            {
                var graphicProp = graphicsProp.GetArrayElementAtIndex(i);
                var imageProp = graphicProp.FindPropertyRelative("image");

                if (imageProp.objectReferenceValue != null)
                {
                    targets.Remove(imageProp.objectReferenceValue);
                    continue;
                }

                var rendererProp = graphicProp.FindPropertyRelative("renderer");

                if (rendererProp.objectReferenceValue != null)
                {
                    targets.Remove(rendererProp.objectReferenceValue);
                    continue;
                }

                var textProp = graphicProp.FindPropertyRelative("text");

                if (textProp.objectReferenceValue != null)
                {
                    targets.Remove(textProp.objectReferenceValue);
                    continue;
                }

                var textGUIProp = graphicProp.FindPropertyRelative("textGUI");

                if (textGUIProp.objectReferenceValue != null)
                {
                    targets.Remove(textGUIProp.objectReferenceValue);
                    continue;
                }
            }

            if (targets.Count == 0)
            {
                if (root.Contains(container))
                    root.Remove(container);

                return;
            }

            container.Clear();

            foreach (var target in targets)
            {
                void addTarget()
                {
                    graphicsProp.InsertArrayElementAtIndex(graphicsProp.arraySize);
                    var element = graphicsProp.GetArrayElementAtIndex(graphicsProp.arraySize - 1);

                    if (target is UIImage image)
                        element.boxedValue = new InteractableGraphic(image);
                    else if (target is UIText text)
                        element.boxedValue = new InteractableGraphic(text);
                    else if (target is UITextGUI textGUI)
                        element.boxedValue = new InteractableGraphic(textGUI);
                    else if (target is Renderer renderer)
                        element.boxedValue = new InteractableGraphic(renderer);

                    graphicsProp.serializedObject.ApplyModifiedProperties();
                }

                var button = new Button(addTarget) { text = $"+ {target.name}" };

                container.Add(button);
            }

            root.Add(container);
        }
    }
}
