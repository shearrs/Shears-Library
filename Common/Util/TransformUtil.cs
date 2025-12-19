using UnityEngine;

namespace Shears
{
    public static class TransformUtil
    {
        public static void SetLayerOnAllChildren(this GameObject gameObject, int layer)
            => SetLayerOnAllChildren(gameObject.transform, layer);

        public static void SetLayerOnAllChildren(this Transform transform, int layer)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                child.gameObject.layer = layer;

                SetLayerOnAllChildren(child, layer);
            }
        }
    }
}
