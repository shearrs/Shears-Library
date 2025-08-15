using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphElementClipboardData
    {
        public abstract GraphElementData Paste(PasteData data);
    }
}
