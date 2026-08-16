using Shears.Editor;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Shears.UI.Editor
{
    public static class UIElementsCreateMenu
    {
        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/UI Element",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 0
        )]
        private static void MenuCreateUIElement()
        {
            var gameObject = CreateGameObject("UI Element");
            gameObject.AddComponent<UIElement>();

            var parent = GetOrCreateParent();
            gameObject.transform.SetParent(parent.transform);

            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;

            Selection.activeGameObject = gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Image",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 1
        )]
        private static void MenuCreateManagedImage()
        {
            var gameObject = CreateGameObject("Image");
            gameObject.AddComponent<UIImage>();

            var parent = GetOrCreateParent();
            gameObject.transform.SetParent(parent.transform);

            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;

            Selection.activeGameObject = gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Button",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 2
        )]
        private static void MenuCreateButton()
        {
            var gameObject = CreateGameObject("Button");
            var button = gameObject.AddComponent<UIButton>();

            var image = CreateGameObject("Image").AddComponent<UIImage>();
            image.transform.SetParent(gameObject.transform);

            button.AddGraphic(image);
            image.Sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.RawImage.type = Image.Type.Sliced;

            var imageRect = image.GetComponent<RectTransform>();
            imageRect.anchorMin = Vector2.zero;
            imageRect.anchorMax = Vector2.one;
            imageRect.offsetMin = Vector2.zero;
            imageRect.offsetMax = Vector2.zero;

            var parent = GetOrCreateParent();
            if (parent.GetComponentInParent<UIElementCanvas>())
            {
                if (!gameObject.TryGetComponent<RectTransform>(out var _))
                    gameObject.AddComponent<RectTransform>();
            }

            gameObject.transform.SetParent(parent.transform);

            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;

            Selection.activeGameObject = gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Text Mesh",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 3
        )]
        private static void MenuCreateTextMesh()
        {
            var gameObject = CreateGameObject("Text");
            var text = gameObject.AddComponent<UIText>();

            text.Text = "Text";

            var parent = GetOrCreateParent();
            if (parent.GetComponentInParent<UIElementCanvas>())
            {
                if (!gameObject.TryGetComponent<RectTransform>(out var _))
                    gameObject.AddComponent<RectTransform>();
            }

            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;

            Selection.activeGameObject = gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Text Mesh UGUI",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 4
        )]
        private static void MenuCreateTextMeshUGUI()
        {
            var gameObject = CreateGameObject("Text");
            var text = gameObject.AddComponent<UITextGUI>();

            text.Text = "Text";

            var parent = GetOrCreateParent();
            if (parent.GetComponentInParent<UIElementCanvas>())
            {
                if (!gameObject.TryGetComponent<RectTransform>(out var _))
                    gameObject.AddComponent<RectTransform>();
            }

            gameObject.transform.SetParent(parent.transform);
            gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            gameObject.transform.localScale = Vector3.one;

            Selection.activeGameObject = gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Canvas",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 15
        )]
        private static void MenuCreateUIElementCanvas()
        {
            var canvas = CreateUICanvas();

            Selection.activeGameObject = canvas.gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Event System",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 100
        )]
        private static void MenuCreateUIElementEventSystem()
        {
            var eventSystem = CreateEventSystem();

            Selection.activeGameObject = eventSystem.gameObject;
        }

        [MenuItem(
            CreateMenuUtility.LIBRARY_PATH + "/UI Elements/Convert to UI Hierarchy",
            priority = CreateMenuUtility.LIBRARY_PRIORITY,
            secondaryPriority = 110
        )]
        private static void MenuConvertToUIHierarchy()
        {
            var selection = Selection.activeGameObject;

            if (selection == null)
                return;

            ConvertToUIHierarchyRecursive(selection.transform);
        }

        private static void ConvertToUIHierarchyRecursive(Transform transform)
        {
            ConvertToUIElement(transform.gameObject);

            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                ConvertToUIHierarchyRecursive(child);
            }
        }

        private static void ConvertToUIElement(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out UIElement _))
                return;
            else if (gameObject.TryGetComponent(out SpriteRenderer _))
                gameObject.AddComponent<UISprite>();
            else if (gameObject.TryGetComponent(out Image _))
                gameObject.AddComponent<UIImage>();
            else if (gameObject.TryGetComponent(out TextMeshPro _))
                gameObject.AddComponent<UIText>();
            else if (gameObject.TryGetComponent(out TextMeshProUGUI _))
                gameObject.AddComponent<UITextGUI>();
            else
                gameObject.AddComponent<UIElement>();

            EditorUtility.SetDirty(gameObject);
        }

        private static void CreateEventSystemIfNecessary(UIElementEventSystem.DetectionTypes type)
        {
            var eventSystems = Object.FindObjectsByType<UIElementEventSystem>(
                FindObjectsInactive.Include
            );
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
                CreateEventSystem();
        }

        private static GameObject CreateGameObject(string name)
        {
            var gameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + name);

            return gameObject;
        }

        private static GameObject GetOrCreateParent()
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            var selection = Selection.activeGameObject;

            if (stage != null)
            {
                if (selection != null)
                    return selection;

                return stage.prefabContentsRoot;
            }

            if (selection != null)
            {
                var canvas = selection.GetComponentInParent<UIElementCanvas>(true);
                Undo.RegisterCreatedObjectUndo(canvas, "Create UI Element Canvas");

                if (canvas != null)
                    return selection;
            }

            return CreateUICanvas().gameObject;
        }

        private static UIElementEventSystem CreateEventSystem()
        {
            var gameObject = CreateGameObject("UI Element Event System");
            var eventSystem = gameObject.AddComponent<UIElementEventSystem>();

            return eventSystem;
        }

        private static UIElementCanvas CreateUICanvas()
        {
            var gameObject = CreateGameObject("UI Element Canvas");
            gameObject.layer = LayerMask.NameToLayer("UI");

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

            CreateEventSystemIfNecessary(UIElementEventSystem.DetectionTypes.Canvas);

            return uiCanvas;
        }
    }
}
