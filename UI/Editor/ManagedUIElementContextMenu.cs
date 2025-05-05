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
        private static readonly Color focusColor = Color.black;
        private static readonly Color unfocusColor = new(0, 0, 0, 0);

        private struct TweenerGroup
        {
            public ImageTweener SelectTweener { get; set; }
            public ImageTweener ClickTweener { get; set; }
            public ImageTweener FocusTweener { get; set; }
            public ImageTweener HoverTweener { get; set; }
            public ImageTweener HoverSelectTweener { get; set; }

            public TweenerGroup(ImageTweener hoverTweener, ImageTweener clickTweener, ImageTweener focusTweener, ImageTweener selectTweener, ImageTweener hoverSelectTweener)
            {
                HoverTweener = hoverTweener;
                ClickTweener = clickTweener;
                FocusTweener = focusTweener;
                SelectTweener = selectTweener;
                HoverSelectTweener = hoverSelectTweener;
            }
        }

        private class TweenerData
        {
            public string Name { get; set; }
            public Transform Parent { get; set; }
            public Image Image { get; set; }
            public TweenData Data { get; set; }

            public TweenerData(string name, Transform parent, Image image, TweenData data)
            {
                Name = name;
                Parent = parent;
                Image = image;
                Data = data;
            }
        }

        [MenuItem("GameObject/ManagedUI/Simple Button", priority = 7)]
        private static void CreateSimpleButton(MenuCommand command)
        {
            var button = CreateButton();
            var uiElement = button.AddComponent<ManagedUIElement>();
            var (backgroundImageChild, backgroundImage) = CreateBackgroundImage(button.transform);
            var (focusImageChild, focusImage) = CreateFocusImage(button.transform);

            var tweenData = Resources.Load<TweenData>(defaultTweenPath);

            if (tweenData == null)
            {
                Debug.LogWarning($"Could not find tween data with path: {defaultTweenPath}");
                return;
            }

            var backgroundImageTweenerParent = new GameObject("Tweens");
            backgroundImageTweenerParent.transform.SetParent(backgroundImageChild.transform, false);

            var focusImageTweenerParent = new GameObject("Tweens");
            focusImageTweenerParent.transform.SetParent(focusImageChild.transform, false);

            var tweeners = CreateTweeners(backgroundImageTweenerParent.transform, focusImageTweenerParent.transform, backgroundImage, focusImage, tweenData);

            CreateEventBindings(uiElement, tweeners);

            CreateText(button.transform);

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

        private static (GameObject focusImageChild, Image focusImage) CreateFocusImage(Transform parent)
        {
            var focusImageChild = new GameObject("Focus Highlight", typeof(RectTransform));
            focusImageChild.transform.SetParent(parent, false);

            var rectTransform = focusImageChild.GetComponent<RectTransform>();

            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var focusImage = focusImageChild.AddComponent<Image>();
            focusImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            focusImage.type = Image.Type.Sliced;
            focusImage.color = unfocusColor;
            focusImage.fillCenter = false;
            focusImage.raycastTarget = false;

            return (focusImageChild, focusImage);
        }

        private static GameObject CreateText(Transform parent)
        {
            var textGameObject = new GameObject("Text");
            textGameObject.transform.SetParent(parent, false);

            var textMesh = textGameObject.AddComponent<TextMeshProUGUI>();
            textMesh.text = "Button";
            textMesh.color = Color.black;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.raycastTarget = false;

            return textGameObject;
        }

        private static TweenerGroup CreateTweeners(Transform backgroundParent, Transform focusParent, Image backgroundImage, Image focusHighlight, TweenData data)
        {
            var mainImageData = new TweenerData("", backgroundParent, backgroundImage, data);
            var focusImageData = new TweenerData("Focus Tweener", focusParent, focusHighlight, data);

            mainImageData.Name = "Select Tweener";
            var selectTweener = CreateTweener(mainImageData, initialColor, selectedColor);

            mainImageData.Name = "Click Tweener";
            var clickTweener = CreateTweener(mainImageData, hoverColor, selectedColor);

            mainImageData.Name = "Hover Tweener";
            var hoverTweener = CreateTweener(mainImageData, initialColor, hoverColor);

            mainImageData.Name = "Hover Select Tweener";
            var hoverSelectTweener = CreateTweener(mainImageData, initialColor, selectedColor);

            var focusTweener = CreateTweener(focusImageData, unfocusColor, focusColor);

            return new(hoverTweener, clickTweener, focusTweener, selectTweener, hoverSelectTweener);
        }

        private static ImageTweener CreateTweener(TweenerData data, Color color1, Color color2)
        {
            var gameObject = new GameObject(data.Name);
            gameObject.transform.SetParent(data.Parent, false);

            var tweener = gameObject.AddComponent<ImageTweener>();

            tweener.TweenData = data.Data;
            tweener.Image = data.Image;
            tweener.Type = ImageTweener.TweenType.Color;
            tweener.Color1 = color1;
            tweener.Color2 = color2;

            return tweener;
        }

        private static void CreateEventBindings(ManagedUIElement uiElement, TweenerGroup tweeners)
        {
            // this is the most insane thing ever
            // I literally never use this SerializedObject, but if I don't create it, the UnityEvents will not be instantiated yet...
            SerializedObject uiElementSO = new(uiElement);

            CreateEventBinding(uiElement, "onHoverBegin", tweeners.HoverTweener, play1To2);
            CreateEventBinding(uiElement, "onHoverEnd", tweeners.HoverTweener, play2To1);
            CreateEventBinding(uiElement, "onFocusBegin", tweeners.FocusTweener, play1To2);
            CreateEventBinding(uiElement, "onFocusEnd", tweeners.FocusTweener, play2To1);
            CreateEventBinding(uiElement, "onSelectBegin", tweeners.SelectTweener, play1To2);
            CreateEventBinding(uiElement, "onSelectEnd", tweeners.SelectTweener, play2To1);
            CreateEventBinding(uiElement, "onClickBegin", tweeners.ClickTweener, play1To2);
            CreateEventBinding(uiElement, "onClickEnd", tweeners.ClickTweener, play2To1);
            CreateEventBinding(uiElement, "onHoverBeginClicked", tweeners.HoverSelectTweener, play1To2);
            CreateEventBinding(uiElement, "onHoverEndClicked", tweeners.HoverSelectTweener, play2To1);
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
