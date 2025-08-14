using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    public readonly struct CopyData
    {
        private readonly GraphData graphData;
        private readonly IReadOnlyList<GraphElementClipboardData> clipboardData;

        public readonly GraphData GraphData => graphData;
        public readonly IReadOnlyList<GraphElementClipboardData> ClipboardData => clipboardData;

        public CopyData(GraphData graphData, IReadOnlyList<GraphElementClipboardData> clipboardData)
        {
            this.graphData = graphData;
            this.clipboardData = clipboardData;
        }
    }

    public interface ICopyable
    {
        public GraphElementClipboardData CopyToClipboard(CopyData data);
    }

    public interface ICopyable<T> : ICopyable where T : GraphElementClipboardData
    {
        new public T CopyToClipboard(CopyData data);
    }
}
