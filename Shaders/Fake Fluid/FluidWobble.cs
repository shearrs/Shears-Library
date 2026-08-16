using UnityEngine;

public class FluidWobble : MonoBehaviour
{
    [SerializeField]
    private Renderer renderer;

    [SerializeField]
    private float springiness = 1.0f;

    [SerializeField]
    private float damping = 1.0f;

    private Vector3 springPosition;
    private Vector3 velocity;

    private void Awake()
    {
        springPosition = transform.position;
    }

    private void Update()
    {
        var currentPosition = transform.position;
        var difference = currentPosition - springPosition;
        var distance = difference.magnitude;

        if (!Mathf.Approximately(distance, 0.0f))
        {
            var changeDirection = difference / distance;

            var springForce = springiness * distance * changeDirection;
            velocity += springForce;
        }

        var damping = Time.deltaTime * this.damping * -velocity;
        velocity += damping;

        springPosition += Time.deltaTime * velocity;

        renderer.material.SetVector(
            "_WobbleVelocity",
            new(velocity.x, velocity.y, velocity.z, 0.0f)
        );
    }
}
