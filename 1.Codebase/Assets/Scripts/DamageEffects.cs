using UnityEngine;

public class DamageEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Plane plane;

    [Header("Smoke Effects (ParticleSystem)")]
    [SerializeField]
    private ParticleSystem lightSmoke;

    [SerializeField]
    private ParticleSystem heavySmoke;

    [SerializeField]
    private ParticleSystem fire;

    [SerializeField]
    private ParticleSystem sparks;

    [Header("Effect Prefabs (GameObject - Alternative)")]
    [SerializeField]
    private GameObject lightSmokePrefab;

    [SerializeField]
    private GameObject heavySmokePrefab;

    [SerializeField]
    private GameObject firePrefab;

    [SerializeField]
    private GameObject sparksPrefab;

    [SerializeField]
    private Transform effectSpawnPoint;

    [Header("Thresholds")]
    [SerializeField]
    private float lightDamageThreshold = 0.7f;

    [SerializeField]
    private float heavyDamageThreshold = 0.4f;

    [SerializeField]
    private float criticalThreshold = 0.2f;

    [Header("Control Degradation")]
    [SerializeField]
    private bool enableControlDegradation = true;

    [SerializeField]
    private float maxControlLoss = 0.5f;

    [Header("Audio (Now uses AudioManager)")]
    [Tooltip("Optional: Fallback AudioSource if AudioManager not available")]
    [SerializeField]
    private AudioSource damageAudio;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip fireLoopSound;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip warningBeep;

    private float lastHealthRatio = 1f;
    private bool isOnFire = false;
    private bool useAudioManager = true;

    private GameObject lightSmokeInstance;
    private GameObject heavySmokeInstance;
    private GameObject fireInstance;

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponent<Plane>();
        }

        if (effectSpawnPoint == null)
        {
            effectSpawnPoint = transform;
        }

        useAudioManager = AudioManager.Instance != null;

        StopAllEffects();
    }

    void Update()
    {
        if (plane == null)
            return;

        float healthRatio = plane.Health / plane.MaxHealth;

        if (healthRatio < lastHealthRatio - 0.01f)
        {
            OnDamageTaken(lastHealthRatio - healthRatio);
        }
        lastHealthRatio = healthRatio;

        UpdateEffects(healthRatio);
    }

    void OnDamageTaken(float damageRatio)
    {
        if (sparks != null)
        {
            sparks.Play();
        }
        else if (sparksPrefab != null)
        {
            var spark = Instantiate(
                sparksPrefab,
                effectSpawnPoint.position,
                effectSpawnPoint.rotation
            );
            Destroy(spark, 2f);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit();
        }

        Debug.Log($"Damage taken! Lost: {damageRatio * 100:F0}%");
    }

    void UpdateEffects(float healthRatio)
    {
        bool shouldShowLightSmoke =
            healthRatio < lightDamageThreshold && healthRatio >= heavyDamageThreshold;
        UpdateEffect(ref lightSmokeInstance, lightSmoke, lightSmokePrefab, shouldShowLightSmoke);

        bool shouldShowHeavySmoke =
            healthRatio < heavyDamageThreshold && healthRatio >= criticalThreshold;
        UpdateEffect(ref heavySmokeInstance, heavySmoke, heavySmokePrefab, shouldShowHeavySmoke);

        bool shouldShowFire = healthRatio < criticalThreshold && healthRatio > 0;
        if (shouldShowFire && !isOnFire)
        {
            OnCatchFire();
        }
        else if (!shouldShowFire)
        {
            isOnFire = false;
        }
        UpdateEffect(ref fireInstance, fire, firePrefab, shouldShowFire);

        if (enableControlDegradation)
        {
            ApplyControlDegradation(healthRatio);
        }
    }

    void UpdateEffect(
        ref GameObject instance,
        ParticleSystem particleSystem,
        GameObject prefab,
        bool shouldShow
    )
    {
        if (particleSystem != null)
        {
            if (shouldShow)
            {
                if (!particleSystem.isPlaying)
                    particleSystem.Play();
            }
            else
            {
                if (particleSystem.isPlaying)
                    particleSystem.Stop();
            }
        }
        else if (prefab != null)
        {
            if (shouldShow && instance == null)
            {
                instance = Instantiate(
                    prefab,
                    effectSpawnPoint.position,
                    effectSpawnPoint.rotation,
                    effectSpawnPoint
                );
            }
            else if (!shouldShow && instance != null)
            {
                Destroy(instance);
                instance = null;
            }
        }
    }

    void OnCatchFire()
    {
        if (isOnFire)
            return;
        isOnFire = true;

        Debug.LogWarning("Aircraft on fire!");

        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFireAlarm();
            AudioManager.Instance.PlayLowHealthWarning();
        }
        else if (damageAudio != null && fireLoopSound != null)
        {
            damageAudio.clip = fireLoopSound;
            damageAudio.loop = true;
            damageAudio.Play();
        }
    }

    void ApplyControlDegradation(float healthRatio)
    {
        float controlEfficiency = Mathf.Lerp(1f - maxControlLoss, 1f, healthRatio);
    }

    void StopAllEffects()
    {
        if (lightSmoke != null)
            lightSmoke.Stop();
        if (heavySmoke != null)
            heavySmoke.Stop();
        if (fire != null)
            fire.Stop();
        if (sparks != null)
            sparks.Stop();

        if (lightSmokeInstance != null)
        {
            Destroy(lightSmokeInstance);
            lightSmokeInstance = null;
        }
        if (heavySmokeInstance != null)
        {
            Destroy(heavySmokeInstance);
            heavySmokeInstance = null;
        }
        if (fireInstance != null)
        {
            Destroy(fireInstance);
            fireInstance = null;
        }
    }

    public void OnRepaired()
    {
        StopAllEffects();
        isOnFire = false;

        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopWarning();
        }

        if (damageAudio != null)
            damageAudio.Stop();
    }

    public bool IsOnFire => isOnFire;
}
