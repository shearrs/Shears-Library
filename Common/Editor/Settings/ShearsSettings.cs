using UnityEditor;
using UnityEngine;

namespace Shears.Editor
{
    [FilePath("Shears Settings/ShearsSettings.asset", FilePathAttribute.Location.PreferencesFolder)]
    public class ShearsSettings : ScriptableSingleton<ShearsSettings>
    {
        [SerializeField]
        private bool hidePrefabChildren = true;

        public bool HidePrefabChildren => hidePrefabChildren;

        public void TogglePrefabHiding()
        {
            hidePrefabChildren = !hidePrefabChildren;
            Save(true);
        }
    }

    internal static class ShearsSettingsWindow
    {
        [MenuItem("Shears Library/Settings/Toggle Prefab Hiding")]
        private static void Open()
        {
            ShearsSettings.instance.TogglePrefabHiding();
        }
    }
}
