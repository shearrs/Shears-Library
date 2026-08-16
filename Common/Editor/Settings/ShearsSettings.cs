using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    [FilePath("Shears Settings/ShearsSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class ShearsSettings : ScriptableSingleton<ShearsSettings>
    {
        [SerializeField]
        private bool hidePrefabChildren = true;

        [SerializeField]
        private bool hideRequiredIcons = false;

        public bool HidePrefabChildren => hidePrefabChildren;
        public bool HideRequiredIcons => hideRequiredIcons;

        public void TogglePrefabHiding()
        {
            hidePrefabChildren = !hidePrefabChildren;
            Save(true);
        }

        public void ToggleHideIcons()
        {
            hideRequiredIcons = !hideRequiredIcons;
            Save(true);
        }
    }

    internal static class ShearsSettingsWindow
    {
        [MenuItem("Shears Library/Settings/Toggle Prefab Hiding")]
        private static void TogglePrefabHiding()
        {
            ShearsSettings.instance.TogglePrefabHiding();
        }

        [MenuItem("Shears Library/Settings/Toggle Required Icons")]
        private static void ToggleRequiredIcons()
        {
            ShearsSettings.instance.ToggleHideIcons();
        }
    }
}
