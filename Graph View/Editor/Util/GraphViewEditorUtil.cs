using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public static class GraphViewEditorUtil
    {
        private static readonly string GRAPH_VIEW_UNDO = "Graph View";

        private static readonly string STYLE_SHEET_PATH = "Graph View/Style Sheets";

        #region StyleSheet Classes
        public static readonly string GraphViewClassName = "graphView";
        public static readonly string RootContainerClassName = "rootContainer";
        public static readonly string BodyContainerClassName = "bodyContainer";
        public static readonly string GraphViewContainerClassName = "graphViewContainer";
        public static readonly string ContentViewContainerClassName = "contentViewContainer";
        public static readonly string GridBackgroundClassName = "gridBackground";
        public static readonly string GraphNodeClassName = "graphNode";
        public static readonly string GraphMultiNodeClassName = "graphMultiNode";
        public static readonly string GraphNodeSelectedClassName = "graphNodeSelected";
        public static readonly string EdgeClassName = "edge";
        public static readonly string EdgeLineClassName = "edgeLine";
        public static readonly string EdgeArrowClassName = "edgeArrow";
        public static readonly string EdgeLineSelectedClassName = "edgeLineSelected";
        public static readonly string EdgeArrowSelectedClassName = "edgeArrowSelected";
        #endregion

        public static StyleSheet GraphViewStyleSheet => Resources.Load<StyleSheet>($"{STYLE_SHEET_PATH}/GraphView");

        public static event Action UndoRedoEvent;

        static GraphViewEditorUtil()
        {
            Undo.undoRedoEvent += OnUndoRedo;
        }

        private static void OnUndoRedo(in UndoRedoInfo info)
        {
            if (info.undoName.Contains(GRAPH_VIEW_UNDO))
                UndoRedoEvent?.Invoke();
        }

        public static void Record(UnityEngine.Object obj, string actionName = "Undo Action")
        {
            string name = $"{GRAPH_VIEW_UNDO} - {actionName}";

            Undo.RecordObject(obj, name);
        }

        public static void RecordAndSave(UnityEngine.Object obj, Action action, string actionName = "Undo Action")
        {
            Record(obj, actionName);

            action();

            Save(obj);
        }

        public static void Save(UnityEngine.Object obj) => EditorUtility.SetDirty(obj);

        public static SerializedProperty GetElementProp(GraphData graphData, string elementID)
        {
            var graphSO = new SerializedObject(graphData);

            return GetElementProp(graphSO, elementID);
        }

        public static SerializedProperty GetElementProp(SerializedObject graphSO, string elementID)
        {
            var elementsProp = graphSO.FindProperty("graphElements");
            var valuesProp = elementsProp.FindPropertyRelative("values");
            SerializedProperty element = null;

            for (int i = 0; i < valuesProp.arraySize; i++)
            {
                var elementProp = valuesProp.GetArrayElementAtIndex(i);
                var elementIDProp = elementProp.FindPropertyRelative("id");

                if (elementIDProp.stringValue == elementID)
                {
                    element = elementProp;
                    break;
                }
            }

            return element;
        }

        public static bool IsInspectorLocked()
        {
            var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
            var window = EditorWindow.GetWindow(type, false, null, false);

            PropertyInfo info = type.GetProperty("isLocked", BindingFlags.Public | BindingFlags.Instance);

            return (bool)info.GetValue(window, null);
        }
    }
}
