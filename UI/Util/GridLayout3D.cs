using UnityEngine;

namespace Shears.UI
{
    [ExecuteAlways]
    public class GridLayout3D : MonoBehaviour
    {
        [SerializeField]
        private Vector3Int dimensions = Vector3Int.one;

        [SerializeField]
        private float xSpacing = 0.1f;

        [SerializeField]
        private float ySpacing = 0.1f;

        [SerializeField]
        private float zSpacing = 0.1f;

        private void OnValidate()
        {
            if (dimensions.x < 0)
                dimensions.x = 1;
            if (dimensions.y < 0)
                dimensions.y = 1;
            if (dimensions.z < 0)
                dimensions.z = 1;
        }

        private void Update()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);

                float xOffset = i % dimensions.x * xSpacing;
                float yOffset = Mathf.Floor(i / dimensions.x) * ySpacing;
                float zOffset = Mathf.Floor(i / (dimensions.x * dimensions.y)) * zSpacing;

                Vector3 offset = transform.TransformPoint(new Vector3(xOffset, yOffset, zOffset));

                child.transform.position = offset;
            }
        }
    }
}
