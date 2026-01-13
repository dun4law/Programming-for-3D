using UnityEngine;

public class Countermeasures : MonoBehaviour
{
    [Header("Flare Settings")]
    [SerializeField]
    private GameObject flarePrefab;

    [SerializeField]
    private Transform[] flareSpawnPoints;

    [SerializeField]
    private int flareCount = 30;

    [SerializeField]
    private float flareCooldown = 0.5f;

    [SerializeField]
    private int flaresPerBurst = 2;

    [Header("Chaff Settings")]
    [SerializeField]
    private GameObject chaffPrefab;

    [SerializeField]
    private int chaffCount = 30;

    [SerializeField]
    private float chaffCooldown = 0.5f;

    [Header("Audio (Legacy - Now uses AudioManager)")]
    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip flareSound;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip chaffSound;

    private int currentFlares;
    private int currentChaff;
    private float lastFlareTime;
    private float lastChaffTime;
    private AudioSource audioSource;
    private bool useAudioManager = true;

    public int CurrentFlares => currentFlares;
    public int MaxFlares => flareCount;
    public int CurrentChaff => currentChaff;
    public int MaxChaff => chaffCount;

    void Start()
    {
        currentFlares = flareCount;
        currentChaff = chaffCount;

        useAudioManager = AudioManager.Instance != null;

        if (!useAudioManager)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public bool DeployFlares()
    {
        Debug.Log($"[Countermeasures] DeployFlares() called on {gameObject.name}");
        Debug.Log(
            $"[Countermeasures] Status: Flares={currentFlares}/{flareCount}, FlarePrefab={(flarePrefab != null ? flarePrefab.name : "NULL")}, SpawnPoints={(flareSpawnPoints != null ? flareSpawnPoints.Length : 0)}"
        );

        if (currentFlares <= 0)
        {
            Debug.LogWarning("[Countermeasures] No flares remaining!");
            return false;
        }
        if (Time.time - lastFlareTime < flareCooldown)
        {
            Debug.Log("[Countermeasures] Flares on cooldown");
            return false;
        }

        lastFlareTime = Time.time;

        int toFire = Mathf.Min(flaresPerBurst, currentFlares);
        currentFlares -= toFire;

        for (int i = 0; i < toFire; i++)
        {
            SpawnFlare(i);
        }

        PlaySound(flareSound, true);
        Debug.Log($"Flares deployed! Remaining: {currentFlares}");
        return true;
    }

    public bool DeployChaff()
    {
        if (currentChaff <= 0)
            return false;
        if (Time.time - lastChaffTime < chaffCooldown)
            return false;

        lastChaffTime = Time.time;
        currentChaff--;

        if (chaffPrefab != null)
        {
            var spawnPoint = flareSpawnPoints.Length > 0 ? flareSpawnPoints[0] : transform;
            Instantiate(chaffPrefab, spawnPoint.position, spawnPoint.rotation);
        }

        PlaySound(chaffSound, false);
        Debug.Log($"Chaff deployed! Remaining: {currentChaff}");
        return true;
    }

    private void SpawnFlare(int index)
    {
        if (flarePrefab == null)
        {
            Debug.LogError(
                "[Countermeasures] CANNOT SPAWN FLARE - flarePrefab is NULL! Assign a flare prefab in Inspector."
            );
            return;
        }

        Transform spawnPoint;
        if (flareSpawnPoints != null && flareSpawnPoints.Length > 0)
        {
            spawnPoint = flareSpawnPoints[index % flareSpawnPoints.Length];
        }
        else
        {
            spawnPoint = transform;
        }

        var flare = Instantiate(flarePrefab, spawnPoint.position, spawnPoint.rotation);

        var rb = flare.GetComponent<Rigidbody>();
        if (rb != null)
        {
            var planeRb = GetComponent<Rigidbody>();
            if (planeRb != null)
            {
                rb.linearVelocity = planeRb.linearVelocity - transform.forward * 20f;
            }
        }
    }

    private void PlaySound(AudioClip clip, bool isFlare)
    {
        if (useAudioManager && AudioManager.Instance != null)
        {
            if (isFlare)
            {
                AudioManager.Instance.PlayFlare();
            }
            else
            {
                AudioManager.Instance.PlayChaff();
            }
        }
        else if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void Reload()
    {
        currentFlares = flareCount;
        currentChaff = chaffCount;
    }
}
