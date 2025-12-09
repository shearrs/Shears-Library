using UnityEngine;

public readonly struct HitResult3D
{
    private readonly Vector3 point;
    private readonly Vector3 normal;
    private readonly float distance;
    private readonly Transform transform;
    private readonly Collider collider;

    public readonly Vector3 Point => point;
    public readonly Vector3 Normal => normal;
    public readonly float Distance => distance;
    public readonly Transform Transform => transform;
    public readonly Collider Collider => collider;

    public HitResult3D(RaycastHit hit)
    {
        point = hit.point;
        normal = hit.normal;
        distance = hit.distance;
        transform = hit.collider.transform;
        collider = hit.collider;
    }
}
