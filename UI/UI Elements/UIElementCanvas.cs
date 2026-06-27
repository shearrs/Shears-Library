using System.Collections.Generic;
using Shears.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [CustomWrapper(DisplayFields = new string[] { "m_RenderMode", "m_Camera", "m_PlaneDistance" })]
    [RequireComponent(typeof(GraphicRaycaster))]
    public class UIElementCanvas : ManagedWrapper<Canvas>
    {
        [SerializeField]
        private int sortOrder = 0;

        private readonly Dictionary<UIElement, int> childrenSortingOrder = new();
        private GraphicRaycaster raycaster;

        public int SortOrder
        {
            get => sortOrder;
            set => sortOrder = value;
        }
        public Canvas UnityCanvas => TypedWrappedValue;
        public GraphicRaycaster Raycaster => raycaster;

        private void OnValidate()
        {
            TypedWrappedValue.sortingOrder = sortOrder;
        }

        private void Awake()
        {
            raycaster = GetComponent<GraphicRaycaster>();

            UpdateSortingOrder();
        }

        private void OnTransformChildrenChanged()
        {
            UpdateSortingOrder();
        }

        private void OnEnable()
        {
            UIElementEventSystem.RegisterCanvas(this);
        }

        private void OnDisable()
        {
            UIElementEventSystem.DeregisterCanvas(this);
        }

        public int GetSortOrder(UIElement element)
        {
            if (childrenSortingOrder.TryGetValue(element, out var value))
                return value;

            SHLogger.Log(
                $"{nameof(UIElementCanvas)} does not contain child {element}!",
                SHLogLevels.Error,
                context: this
            );
            return -1;
        }

        private void UpdateSortingOrder()
        {
            childrenSortingOrder.Clear();

            UpdateSortingOrderRecursive(transform, 0);
        }

        private int UpdateSortingOrderRecursive(Transform transform, int weight = 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                weight += i;
                var child = transform.GetChild(i);

                if (child.TryGetComponent(out UIElement element))
                    childrenSortingOrder.Add(element, weight);

                weight += UpdateSortingOrderRecursive(child);
            }

            return weight;
        }
    }
}
