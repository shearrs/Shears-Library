using UnityEditor;
using UnityEngine;

namespace Shears.HitDetection.Editor
{
    [CustomEditor(typeof(HitBox3D))]
    public class HitBox3DEditor : UnityEditor.Editor
    {
        //[MenuItem("CONTEXT/HitBox3D/Reset Gizmo Settings")]
        //private static void ResetGizmoSettings(MenuCommand command)
        //{
        //    var hitbox = (HitBox3D)command.context;

        //    hitbox.ResetGizmoSettings();
        //}
    }
}
