using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(ManagedSelectable))]
    public class ManagedSelectableEditor : UnityEditor.Editor
    {
        private GameObject gameObject;

        private void OnEnable()
        {
            gameObject = (target as MonoBehaviour).gameObject;

            var selectable = GetSelectable();

            selectable.hideFlags = HideFlags.HideInInspector;
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
                return;

            if (target == null && gameObject != null)
            {
                if (gameObject.TryGetComponent(out ManagedSelectable _))
                    return;

                DestroyImmediate(gameObject.GetComponent<Selectable>());

                gameObject = null;
            } 
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SerializedObject selectableSO = new(GetSelectable());

            EditorGUILayout.PropertyField(selectableSO.FindProperty("m_Navigation"));
            selectableSO.ApplyModifiedProperties();
        }

        private Selectable GetSelectable()
        {
            var managedSelectable = target as ManagedSelectable;

            return managedSelectable.GetComponent<Selectable>();
        }
    }
}