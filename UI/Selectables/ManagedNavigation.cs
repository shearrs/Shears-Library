using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI
{
    public class ManagedNavigation
    {
        private readonly ManagedSelectable managedSelectable;
        private readonly Selectable selectable;

        public ManagedSelectable SelectOnUp 
        { 
            get => GetManagedSelectable(selectable.navigation.selectOnUp);
            set
            {
                var newNavigation = selectable.navigation;
                newNavigation.selectOnUp = value.Selectable;

                selectable.navigation = newNavigation;
            }
        }

        public ManagedSelectable SelectOnRight
        {
            get => GetManagedSelectable(selectable.navigation.selectOnRight);
            set
            {
                var newNavigation = selectable.navigation;
                newNavigation.selectOnRight = value.Selectable;

                selectable.navigation = newNavigation;
            }
        }

        public ManagedSelectable SelectOnDown
        {
            get => GetManagedSelectable(selectable.navigation.selectOnDown);
            set
            {
                var newNavigation = selectable.navigation;
                newNavigation.selectOnDown = value.Selectable;

                selectable.navigation = newNavigation;
            }
        }

        public ManagedSelectable SelectOnLeft
        {
            get => GetManagedSelectable(selectable.navigation.selectOnDown);
            set
            {
                var newNavigation = selectable.navigation;
                newNavigation.selectOnLeft = value.Selectable;

                selectable.navigation = newNavigation;
            }
        }

        public ManagedNavigation(ManagedSelectable managedSelectable)
        {
            this.managedSelectable = managedSelectable;
            selectable = this.managedSelectable.Selectable;
        }

        private ManagedSelectable GetManagedSelectable(Selectable selectable) => selectable.GetComponent<ManagedSelectable>();
    }
}
