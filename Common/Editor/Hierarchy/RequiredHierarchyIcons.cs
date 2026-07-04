using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shears.Editor
{
    [InitializeOnLoad]
    public class RequiredHierarchyIcons
    {
        private const string ICON_PATH = "Shears Library/Symbols/hierarchyError";

        private static readonly Dictionary<
            Type,
            List<(string name, RequiredAttribute attribute)>
        > targetTypes = new();
        private static readonly Dictionary<GameObject, HashSet<Component>> targetComponents = new();
        private static readonly List<GameObject> rootObjects = new();
        private static readonly Texture2D errorSprite;
        private static readonly MethodInfo getExpandedIDs;
        private static readonly PropertyInfo lastInteractedProperty;
        private static readonly List<Component> components = new();

        static RequiredHierarchyIcons()
        {
            errorSprite = Resources.Load<Texture2D>(ICON_PATH);

            if (errorSprite == null)
            {
                Debug.LogError("Failed to load error sprite!");
                return;
            }

            var attributeFields = TypeCache.GetFieldsWithAttribute<RequiredAttribute>();

            foreach (var field in attributeFields)
            {
                if (!targetTypes.TryGetValue(field.DeclaringType, out var stringList))
                {
                    stringList = new();
                    targetTypes[field.DeclaringType] = stringList;
                }

                var attribute = field.GetCustomAttribute<RequiredAttribute>();

                stringList.Add((field.Name, attribute));
            }

            var hierarchyWindowType = typeof(EditorWindow).Assembly.GetType(
                "UnityEditor.SceneHierarchyWindow"
            );
            getExpandedIDs = hierarchyWindowType.GetMethod(
                "GetExpandedIDs",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            lastInteractedProperty = hierarchyWindowType.GetProperty(
                "lastInteractedHierarchyWindow",
                BindingFlags.Public | BindingFlags.Static
            );

            EditorApplication.hierarchyWindowItemOnGUI += DrawHierarchyItem;
            ObjectChangeEvents.changesPublished += OnObjectsChanged;
            EditorSceneManager.activeSceneChangedInEditMode += OnSceneLoaded;
            PrefabStage.prefabStageOpened += OnPrefabOpened;
            PrefabStage.prefabStageClosing += OnPrefabClosed;

            InitializeAllObjects();
        }

        ~RequiredHierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= DrawHierarchyItem;
            ObjectChangeEvents.changesPublished -= OnObjectsChanged;
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneLoaded;
            PrefabStage.prefabStageOpened -= OnPrefabOpened;
            PrefabStage.prefabStageClosing -= OnPrefabClosed;
        }

        // game object specifically changed: just refresh the components for that object
        // everything changed: refresh everything, stop looping
        private static void OnObjectsChanged(ref ObjectChangeEventStream stream)
        {
            if (Application.isPlaying)
                return;

            bool hierarchyChanged = false;

            for (int i = 0; i < stream.length; i++)
            {
                var type = stream.GetEventType(i);
                GameObject gameObject = null;

                switch (type)
                {
                    case ObjectChangeKind.CreateGameObjectHierarchy:
                        stream.GetCreateGameObjectHierarchyEvent(i, out var d1);
                        gameObject = EditorUtility.EntityIdToObject(d1.instanceId) as GameObject;
                        break;
                    case ObjectChangeKind.ChangeGameObjectStructureHierarchy:
                        stream.GetChangeGameObjectStructureHierarchyEvent(i, out var d2);
                        gameObject = EditorUtility.EntityIdToObject(d2.instanceId) as GameObject;
                        break;
                    case ObjectChangeKind.ChangeGameObjectStructure:
                        stream.GetChangeGameObjectStructureEvent(i, out var d3);
                        gameObject = EditorUtility.EntityIdToObject(d3.instanceId) as GameObject;
                        break;
                    case ObjectChangeKind.ChangeGameObjectParent:
                        stream.GetChangeGameObjectParentEvent(i, out var d4);
                        var previousParent =
                            EditorUtility.EntityIdToObject(d4.previousParentInstanceId)
                            as GameObject;

                        if (previousParent != null)
                            InitializeObject(previousParent);

                        gameObject = EditorUtility.EntityIdToObject(d4.instanceId) as GameObject;
                        break;
                    case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                        hierarchyChanged = true;
                        break;
                    case ObjectChangeKind.DestroyGameObjectHierarchy:
                        stream.GetDestroyGameObjectHierarchyEvent(i, out var d6);
                        gameObject =
                            EditorUtility.EntityIdToObject(d6.parentInstanceId) as GameObject;
                        break;
                }

                if (gameObject != null)
                {
                    InitializeObject(gameObject);
                    hierarchyChanged = true;
                }
            }

            if (hierarchyChanged)
                EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnSceneLoaded(Scene scene1, Scene scene2)
        {
            InitializeAllObjects();
        }

        private static void OnPrefabOpened(PrefabStage stage)
        {
            targetComponents.Clear();
            rootObjects.Clear();

            InitializeObject(stage.prefabContentsRoot);
        }

        private static void OnPrefabClosed(PrefabStage stage)
        {
            InitializeAllObjects();
        }

        private static void InitializeAllObjects()
        {
            targetComponents.Clear();
            rootObjects.Clear();

            var prefab = PrefabStageUtility.GetCurrentPrefabStage();

            if (prefab != null)
            {
                var root = prefab.prefabContentsRoot;

                rootObjects.Add(root);
            }
            else
            {
                for (int i = 0; i < EditorSceneManager.sceneCount; i++)
                {
                    var scene = EditorSceneManager.GetSceneAt(i);
                    var objects = scene.GetRootGameObjects();

                    rootObjects.AddRange(objects);
                }
            }

            foreach (var obj in rootObjects)
                InitializeObject(obj);
        }

        private static void InitializeObject(GameObject obj)
        {
            components.Clear();
            obj.GetComponentsInChildren(true, components);

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var componentType = component.GetType();

                if (!targetTypes.TryGetValue(componentType, out var _))
                    continue;

                if (!targetComponents.TryGetValue(component.gameObject, out var componentSet))
                {
                    componentSet = new();
                    targetComponents[component.gameObject] = componentSet;
                }

                componentSet.Add(component);
            }

            components.Clear();
            var parent = obj.transform.parent;

            if (parent == null)
                return;

            parent.GetComponentsInParent(true, components);

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var componentType = component.GetType();

                if (!targetTypes.TryGetValue(componentType, out var fields))
                    continue;

                if (!targetComponents.TryGetValue(component.gameObject, out var componentSet))
                {
                    componentSet = new();
                    targetComponents[component.gameObject] = componentSet;
                }

                componentSet.Add(component);
            }
        }

        private static void DrawHierarchyItem(int entityID, Rect selectionRect)
        {
            if (Application.isPlaying)
                return;

            var gameObject = EditorUtility.EntityIdToObject(entityID) as GameObject;

            if (gameObject == null)
                return;

            GetComponentsForObject(gameObject);

            foreach (var component in components)
            {
                if (component == null)
                    continue;

                var componentType = component.GetType();

                if (!targetTypes.TryGetValue(componentType, out var _))
                    continue;

                var serializedObject = new SerializedObject(component);

                var (shouldShowError, propertyPath) = ShouldShowError(
                    componentType,
                    s => serializedObject.FindProperty(s)
                );

                if (shouldShowError)
                {
                    float size = selectionRect.height;
                    var rect = new Rect(
                        new(selectionRect.x - (2.0f * size), selectionRect.y, size, size)
                    );

                    GUI.DrawTextureWithTexCoords(
                        rect,
                        errorSprite,
                        new Rect(Vector2.zero, Vector2.one)
                    );

                    var tooltip = new GUIContent(
                        "",
                        $"Property at path '{propertyPath}' needs to be assigned"
                    );

                    GUI.Box(rect, tooltip, GUIStyle.none);

                    break;
                }
            }
        }

        private static (bool shouldShowError, string propertyPath) ShouldShowError(
            Type type,
            Func<string, SerializedProperty> propertyGetter
        )
        {
            if (!targetTypes.TryGetValue(type, out var fields))
                return (false, "");

            foreach (var field in fields)
            {
                var prop = propertyGetter(field.name);
                bool hasAlternativeValue = false;

                if (prop == null)
                {
                    Debug.LogError($"Could not find property {field} in type {type.Name}!");
                    continue;
                }

                if (prop.isArray)
                {
                    var targetSize = field.attribute.TargetCollectionSize;

                    if (targetSize == -1)
                        continue;
                    else if (prop.arraySize >= targetSize)
                    {
                        var fieldType = prop.GetCollectionElementType();

                        for (int i = 0; i < prop.arraySize; i++)
                        {
                            var element = prop.GetArrayElementAtIndex(i);

                            var (shouldShow, path) = ShouldShowError(
                                fieldType,
                                (s) => element.FindPropertyRelative(s)
                            );

                            if (shouldShow)
                                return (shouldShow, path);
                        }

                        return (false, "");
                    }
                    else
                        return (true, prop.propertyPath);
                }

                var altValues = field.attribute.AlternativeValues;

                if (altValues != null && altValues.Length > 0)
                {
                    foreach (var altValue in altValues)
                    {
                        var altProp = propertyGetter(altValue);

                        if (altProp != null && altProp.boxedValue != null)
                        {
                            hasAlternativeValue = true;
                            break;
                        }
                    }
                }

                if (prop.propertyType == SerializedPropertyType.Generic)
                {
                    var propType = prop.boxedValue.GetType();

                    if (!targetTypes.TryGetValue(propType, out var genericFields))
                        continue;

                    return ShouldShowError(propType, s => prop.FindPropertyRelative(s));
                }
                else if (prop.boxedValue == null && !hasAlternativeValue)
                    return (true, prop.propertyPath);
            }

            return (false, "");
        }

        private static void GetComponentsForObject(GameObject gameObject)
        {
            components.Clear();

            if (targetComponents.TryGetValue(gameObject, out var objComponents))
                components.AddRange(objComponents);

            if (!IsExpandedInHierarchy(gameObject))
                GetComponentsForObjectRecursive(gameObject.transform);
        }

        private static void GetComponentsForObjectRecursive(Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                if (targetComponents.TryGetValue(child.gameObject, out var childComponents))
                    components.AddRange(childComponents);

                if (!IsExpandedInHierarchy(child.gameObject))
                    GetComponentsForObjectRecursive(child);
            }
        }

        public static bool IsExpandedInHierarchy(GameObject gameObject)
        {
            if (getExpandedIDs == null || lastInteractedProperty == null || gameObject == null)
                return false;

            var windowInstance = lastInteractedProperty.GetValue(null);

            if (windowInstance == null)
                return false;

            if (getExpandedIDs.Invoke(windowInstance, null) is not EntityId[] expandedIDs)
                return false;

            if (!expandedIDs.Contains(gameObject.GetEntityId()))
                return false;

            var transform = gameObject.transform;

            while (transform.parent != null)
            {
                var parent = transform.parent;
                int parentID = parent.gameObject.GetEntityId();

                if (!expandedIDs.Contains(parentID))
                    return false;

                transform = parent;
            }

            return true;
        }
    }
}
