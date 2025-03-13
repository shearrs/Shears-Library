using SH.Combat.HitDetection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestHitReceiver : MonoBehaviour, IHitReceiver
{
    public Transform Transform => transform;

    public void OnHitReceived(HitData hitData)
    {
        Debug.Log("hit received!");
    }
}