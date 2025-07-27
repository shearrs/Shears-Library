using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class ContentSelector : MouseManipulator
    {
        private const long CLICK_COOLDOWN = 100; // milliseconds

        private readonly GraphView graphView;
        private long previousTimestamp = -1;

        public ContentSelector(GraphView graphView)
        {
            this.graphView = graphView;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(Select);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(Select);
        }

        private void Select(MouseDownEvent evt)
        {
            if (evt.timestamp - previousTimestamp < CLICK_COOLDOWN)
            {
                previousTimestamp = evt.timestamp;
                return;
            }

            previousTimestamp = evt.timestamp;

            if (evt.target is ISelectable selectable)
                graphView.Select(selectable, IsMultiSelect(evt.modifiers));
            else
                graphView.Select(null);

            evt.StopPropagation();
        }

        private bool IsMultiSelect(EventModifiers modifiers) => modifiers == EventModifiers.Shift || modifiers == EventModifiers.Control;
    }
}
