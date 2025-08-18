using System.Collections.Generic;
using UnityEngine;

namespace Shears.GraphViews
{
    [System.Serializable]
    public abstract class GraphElementClipboardData
    {
        [SerializeField] private string id;

        public string OriginalID => id;

        public GraphElementClipboardData(string id)
        {
            this.id = id;
        }

        public abstract GraphElementData Paste(PasteData data);

        public virtual void PasteDependents(PasteData data)
        {
        }
    }
}
