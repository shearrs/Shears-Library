using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphElement<T> : VisualElement where T : GraphElementData
    {
        private readonly T data;

        public T Data => data;

        public GraphElement(T data)
        {
            this.data = data;
        }

        public abstract void Select();
        public abstract void Deselect();
    }
}
