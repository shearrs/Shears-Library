using System;
using UnityEditor;
using UnityEngine;

namespace Shears.Pathfinding.Editor
{
    [Serializable]
    public class PGETSettings
    {
        [SerializeField] private bool drawNodeData = true;
        [SerializeField] private bool drawPrefab = true;
        [SerializeField] private bool drawAllDepths = true;
        [SerializeField] private int zDepth;
        [SerializeReference] private PathNodeData nodeData;
        [SerializeField] private GameObject nodePrefab;

        private bool isActivated = false;

        private readonly PathGrid grid;
        private readonly SerializedObject gridSO;
        private readonly SerializedObject editorSO;
        private readonly SerializedProperty nodeDataProp;
        private readonly SerializedProperty zDepthProp;
        private readonly SerializedProperty nodePrefabProp;
        private readonly SerializedProperty drawNodeDataProp;
        private readonly SerializedProperty drawPrefabProp;
        private readonly SerializedProperty drawAllDepthsProp;

        public bool DrawNodeData => drawNodeData;
        public bool DrawPrefab => drawPrefab;
        public bool DrawAllDepths => drawAllDepths;
        public int ZDepth => zDepth;
        public PathNodeData NodeData => nodeData;
        public GameObject NodePrefab => nodePrefab;
        public bool IsActivated => isActivated;
        public PathGrid Grid => grid;
        public SerializedObject GridSO => gridSO;
        public SerializedObject EditorSO => editorSO;
        public SerializedProperty NodeDataProp => nodeDataProp;
        public SerializedProperty ZDepthProp => zDepthProp;
        public SerializedProperty NodePrefabProp => nodePrefabProp;
        public SerializedProperty DrawNodeDataProp => drawNodeDataProp;
        public SerializedProperty DrawPrefabProp => drawPrefabProp;
        public SerializedProperty DrawAllDepthsProp => drawAllDepthsProp;

        public PGETSettings(SerializedObject editorSO, PathGrid grid, SerializedObject gridSO)
        {
            this.editorSO = editorSO;
            this.grid = grid;
            this.gridSO = gridSO;

            var settingsProp = editorSO.FindProperty("settings");
            nodeDataProp = settingsProp.FindPropertyRelative("nodeData");
            zDepthProp = settingsProp.FindPropertyRelative("zDepth");
            nodePrefabProp = settingsProp.FindPropertyRelative("nodePrefab");
            drawNodeDataProp = settingsProp.FindPropertyRelative("drawNodeData");
            drawPrefabProp = settingsProp.FindPropertyRelative("drawPrefab");
            drawAllDepthsProp = settingsProp.FindPropertyRelative("drawAllDepths");
        }

        public void SetActivated(bool activated)
        {
            isActivated = activated;
        }
    }
}
