using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.GraphViews.Editor
{
    public class ContentSelector : MouseManipulator
    {
        private readonly GraphView graphView;

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
            if (evt.target is ISelectable selectable)
                graphView.Select(selectable, IsMultiSelect(evt.modifiers));
            else
                graphView.Select(null);

            evt.StopImmediatePropagation();
        }

        private bool IsMultiSelect(EventModifiers modifiers) => modifiers == EventModifiers.Shift || modifiers == EventModifiers.Control;
    }
}
