using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphElementData
    {
        private readonly string id;

        public string ID => id;

        public GraphElementData()
        {
            id = GraphViewUtil.NewGUID();
        }

        public GraphElementData(string id)
        {
            this.id = id;
        }

        public abstract void Select();
        public abstract void Deselect();
    }
}
