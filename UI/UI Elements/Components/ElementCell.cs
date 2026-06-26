using System;
using Shears.Tweens;
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

        [SerializeField, RuntimeReadOnly]
        private SpriteRenderer[] sprites;

        public CellContent Content => content;
        public Action<ElementCell, CellContent> ContentSetter { get; set; }

        public void SetContent(CellContent content)
        {
            if (ContentSetter != null)
            {
                ContentSetter(this, content);
                return;
            }

            ForceSetContent(content);
        }

        public void ForceSetContent(CellContent content)
        {
            this.content = content;

            if (content == null)
                return;

            content.transform.SetParent(transform, true);
            content.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            content.Cell = this;
        }
    }
}
