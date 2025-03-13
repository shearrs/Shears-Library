using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SH.Combat.HitDetection;

public class TestHitDeliverer : MonoBehaviour, IHitDeliverer
{
    private Vector3 hitPoint;

    public Transform Transform => transform;

    public void OnHitDelivered(HitData hitData)
    {
        hitPoint = hitData.hit.point;
    }

    private void OnDrawGizmos()
    {
        if (hitPoint == Vector3.zero)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hitPoint, .025f);
    }
}