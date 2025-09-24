using UnityEngine;

namespace Shears
{
    /// <summary>
    /// Forces the GameObject to always face the main camera. Has options to constrain rotations to avoid issues with Euler rotations.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool invertRotation = false;
        [SerializeField] private bool usesWorldUp = true;

        [Header("Constraints")]
        [SerializeField] private Bool3 axisConstraints = Bool3.False;
        
        private void Update()
        {
            Look();
        }

        private void Look()
        {
            Vector3 direction = (invertRotation) ?
                (transform.position - Camera.main.transform.position).normalized :
                (Camera.main.transform.position - transform.position).normalized;

            Vector3 up = (usesWorldUp) ?
                Vector3.up :
                transform.up;

            Vector3 targetRotation = Quaternion.LookRotation(direction, up).eulerAngles;

            if (axisConstraints.x)
                targetRotation.x = 0;
            if (axisConstraints.y)
                targetRotation.y = 0;
            if (axisConstraints.z)
                targetRotation.z = 0;

            transform.rotation = Quaternion.Euler(targetRotation);
        }
    }
}
