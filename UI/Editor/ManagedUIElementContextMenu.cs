using Shears.Tweens;
using System;
using System.Reflection;
using TMPro;
using TreeEditor;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Shears.UI.Editor
{
    internal static class ManagedUIElementContextMenu
    {
        private static readonly string defaultTweenPath = "ManagedUI/Shears_DefaultUISelectTween";

        private static readonly string play1To2 = nameof(ImageTweener.Play1To2);
        private static readonly string play2To1 = nameof(ImageTweener.Play2To1);

        private static readonly Color initialColor = Color.white;
        private static readonly Color hoverColor = new(0.78f, 0.78f, 0.78f);
        private static readonly Color selectedColor = new(0.5f, 0.5f, 0.5f);

        [MenuItem("GameObject/ManagedUI/Simple Button", priority = 7)]
        private static void CreateSimpleButton(MenuCommand command)
        {
            var button = CreateButton();
            var uiElement = button.AddComponent<ManagedUIElement>();
            var (backgroundImageChild, backgroundImage) = CreateBackgroundImage(button.transform);

            var tweenData = Resources.Load<TweenData>(defaultTweenPath);

            if (tweenData == null)
            {
                Debug.LogWarning($"Could not find tween data with path: {defaultTweenPath}");
                return;
            }

            var tweenerParent = new GameObject("Tweens");
            tweenerParent.transform.SetParent(backgroundImageChild.transform, false);

            var (hoverTweener, focusTweener, selectTweener, hoverSelectTweener) = CreateTweeners(tweenerParent.transform, backgroundImage, tweenData);

            CreateEventBindings(uiElement, hoverTweener, focusTweener, selectTweener, hoverSelectTweener);

            var textGameObject = new GameObject("Text");
            textGameObject.transform.SetParent(button.transform, false);

            var textMesh = textGameObject.AddComponent<TextMeshProUGUI>();
            textMesh.text = "Button";
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;

            Selection.SetActiveObjectWithContext(button, null);
        }

        private static GameObject CreateButton()
        {
            var parent = Selection.activeGameObject;

            var button = new GameObject("Button", typeof(RectTransform));
            button.transform.SetParent(parent.transform);
            button.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            var rectTransform = button.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);

            return button;
        }

        private static (GameObject imageChild, Image image) CreateBackgroundImage(Transform parent)
        {
            var imageChild = new GameObject("Background Image", typeof(RectTransform));
            imageChild.transform.SetParent(parent, false);

            var rectTransform = imageChild.GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var image = imageChild.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;

            return (imageChild, image);
        }

        private static (ImageTweener hoverTweener, ImageTweener focusTweener, ImageTweener selectTweener, ImageTweener hoverSelectTweener) CreateTweeners(Transform parent, Image image, TweenData data)
        {
            var hoverTweener = CreateTweener("Hover Tweener", parent, image, data, initialColor, hoverColor);
            var focusTweener = CreateTweener("Focus Tweener", parent, image, data, initialColor, hoverColor);
            var selectTweener = CreateTweener("Select Tweener", parent, image, data, hoverColor, selectedColor);
            var hoverSelectTweener = CreateTweener("Hover Select Tweener", parent, image, data, initialColor, selectedColor);

            return (hoverTweener, focusTweener, selectTweener, hoverSelectTweener);
        }

        private static ImageTweener CreateTweener(string name, Transform parent, Image image, TweenData data, Color color1, Color color2)
        {
            var gameObject = new GameObject(name);
            gameObject.transform.SetParent(parent, false);

            var tweener = gameObject.AddComponent<ImageTweener>();

            tweener.TweenData = data;
            tweener.Image = image;
            tweener.Type = ImageTweener.TweenType.Color;
            tweener.Color1 = color1;
            tweener.Color2 = color2;

            return tweener;
        }

        private static void CreateEventBindings(ManagedUIElement uiElement, ImageTweener hoverTweener, ImageTweener focusTweener, ImageTweener selectTweener, ImageTweener hoverSelectTweener)
        {
            // this is the most insane thing ever
            // I literally never use this SerializedObject, but if I don't create it, the UnityEvents will not be instantiated yet...
            SerializedObject uiElementSO = new(uiElement);

            CreateEventBinding(uiElement, "onHoverBegin", hoverTweener, play1To2);
            CreateEventBinding(uiElement, "onHoverEnd", hoverTweener, play2To1);
            CreateEventBinding(uiElement, "onFocusBegin", focusTweener, play1To2);
            CreateEventBinding(uiElement, "onFocusEnd", focusTweener, play2To1);
            CreateEventBinding(uiElement, "onSelectBegin", selectTweener, play1To2);
            CreateEventBinding(uiElement, "onSelectEnd", selectTweener, play2To1);
            CreateEventBinding(uiElement, "onClickBegin", selectTweener, play1To2);
            CreateEventBinding(uiElement, "onClickEnd", selectTweener, play2To1);
            CreateEventBinding(uiElement, "onHoverBeginClicked", hoverSelectTweener, play1To2);
            CreateEventBinding(uiElement, "onHoverEndClicked", hoverSelectTweener, play2To1);
        }

        private static void CreateEventBinding(ManagedUIElement uiElement, string unityEventName, object target, string methodName)
        {
            UnityEvent unityEvent = GetUnityEvent(uiElement, unityEventName);

            var targetInfo = UnityEvent.GetValidMethodInfo(target, methodName, new System.Type[0]);
            UnityAction methodDelegate = Delegate.CreateDelegate(typeof(UnityAction), target, targetInfo) as UnityAction;
            UnityEventTools.AddPersistentListener(unityEvent, methodDelegate);
        }

        // freya my goddess.... (https://discussions.unity.com/t/unityevent-as-a-serialized-property/144948/3)
        private static UnityEvent GetUnityEvent(object obj, string fieldName)
        {
            if (obj != null)
            {
                FieldInfo fi = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                if (fi != null)
                {
                    return fi.GetValue(obj) as UnityEvent;
                }
            }
            return null;
        }
    }
}
