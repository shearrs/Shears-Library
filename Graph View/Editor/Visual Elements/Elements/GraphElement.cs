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
}
