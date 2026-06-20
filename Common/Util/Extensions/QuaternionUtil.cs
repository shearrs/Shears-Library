using UnityEngine;

namespace Shears
{
    public static class QuaternionUtil
    {
        public static Quaternion SmoothDamp(Quaternion current, Quaternion target, ref Vector3 currentVelocity, float smoothTime)
        {
            if (Time.deltaTime == 0) return current;
            if (smoothTime == 0) return target;

            Vector3 c = current.eulerAngles;
            Vector3 t = target.eulerAngles;

            return Quaternion.Euler(
                Mathf.SmoothDampAngle(c.x, t.x, ref currentVelocity.x, smoothTime),
                Mathf.SmoothDampAngle(c.y, t.y, ref currentVelocity.y, smoothTime),
                Mathf.SmoothDampAngle(c.z, t.z, ref currentVelocity.z, smoothTime)
            );
        }

        public static Vector3 SmoothDampEuler(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime)
        {
            return new Vector3(
                Mathf.SmoothDampAngle(current.x, target.x, ref currentVelocity.x, smoothTime),
                Mathf.SmoothDampAngle(current.y, target.y, ref currentVelocity.y, smoothTime),
                Mathf.SmoothDampAngle(current.z, target.z, ref currentVelocity.z, smoothTime)
            );
        }

        /// <summary>
        /// Get the rotation that would be applied to 'start' to end up at 'end'.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Quaternion FromToRotation(Quaternion start, Quaternion end)
        {
            return Quaternion.Inverse(start) * end;
        }
    }
}
