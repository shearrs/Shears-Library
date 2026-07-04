using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    [CustomWrapper(DisplayFields = new string[] { "m_RenderMode", "m_Camera", "m_PlaneDistance" })]
    [RequireComponent(typeof(GraphicRaycaster), typeof(Canvas))]
    public class UIElementCanvas : UIElement
    {
        private Canvas unityCanvas;
        private GraphicRaycaster raycaster;

        internal GraphicRaycaster Raycaster => raycaster;
        public Canvas UnityCanvas
        {
            get
            {
                if (unityCanvas == null)
                    unityCanvas = GetComponent<Canvas>();

                return unityCanvas;
            }
        }

        protected override void Awake()
        {
            base.Awake();

            raycaster = GetComponent<GraphicRaycaster>();
        }

        private void OnEnable()
        {
            UIElementEventSystem.RegisterCanvas(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            UIElementEventSystem.DeregisterCanvas(this);
        }
    }
}
