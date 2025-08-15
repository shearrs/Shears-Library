using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphElementClipboardData
    {
        private readonly string id;

        public string OriginalID => id;

        public GraphElementClipboardData(string id)
        {
            this.id = id;
        }

        public abstract GraphElementData Paste(PasteData data);

        public virtual GraphElementData PasteDependents(Dictionary<string, GraphElementData> data)
        {
            return null;
        }
    }
}
