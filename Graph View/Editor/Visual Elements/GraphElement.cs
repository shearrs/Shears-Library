using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public abstract class GraphElement : VisualElement
    {
        public abstract GraphElementData GetData();

        public abstract void Select();
        public abstract void Deselect();
    }

    public abstract class GraphElement<T> : GraphElement where T : GraphElementData
    {
        private readonly T data;

        public T Data => data;

        public GraphElement(T data)
        {
            this.data = data;
        }

        public override GraphElementData GetData() => data;
    }
}
