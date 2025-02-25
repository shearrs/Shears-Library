using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI.Editor
{
    [CustomEditor(typeof(PrebuiltSelectable))]
    public class PrebuiltSelectableEditor : UnityEditor.Editor
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
                if (gameObject.TryGetComponent(out PrebuiltSelectable _))
                    return;

                DestroyImmediate(gameObject.GetComponent<Selectable>());

                gameObject = null;
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            SerializedObject selectableSO = new(GetSelectable());

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Navigation", EditorStyles.boldLabel);
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
