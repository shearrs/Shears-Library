using Shears.Logging;
using UnityEngine;

namespace Shears.UI
{
    public abstract class UIManipulator : IUIManipulator, ISHLoggable
    {
        public SHLogLevels LogLevels
        {
            get => Element.LogLevels;
            set => Element.LogLevels = value;
        }
        protected UIElement Element { get; private set; }
        public bool IsEnabled { get; private set; }

        public UIManipulator(UIElement element)
        {
            Element = element;
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            RegisterEvents();
            IsEnabled = true;
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            DeregisterEvents();
            IsEnabled = false;
        }

        protected abstract void RegisterEvents();
        protected abstract void DeregisterEvents();
    }
}
