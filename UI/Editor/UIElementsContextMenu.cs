using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI.Editor
{
    public static class UIElementsContextMenu
    {
        [MenuItem("GameObject/Shears Library/UI Elements/Image", secondaryPriority = 0)]
        private static void MenuCreateManagedImage()
        {
            var gameObject = new GameObject("Image");
            gameObject.AddComponent<ManagedImage>();

            var parent = GetOrCreateCanvas();
            gameObject.transform.SetParent(parent.transform);

            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;
        }

        [MenuItem("GameObject/Shears Library/UI Elements/Button", secondaryPriority = 1)]
        private static void MenuCreateCanvasButton()
        {
            var gameObject = new GameObject("Button");
            gameObject.AddComponent<CanvasRenderer>();
            var button = gameObject.AddComponent<CanvasButton>();
            var image = gameObject.AddComponent<ManagedImage>();

            button.Image = image;
            image.Sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.RawImage.type = Image.Type.Sliced;

            var parent = GetOrCreateCanvas();
            gameObject.transform.SetParent(parent.transform);

            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;
        }

        [MenuItem("GameObject/Shears Library/UI Elements/Canvas", priority = 2, secondaryPriority = 10)]
        private static void MenuCreateUIElementCanvas()
        {
            CreateUICanvas();
        }

        [MenuItem("GameObject/Shears Library/UI Elements/Event System", secondaryPriority = 100)]
        private static void MenuCreateUIElementEventSystem()
        {
            var gameObject = new GameObject("UI Element Event System");
            gameObject.AddComponent<UIElementEventSystem>();
        }

        private static void CreateEventSystemIfNecessary(UIElementEventSystem.DetectionType type)
        {
            var eventSystems = GameObject.FindObjectsByType<UIElementEventSystem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool targetSystem = false;

            foreach (var eventSystem in eventSystems)
            {
                if (eventSystem.SystemType == type)
                {
                    targetSystem = true;
                    break;
                }
            }

            if (!targetSystem)
                MenuCreateUIElementEventSystem();
        }

        private static UIElementCanvas GetOrCreateCanvas()
        {
            if (Selection.activeGameObject != null)
            {
                var activeGameObject = Selection.activeGameObject;
                var canvas = activeGameObject.GetComponentInParent<UIElementCanvas>(true);

                if (canvas != null)
                    return canvas;
            }

            return CreateUICanvas();
        }

        private static UIElementCanvas CreateUICanvas()
        {
            var gameObject = new GameObject("UI Element Canvas")
            {
                layer = LayerMask.NameToLayer("UI")
            };

            var canvas = gameObject.AddComponent<Canvas>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            gameObject.AddComponent<GraphicRaycaster>();
            var uiCanvas = gameObject.AddComponent<UIElementCanvas>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new(1920, 1080);
            scaler.matchWidthOrHeight = 0.45f;

            if (Selection.activeGameObject != null)
                gameObject.transform.SetParent(Selection.activeGameObject.transform);

            CreateEventSystemIfNecessary(UIElementEventSystem.DetectionType.Canvas);

            return uiCanvas;
        }
    }
}
