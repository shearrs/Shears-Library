using UnityEngine;

namespace Shears
{
    public class ControlledTransform : MonoBehaviour
    {
        [SerializeField]
        private bool controlPosition;

        [SerializeField, ShowIf(nameof(controlPosition))]
        private Vector3 position;

        [SerializeField]
        private bool controlRotation;

        [SerializeField, ShowIf(nameof(controlRotation))]
        private Vector3 rotation;

        private void Update()
        {
            if (controlPosition)
                transform.position = position;
            if (controlRotation)
                transform.rotation = Quaternion.Euler(rotation);
        }
    }
}
