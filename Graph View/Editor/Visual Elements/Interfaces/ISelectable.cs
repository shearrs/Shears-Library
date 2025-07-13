using UnityEngine;

namespace Shears.GraphViews.Editor
{
    public interface ISelectable : IGraphElement
    {
        public void Select();

        public void Deselect();
    }
}
