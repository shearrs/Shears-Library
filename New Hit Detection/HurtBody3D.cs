using System;
using UnityEngine;

public class HurtBody3D : MonoBehaviour
{
    public event Action HitReceived;

    public void Hit()
    {
        HitReceived?.Invoke();
    }
}
