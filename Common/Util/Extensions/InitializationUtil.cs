using UnityEngine;

namespace Shears
{
    internal class InactiveParent : PersistentProtectedSingleton<InactiveParent>
    {
        public static Transform Transform => Instance.transform;

        protected override void Awake()
        {
            base.Awake();

            gameObject.SetActive(false);
        }
    }

    public static class InitializationUtil
    {
        public static T InstantiateInactive<T>(T prefab, Transform parent = null)
            where T : Component
        {
            var scale = prefab.transform.localScale;
            var rotation = prefab.transform.localRotation;
            var instance = Object.Instantiate(prefab, InactiveParent.Transform);
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(parent);
            instance.transform.localScale = scale;
            instance.transform.localRotation = rotation;

            return instance;
        }
    }
}
