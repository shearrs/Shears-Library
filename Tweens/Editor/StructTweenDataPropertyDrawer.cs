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
            var isExpandedProp = property.FindPropertyRelative("isExpanded");

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

            root.BindProperty(isExpandedProp);

            var dataObjectField = new PropertyField(dataObjectProp);
            var durationField = new PropertyField(durationProp);
            var forceFinalValueField = new PropertyField(forceFinalValueProp);
            var loopsField = new PropertyField(loopsProp);
            var loopModeField = new PropertyField(loopModeProp);
            var usesCurveField = new PropertyField(usesCurveProp);
            var easingFunctionField = new PropertyField(easingFunctionProp);
            var curveField = new PropertyField(curveProp);

            var settingsContainer = new VisualElement();
            settingsContainer.AddAll(
                dataObjectField, durationField, forceFinalValueField,
                loopsField, loopModeField
            );

            var curveContainer = new VisualElement();
            curveContainer.AddAll(
                usesCurveField,
                easingFunctionField,
                curveField
            );

            void evaluateUsesDataObject(SerializedProperty usesObject)
            {
                dataObjectField.style.display = usesObject.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
                durationField.style.display = usesObject.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                forceFinalValueField.style.display = usesObject.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                loopsField.style.display = usesObject.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                loopModeField.style.display = usesObject.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                curveContainer.style.display = usesObject.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            }

            void evaluateUsesCurve(SerializedProperty prop)
            {
                easingFunctionField.style.display = prop.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                curveField.style.display = prop.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
            }

            settingsContainer.TrackPropertyValue(usesDataObjectProp, evaluateUsesDataObject);
            curveContainer.TrackPropertyValue(usesCurveProp, evaluateUsesCurve);

            evaluateUsesCurve(usesCurveProp);
            evaluateUsesDataObject(usesDataObjectProp);

            root.AddAll(
                new PropertyField(usesDataObjectProp),
                settingsContainer, curveContainer
            );

            return root;
        }
    }
}
