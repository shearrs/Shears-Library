using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace Shears.Editor
{
    [CustomEditor(typeof(UnityEngine.Object), true), CanEditMultipleObjects]
    public class ObjectEditor : UnityEditor.Editor
    {
        private const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private readonly Dictionary<string, FoldoutGroup> foldouts = new();
        private CompositeFoldout currentFoldout;

        private readonly struct FoldoutGroup
        {
            private readonly string name;
            private readonly List<VisualElement> elements;
            private readonly bool expanded;

            public readonly string Name => name;
            public readonly IReadOnlyList<VisualElement> Elements => elements;
            public readonly bool Expanded => expanded;

            public FoldoutGroup(string name, bool expanded)
            {
                this.name = name;
                elements = new();
                this.expanded = expanded;
            }

            public void Add(VisualElement element) => elements.Add(element);
        }

        private struct CompositeFoldout
        {
            private readonly string name;
            private int fieldCount;

            public readonly string Name => name;
            public int FieldCount { readonly get => fieldCount; set => fieldCount = value; }

            public CompositeFoldout(string name, int fieldCount)
            {
                this.name = name;
                this.fieldCount = fieldCount;
            }   
        }

        public override VisualElement CreateInspectorGUI()
        {
            var defaultFieldsContainer = VisualElementUtil.CreateDefaultFields(serializedObject);

            for (int i = 0; i < defaultFieldsContainer.childCount; i++)
            {
                var childField = defaultFieldsContainer[i];
                string propName = childField.name;

                if (currentFoldout.FieldCount > 0)
                {
                    currentFoldout.FieldCount--;

                    foldouts[currentFoldout.Name].Add(childField);

                    continue;
                }

                var targetObject = serializedObject.targetObject;
                Type type = targetObject.GetType();
                FieldInfo field = type.GetField(propName, FLAGS);
                
                FoldoutGroupAttribute attribute;

                try
                {
                    attribute = field.GetCustomAttribute<FoldoutGroupAttribute>();
                }
                catch (ArgumentNullException)
                {
                    continue;
                }

                if (attribute == null)
                    continue;

                if (!foldouts.TryGetValue(attribute.Name, out var foldout))
                {
                    foldout = new(attribute.Name, attribute.Expanded);
                    foldout.Add(childField);

                    foldouts[attribute.Name] = foldout;
                }
                else
                    foldout.Add(childField);

                currentFoldout = new(attribute.Name, attribute.FieldCount - 1);
            }

            foreach (var foldoutGroup in foldouts.Values)
            {
                var foldout = new Foldout
                {
                    text = foldoutGroup.Name,
                    name = foldoutGroup.Name,
                    value = foldoutGroup.Expanded
                };

                foldout.AddStyleSheet(ShearsStyles.InspectorStyles);
                foldout.AddToClassList(ShearsStyles.DarkFoldoutClass);

                int index = defaultFieldsContainer.IndexOf(foldoutGroup.Elements[0]);
                defaultFieldsContainer.Insert(index, foldout);

                foreach (var element in foldoutGroup.Elements)
                {
                    defaultFieldsContainer.Remove(element);
                    foldout.Add(element);
                }
            }

            return defaultFieldsContainer;
        }
    }
}
