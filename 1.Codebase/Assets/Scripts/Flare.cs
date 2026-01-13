using System.Collections.Generic;
using UnityEngine;

public class Flare : MonoBehaviour
{
    [SerializeField]
    float lifetime = 5f;

    [SerializeField]
    float heatSignature = 1f;

    [SerializeField]
    bool enableDebugLogs = true;

    static List<Flare> activeFlares = new List<Flare>();

    float spawnTime;
    Rigidbody rb;

    public Vector3 Position => rb != null ? rb.position : transform.position;
    public Vector3 Velocity => rb != null ? rb.linearVelocity : Vector3.zero;

    public float HeatSignature
    {
        get
        {
            float age = Time.time - spawnTime;
            float normalizedAge = age / lifetime;

            return heatSignature * (1f - Mathf.Sqrt(normalizedAge));
        }
    }

    public static IReadOnlyList<Flare> ActiveFlares => activeFlares;

    void Start()
    {
        spawnTime = Time.time;
        rb = GetComponent<Rigidbody>();
        activeFlares.Add(this);
        if (enableDebugLogs)
        {
            Debug.Log($"[Flare] Spawned at {Position}, activeFlares count: {activeFlares.Count}");
        }
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[Flare] Expired after {lifetime}s, destroying");
            }
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        activeFlares.Remove(this);
        if (enableDebugLogs)
        {
            Debug.Log($"[Flare] Destroyed, activeFlares count: {activeFlares.Count}");
        }
    }

    public static Flare GetNearestFlare(Vector3 position, float maxDistance)
    {
        Flare nearest = null;
        float nearestDist = maxDistance;

        foreach (var flare in activeFlares)
        {
            if (flare == null)
                continue;

            float dist = Vector3.Distance(position, flare.Position);
            if (dist < nearestDist && flare.HeatSignature > 0.1f)
            {
                nearestDist = dist;
                nearest = flare;
            }
        }

        return nearest;
    }
}
