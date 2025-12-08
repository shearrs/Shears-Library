using Shears.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class HitBody3D : MonoBehaviour, ISHLoggable
{
    [field: Header("Logging")]
    [field: SerializeField] public SHLogLevels LogLevels { get; set; }

    [Header("Hit Settings")]
    [SerializeField] private HitShape3D shape;
    [SerializeField] private bool fixedUpdate = false;
    [SerializeField] private bool multiHits;
    [SerializeField] protected LayerMask collisionMask = 1;
    [SerializeField] protected List<Collider> ignoreList;

    private bool isEnabled = false;
    private List<HurtBody3D> unclearedHits;
    private Dictionary<Collider, RaycastHit> finalHits;
    private List<int> sortedHits = new();

    public HitShape3D Shape { get => shape; set => shape = value; }
    public bool UseFixedUpdate { get => fixedUpdate; set => fixedUpdate = value; }
    public bool MultiHits { get => multiHits; set => multiHits = value; }
    public LayerMask CollisionMask { get => collisionMask; set => collisionMask = value; }
    public List<Collider> IgnoreList { get => ignoreList; set => ignoreList = value; }

    public event Action Enabled;
    public event Action Disabled;
    public event Action HitDelivered;

    private void Awake()
    {
        finalHits = DictionaryPool<Collider, RaycastHit>.Get();
        sortedHits = ListPool<int>.Get();
        unclearedHits = ListPool<HurtBody3D>.Get();
    }

    private void OnDestroy()
    {
        DictionaryPool<Collider, RaycastHit>.Release(finalHits);
        ListPool<int>.Release(sortedHits);
        ListPool<HurtBody3D>.Release(unclearedHits);
    }

    public void Enable()
    {
        if (isEnabled)
            return;

        isEnabled = true;
        Enabled?.Invoke();
    }

    public void Disable()
    {
        if (!isEnabled)
            return;

        isEnabled = false;
        Disabled?.Invoke();
    }

    private void Update()
    {
        if (!isEnabled || fixedUpdate)
            return;

        DetectHits();
    }

    private void FixedUpdate()
    {
        if (!isEnabled || !fixedUpdate)
            return;

        DetectHits();
    }

    private void DetectHits()
    {
        if (shape == null)
        {
            this.Log("HitShape3D is not assigned.", SHLogLevels.Issues);
            return;
        }

        var hitRays = shape.Sweep();

        foreach (var hitCollection in hitRays)
            ValidateHits(hitCollection);

        DeliverHits();
    }

    private void ValidateHits(RaycastHit[] hits)
    {
        sortedHits.Clear();

        for (int i = 0; i < hits.Length; i++)
            sortedHits.Add(i);

        sortedHits.Sort((h1, h2) => hits[h1].distance.CompareTo(hits[h2].distance));

        foreach (var hitIndex in sortedHits)
        {
            RaycastHit hit = hits[hitIndex];

            if (hit.collider == null)
                continue;

            if (finalHits.TryGetValue(hit.collider, out var oldHit))
            {
                if (oldHit.distance < hit.distance)
                    finalHits[hit.collider] = hit;
            }
            else
                finalHits[hit.collider] = hit;
        }
    }

    private void DeliverHits()
    {

    }
}
