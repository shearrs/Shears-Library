using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Shears.Tweens.Editor
{
    [CustomPropertyDrawer(typeof(StructTweenData))]
    public class StructTweenDataPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var usesDataObjectProp = property.FindPropertyRelative("usesDataObject");
            var dataObjectProp = property.FindPropertyRelative("tweenDataObject");
            var durationProp = property.FindPropertyRelative("duration");
            var forceFinalValueProp = property.FindPropertyRelative("forceFinalValue");
            var loopsProp = property.FindPropertyRelative("loops");
            var loopModeProp = property.FindPropertyRelative("loopMode");
            var easingFunctionProp = property.FindPropertyRelative("easingFunction");
            var usesCurveProp = property.FindPropertyRelative("usesCurve");
            var curveProp = property.FindPropertyRelative("curve");

            bool dontUseDataObject = !usesDataObjectProp.boolValue;
            bool hasNoDuration = durationProp.floatValue == 0f;
            bool dontForceFinalValue = !forceFinalValueProp.boolValue;
            bool dontLoop = loopsProp.intValue == 0;
            bool noLoopMode = loopModeProp.enumValueIndex == 0;
            bool linearEasing = easingFunctionProp.enumValueIndex == 0;

            if (dontUseDataObject && hasNoDuration && dontForceFinalValue && dontLoop && noLoopMode && linearEasing)
            {
                durationProp.floatValue = 1.0f;
                forceFinalValueProp.boolValue = true;
                curveProp.boxedValue = AnimationCurve.Linear(0, 0, 1, 1);

                property.serializedObject.ApplyModifiedProperties();
            }

            var root = new Foldout
            {
                text = property.displayName
            };

            root.Add(new PropertyField(usesDataObjectProp));
            root.Add(new PropertyField(dataObjectProp));
            root.Add(new PropertyField(durationProp));
            root.Add(new PropertyField(forceFinalValueProp));
            root.Add(new PropertyField(loopsProp));
            root.Add(new PropertyField(loopModeProp));
            root.Add(new PropertyField(easingFunctionProp));
            root.Add(new PropertyField(usesCurveProp));
            root.Add(new PropertyField(curveProp));

            return root;
        }
    }
}
