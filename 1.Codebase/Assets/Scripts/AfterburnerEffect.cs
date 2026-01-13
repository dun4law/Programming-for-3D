using UnityEngine;

public class AfterburnerEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Plane plane;

    [Header("Visual Effects")]
    [SerializeField]
    private ParticleSystem[] afterburnerParticles;

    [SerializeField]
    private Light[] afterburnerLights;

    [SerializeField]
    private float lightIntensityMax = 3f;

    [SerializeField]
    private float throttleThreshold = 0.9f;

    [Header("Audio (Legacy - Now uses AudioManager)")]
    [Tooltip("Optional: Fallback AudioSource if AudioManager not available")]
    [SerializeField]
    private AudioSource afterburnerAudio;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip afterburnerSound;

    [SerializeField]
    private float audioFadeSpeed = 3f;

    [Header("Heat Distortion")]
    [SerializeField]
    private GameObject heatDistortionEffect;

    private bool isAfterburnerActive = false;
    private float currentIntensity = 0f;
    private bool useAudioManager = true;

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponent<Plane>();
        }

        useAudioManager = AudioManager.Instance != null;

        SetEffectsIntensity(0f);
    }

    void Update()
    {
        if (plane == null)
            return;

        bool shouldBeActive = plane.Throttle >= throttleThreshold && !plane.Dead;

        if (shouldBeActive && !isAfterburnerActive)
        {
            ActivateAfterburner();
        }
        else if (!shouldBeActive && isAfterburnerActive)
        {
            DeactivateAfterburner();
        }

        float targetIntensity = shouldBeActive ? 1f : 0f;
        currentIntensity = Mathf.MoveTowards(
            currentIntensity,
            targetIntensity,
            Time.deltaTime * audioFadeSpeed
        );
        SetEffectsIntensity(currentIntensity);
    }

    void ActivateAfterburner()
    {
        isAfterburnerActive = true;

        if (afterburnerParticles != null)
        {
            foreach (var ps in afterburnerParticles)
            {
                if (ps != null)
                    ps.Play();
            }
        }

        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.StartAfterburnerLoop(afterburnerSound);
        }
        else if (afterburnerAudio != null && afterburnerSound != null)
        {
            afterburnerAudio.clip = afterburnerSound;
            afterburnerAudio.loop = true;
            afterburnerAudio.Play();
        }

        if (heatDistortionEffect != null)
        {
            heatDistortionEffect.SetActive(true);
        }

        Debug.Log(" Afterburner Engaged!");
    }

    void DeactivateAfterburner()
    {
        isAfterburnerActive = false;

        if (afterburnerParticles != null)
        {
            foreach (var ps in afterburnerParticles)
            {
                if (ps != null)
                    ps.Stop();
            }
        }

        if (heatDistortionEffect != null)
        {
            heatDistortionEffect.SetActive(false);
        }

        Debug.Log("Afterburner Disengaged");
    }

    void SetEffectsIntensity(float intensity)
    {
        if (afterburnerLights != null)
        {
            foreach (var light in afterburnerLights)
            {
                if (light != null)
                {
                    light.intensity = lightIntensityMax * intensity;
                }
            }
        }

        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateAfterburnerIntensity(intensity);
        }
        else if (afterburnerAudio != null)
        {
            afterburnerAudio.volume = intensity;

            if (intensity <= 0.01f && afterburnerAudio.isPlaying)
            {
                afterburnerAudio.Stop();
            }
        }
    }

    public bool IsActive => isAfterburnerActive;
}
