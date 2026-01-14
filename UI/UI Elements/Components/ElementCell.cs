using UnityEngine;

namespace Shears.UI
{
    /// <summary>
    /// Holds a <see cref="CellContent"/>.
    /// </summary>
    public class ElementCell : UIElement
    {
        [Header("Element Cell")]
        [SerializeField, ReadOnly]
        private CellContent content;

        public CellContent Content => content;

        public void SetContent(CellContent content)
        {
            this.content = content;

            if (content == null)
                return;

            content.transform.SetParent(transform);
            content.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
