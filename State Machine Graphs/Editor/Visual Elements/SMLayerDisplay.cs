using Shears.GraphViews;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.StateMachineGraphs.Editor
{
    public class SMLayerDisplay : VisualElement
    {
        private readonly List<VisualElement> layers = new();
        private  StateMachineGraph graphData;

        public SMLayerDisplay()
        {
            AddToClassList(SMEditorUtil.LayerDisplayClassName);
        }

        public void SetGraphData(StateMachineGraph graphData)
        {
            if (graphData == null)
                return;

            if (this.graphData != null)
                ClearGraphData();

            this.graphData = graphData;
            graphData.NodePathChanged += UpdateLayers;

            UpdateLayers();
        }

        public void ClearGraphData()
        {
            if (graphData == null)
                return;

            graphData.NodePathChanged -= UpdateLayers;
            graphData = null;

            ClearLayers();
        }

        private void ClearLayers()
        {
            foreach (var layer in layers)
                Remove(layer);

            layers.Clear();
        }

        private void UpdateLayers()
        {
            ClearLayers();

            CreateRootTag();
            foreach (var layer in graphData.NodePath)
                CreateLayerTag(layer);
        }

        private void CreateRootTag()
        {
            var layerTag = new Button(() => graphData.OpenRootPath())
            {
                text = "Root"
            };

            layerTag.AddToClassList(SMEditorUtil.LayerDisplayTagClassName);

            layers.Add(layerTag);
            Add(layerTag);
        }

        private void CreateLayerTag(GraphMultiNodeData nodeData)
        {
            var layerTag = new Button(() => graphData.OpenSubPath(nodeData))
            {
                text = nodeData.Name
            };

            layerTag.AddToClassList(SMEditorUtil.LayerDisplayTagClassName);

            layers.Add(layerTag);
            Add(layerTag);
        }
    }
}
