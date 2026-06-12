using System;
using Shears.Logging;
using UnityEditor;
using UnityEngine;

namespace Shears.Pathfinding.Editor
{
    [Serializable]
    public class IPGETSettings
    {
        [SerializeField]
        private bool drawNodeData = true;

        [SerializeField]
        private bool drawPrefab = true;

        [SerializeField]
        private bool drawAllDepths = true;

        [SerializeField]
        private int zDepth;

        [SerializeReference]
        private PathNodeData nodeData;

        [SerializeField]
        private PathNodeObject nodePrefab;

        private readonly IPathGrid grid;
        private readonly SerializedObject gridSO;
        private readonly SerializedObject editorSO;
        private readonly SerializedProperty nodeDataProp;
        private readonly SerializedProperty zDepthProp;
        private readonly SerializedProperty nodePrefabProp;
        private readonly SerializedProperty drawNodeDataProp;
        private readonly SerializedProperty drawPrefabProp;
        private readonly SerializedProperty drawAllDepthsProp;

        public IPathGrid Grid => grid;
        public bool DrawNodeData => drawNodeData;
        public bool DrawPrefab => drawPrefab;
        public bool DrawAllDepths => drawAllDepths;
        public int ZDepth => zDepth;
        public PathNodeData NodeData => nodeData;
        public PathNodeObject NodePrefab => nodePrefab;
        public bool IsActivated { get; set; }

        public SerializedObject EditorSO => editorSO;
        public SerializedObject GridSO => gridSO;
        public SerializedProperty DrawNodeDataProp => drawNodeDataProp;
        public SerializedProperty DrawPrefabProp => drawPrefabProp;
        public SerializedProperty DrawAllDepthsProp => drawAllDepthsProp;
        public SerializedProperty ZDepthProp => zDepthProp;
        public SerializedProperty NodeDataProp => nodeDataProp;
        public SerializedProperty NodePrefabProp => nodePrefabProp;

        public IPGETSettings(PathGridGroupEditorTool editor, IPathGrid grid)
        {
            editorSO = new SerializedObject(editor);
            this.grid = grid;

            if (grid is PathGrid pGrid)
                gridSO = new SerializedObject(pGrid);
            else if (grid is PathGridGroup pGridGroup)
                gridSO = new SerializedObject(pGridGroup);
            else
                SHLogger.Log($"Failed to resolve {nameof(PathGrid)} type!", SHLogLevels.Error);

            var settingsProp = editorSO.FindProperty("settings");
            nodeDataProp = settingsProp.FindPropertyRelative("nodeData");
            zDepthProp = settingsProp.FindPropertyRelative("zDepth");
            nodePrefabProp = settingsProp.FindPropertyRelative("nodePrefab");
            drawNodeDataProp = settingsProp.FindPropertyRelative("drawNodeData");
            drawPrefabProp = settingsProp.FindPropertyRelative("drawPrefab");
            drawAllDepthsProp = settingsProp.FindPropertyRelative("drawAllDepths");
        }

        public void ApplyModifiedProperties()
        {
            editorSO.ApplyModifiedProperties();
        }
    }
}
