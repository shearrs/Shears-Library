using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphElementClipboardData
    {
        [SerializeField] private string id;

        public string ID => id;
    }
}
