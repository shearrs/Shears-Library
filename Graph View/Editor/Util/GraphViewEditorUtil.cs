using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public static class GraphViewEditorUtil
    {
        private static readonly string GRAPH_VIEW_UNDO = "Graph View";

        private static readonly string STYLE_SHEET_PATH = "Graph View/Style Sheets";

        public static string GraphViewClassName => "graphView";
        public static string RootContainerClassName => "rootContainer";
        public static string GraphViewContainerClassName => "graphViewContainer";
        public static string ContentViewContainerClassName => "contentViewContainer";
        public static string GridBackgroundClassName => "gridBackground";
        public static string GraphNodeClassName => "graphNode";
        public static string GraphNodeSelectedClassName => "graphNodeSelected";

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
    }
}
