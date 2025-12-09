using System;
using UnityEditor;
using UnityEngine;

namespace Shears.Pathfinding.Editor
{
    [System.Serializable]
    public class PGETSettings
    {
        [SerializeField] private bool drawNodeData = true;
        [SerializeField] private bool drawPrefab = true;
        [SerializeField] private bool drawAllDepths = true;
        [SerializeField] private int zDepth;
        [SerializeReference] private PathNodeData nodeData;
        [SerializeField] private GameObject nodePrefab;

        private readonly PathGrid grid;
        private readonly SerializedProperty nodeDataProp;
        private readonly SerializedProperty zDepthProp;
        private readonly SerializedProperty nodePrefabProp;
        private readonly SerializedProperty drawNodeDataProp;
        private readonly SerializedProperty drawPrefabProp;
        private readonly SerializedProperty drawAllDepthsProp;

        public PathNodeData NodeData => nodeData;
        public PathGrid Grid => grid;
        public SerializedProperty NodeDataProp => nodeDataProp;
        public SerializedProperty ZDepthProp => zDepthProp;
        public SerializedProperty NodePrefabProp => nodePrefabProp;
        public SerializedProperty DrawNodeDataProp => drawNodeDataProp;
        public SerializedProperty DrawPrefabProp => drawPrefabProp;
        public SerializedProperty DrawAllDepthsProp => drawAllDepthsProp;

        public event Action NodeDataChanged;

        public PGETSettings(SerializedObject editorSO, PathGrid grid)
        {
            this.grid = grid;
            nodeDataProp = editorSO.FindProperty("nodeData");
            zDepthProp = editorSO.FindProperty("zDepth");
            nodePrefabProp = editorSO.FindProperty("nodePrefab");
            drawNodeDataProp = editorSO.FindProperty("drawNodeData");
            drawPrefabProp = editorSO.FindProperty("drawPrefab");
            drawAllDepthsProp = editorSO.FindProperty("drawAllDepths");
        }

        public void SetNodeData(PathNodeData newData)
        {
            nodeData = newData;

            NodeDataChanged?.Invoke();
        }
    }
}
