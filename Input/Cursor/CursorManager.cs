using UnityEngine;

namespace Shears.Input
{
    public class CursorManager
    {
        public static void SetCursorVisibility(bool visible)
        {
            Cursor.visible = visible;
        }

        public static void SetCursorLockMode(CursorLockMode lockMode)
        {
            Cursor.lockState = lockMode;
        }
    }
}
