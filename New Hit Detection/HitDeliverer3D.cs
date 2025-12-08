using System;
using UnityEngine;

public class HitDeliverer3D : MonoBehaviour
{
    public event Action<HitData3D> HitDelivered;

    internal void OnHitDelivered(HitData3D data)
    {
        HitDelivered?.Invoke(data);
    }
}
