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
            var eventsProp = property.FindPropertyRelative("unityEvents");

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

            root.AddAll(
                new PropertyField(usesDataObjectProp), new PropertyField(dataObjectProp), 
                new PropertyField(durationProp), new PropertyField(forceFinalValueProp), 
                new PropertyField(loopsProp), new PropertyField(loopModeProp), 
                new PropertyField(usesCurveProp), new PropertyField(easingFunctionProp), 
                new PropertyField(curveProp), new PropertyField(eventsProp));

            return root;
        }
    }
}
