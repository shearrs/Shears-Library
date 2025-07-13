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
            graphData.LayersChanged += UpdateLayers;

            UpdateLayers();
        }

        public void ClearGraphData()
        {
            if (graphData == null)
                return;

            graphData.LayersChanged -= UpdateLayers;
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

            foreach (var layer in graphData.Layers)
                CreateLayerTag(layer);
        }

        private void CreateLayerTag(GraphLayer layer)
        {
            var layerTag = new Button(() => graphData.OpenLayer(layer))
            {
                text = layer.ParentName
            };

            layerTag.AddToClassList(SMEditorUtil.LayerDisplayTagClassName);

            layers.Add(layerTag);
            Add(layerTag);
        }
    }
}
