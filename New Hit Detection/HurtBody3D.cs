using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

public class HurtBody3D : MonoBehaviour, ISHLoggable
{
    [field: Header("Logging")]
    [field: SerializeField]
    public SHLogLevels LogLevels { get; set; } = SHLogLevels.Log | SHLogUtil.Issues;

    [Header("Settings")]
    [SerializeField, Tooltip("The colliders for receiving hits.")]
    private List<Collider> colliders;

    public List<Collider> Colliders { get => colliders; set => colliders = value; }

    public event Action<HitData3D> HitReceived;

    public void OnHitReceived(HitData3D data)
    {
        this.Log("HurtBody3D received a hit.", SHLogLevels.Verbose);
        HitReceived?.Invoke(data);
    }
}
