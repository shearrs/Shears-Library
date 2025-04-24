using Shears.Input;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Shears.Input.Editor
{
    [CustomEditor(typeof(ManagedInputMap))]
    public class ManagedInputMapInspector : UnityEditor.Editor
    {
        private VisualElement root;
        private DropdownField actionMapDropdown;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();

            var inputActionProp = serializedObject.FindProperty("inputActions");
            var inputActionField = new PropertyField(inputActionProp);

            inputActionField.RegisterValueChangeCallback(UpdateInputAction);

            root.Add(inputActionField);
            TryAddActionMapField();

            return root;
        }

        private void UpdateInputAction(SerializedPropertyChangeEvent evt)
        {
            if (actionMapDropdown != null)
            {
                actionMapDropdown.UnregisterValueChangedCallback(UpdateActionMap);
                root.Remove(actionMapDropdown);
                actionMapDropdown = null;
            }

            TryAddActionMapField();
        }

        private void TryAddActionMapField()
        {
            var inputActions = serializedObject.FindProperty("inputActions").objectReferenceValue as InputActionAsset;

            if (inputActions == null)
                return;

            var actionMaps = GetActionMaps(inputActions);

            if (actionMaps.Count == 0)
                return;

            actionMapDropdown = new DropdownField("Action Map", actionMaps, 0);

            int currentActionMap = GetCurrentActionMapIndex(actionMaps);

            actionMapDropdown.index = currentActionMap;

            actionMapDropdown.RegisterValueChangedCallback(UpdateActionMap);
            SetActionMap(actionMaps[currentActionMap]);

            root.Add(actionMapDropdown);
        }

        private void UpdateActionMap(ChangeEvent<string> evt) => SetActionMap(evt.newValue);

        private void SetActionMap(string name)
        {
            serializedObject.FindProperty("actionMapName").stringValue = name;
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private List<string> GetActionMaps(InputActionAsset inputActions)
        {
            List<string> maps = new();

            foreach (var map in inputActions.actionMaps)
                maps.Add(map.name);

            return maps;
        }

        private int GetCurrentActionMapIndex(List<string> actionMaps)
        {
            for (int i = 0; i < actionMaps.Count; i++)
            {
                if (actionMaps[i] == serializedObject.FindProperty("actionMapName").stringValue)
                    return i;
            }

            return 0;
        }
    }
}