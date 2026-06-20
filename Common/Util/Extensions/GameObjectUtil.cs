using UnityEngine;

namespace Shears
{
    public static class GameObjectUtil
    {
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent(out T component))
                return component;
            else
                return gameObject.AddComponent<T>();
        }

        public static T GetOrAdd<T>(this MonoBehaviour monoBehaviour) where T : Component
        {
            if (monoBehaviour.TryGetComponent(out T component))
                return component;
            else
                return monoBehaviour.gameObject.AddComponent<T>();
        }
    }
}
