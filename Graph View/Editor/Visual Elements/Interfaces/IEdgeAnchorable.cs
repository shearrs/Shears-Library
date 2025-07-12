using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public interface IEdgeAnchorable
    {
        public string ID { get; }
        public GraphElement Element { get; }

        public bool HasConnectionTo(IEdgeAnchorable anchor);
    }
}
