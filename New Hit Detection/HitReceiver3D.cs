using System;
using UnityEngine;

public class HitReceiver3D : MonoBehaviour
{
    public event Action<HitData3D> HitReceived;

    internal void ReceiveHit(HitData3D data)
    {
        HitReceived?.Invoke(data);
    }
}
