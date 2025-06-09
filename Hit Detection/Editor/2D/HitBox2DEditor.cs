using UnityEditor;
using UnityEngine;

namespace Shears.HitDetection.Editor
{
    [CustomEditor(typeof(HitBox2D))]
    public class HitBox2DEditor : UnityEditor.Editor
    {
        [MenuItem("CONTEXT/HitBox2D/Reset Gizmo Settings")]
        private static void ResetGizmoSettings(MenuCommand command)
        {
            var hitbox = (HitBox2D)command.context;

            hitbox.ResetGizmoSettings();
        }
    }
}
