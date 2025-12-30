using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [CustomWrapper(DisplayFields = new string[] { "m_RenderMode", "m_Camera", "m_PlaneDistance" })]
    public class UIElementCanvas : ManagedWrapper<Canvas>
    {
        private GraphicRaycaster raycaster;

        public Canvas UnityCanvas => TypedWrappedValue;
        public GraphicRaycaster Raycaster => raycaster;

        private void Awake()
        {
            TryGetComponent(out raycaster);
        }

        private void OnEnable()
        {
            UIElementEventSystem.RegisterCanvas(this);
        }

        private void OnDisable()
        {
            UIElementEventSystem.DeregisterCanvas(this);
        }
    }
}
