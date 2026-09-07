using UnityEngine;

public sealed class EnergyCell : MonoBehaviour
{
    public Transform Visual;
    public Renderer[] GlowRenderers;
    public float Charge = 0.55f;
    public float RespawnDelay = 16f;
    float availableAt;
    Vector3 rest;
    MaterialPropertyBlock block;
    void Awake() { rest = Visual.localPosition; block = new MaterialPropertyBlock(); }
    void Update()
    {
        bool available = Time.time >= availableAt;
        Visual.gameObject.SetActive(available);
        if (!available) return;
        Visual.localPosition = rest + Vector3.up * (Mathf.Sin(Time.time * 2f + transform.position.x) * 0.08f);
        Visual.Rotate(0f, 35f * Time.deltaTime, 0f, Space.Self);
        block.SetColor("_EmissionColor", new Color(0.12f, 0.85f, 1f) * (1.2f + 0.25f * Mathf.Sin(Time.time * 3f)));
        foreach (var renderer in GlowRenderers) renderer.SetPropertyBlock(block);
    }
    void OnTriggerStay(Collider other)
    {
        if (Time.time < availableAt) return;
        var controller = other.attachedRigidbody != null ? other.attachedRigidbody.GetComponent<BbRigidbodyController>() : null;
        if (controller != null && controller.AddEnergy(Charge)) availableAt = Time.time + RespawnDelay;
    }
}
