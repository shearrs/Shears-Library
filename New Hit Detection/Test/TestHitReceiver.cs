using UnityEngine;

public class TestHitReceiver : MonoBehaviour
{
    [SerializeField] private HurtBody3D hurtBody;
    [SerializeField] private MeshRenderer meshRenderer;

    private Material material;
    private bool hit = false;

    private void Awake()
    {
        material = meshRenderer.material;
        material.color = Color.green;
    }

    private void OnEnable()
    {
        hurtBody.HitReceived += OnHitReceived;
    }

    private void OnDisable()
    {
        hurtBody.HitReceived -= OnHitReceived;
    }

    private void Update()
    {
        if (!hit)
            return;

        hit = false;
        material.color = Color.green;
    }

    private void OnHitReceived(HitData3D data)
    {
        hit = true;

        if (hurtBody.IsBlocking)
            material.color = Color.blue;
        else
            material.color = Color.red;
    }
}
