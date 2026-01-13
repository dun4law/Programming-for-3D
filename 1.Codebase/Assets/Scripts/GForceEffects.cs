using UnityEngine;
using UnityEngine.UI;

public class GForceEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Plane plane;

    [Header("UI Overlay (Optional)")]
    [SerializeField]
    private Image blackoutOverlay;

    [SerializeField]
    private Image redoutOverlay;

    [SerializeField]
    private Image vignetteOverlay;

    [Header("Blackout Settings (Positive G)")]
    [SerializeField]
    private float blackoutStartG = 8f;

    [SerializeField]
    private float blackoutFullG = 9f;

    [SerializeField]
    private float blackoutBuildRate = 3f;

    [SerializeField]
    private float blackoutRecoveryRate = 0.5f;

    [Header("Debounce / Recovery Tuning")]
    [Tooltip(
        "How long G-force must continuously stay above the threshold before black-out/red-out starts, to avoid flickering from brief spikes."
    )]
    [SerializeField]
    private float triggerDelaySeconds = 0.15f;

    [Tooltip(
        "Accelerate fade-out when G-force has dropped significantly below the threshold to avoid persistence after G-force is normal."
    )]
    [SerializeField]
    private float recoveryBoostBufferG = 0.75f;

    [SerializeField]
    private float recoveryBoostMultiplier = 4f;

    [Header("Redout Settings (Negative G)")]
    [SerializeField]
    private float redoutStartG = -8f;

    [SerializeField]
    private float redoutFullG = -9f;

    [SerializeField]
    private float redoutBuildRate = 3f;

    [Header("Audio (Now uses AudioManager)")]
    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip breathingHeavy;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip heartbeat;

    private float currentBlackout = 0f;
    private float currentRedout = 0f;
    private AudioSource audioSource;
    private bool wasBlackedOut = false;
    private float blackoutOverGTimer = 0f;
    private float redoutOverGTimer = 0f;
    private float debugLogTimer = 0f;
    private bool useAudioManager = true;
    private float lastGForceAudioTime = 0f;
    private float glocStateCooldown = 0f;
    private bool gForceLoopActive = false;
    private const float GLOC_ENTRY_THRESHOLD = 0.95f;
    private const float GLOC_EXIT_THRESHOLD = 0.80f;
    private const float GLOC_STATE_COOLDOWN = 0.5f;

    private float cachedBlackoutAlpha = 0f;
    private float cachedRedoutAlpha = 0f;
    private float cachedVignetteAlpha = 0f;
    private bool isPausedHidden = false;

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponent<Plane>();
        }

        useAudioManager = AudioManager.Instance != null;

        if (!useAudioManager)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (blackoutOverlay == null || redoutOverlay == null)
        {
            CreateOverlays();
        }
    }

    public void ApplyTuning(AircraftTuning tuning)
    {
        if (tuning == null)
            return;

        blackoutStartG = tuning.blackoutStartG;
        blackoutFullG = tuning.blackoutFullG;
        redoutStartG = tuning.redoutStartG;
        redoutFullG = tuning.redoutFullG;
    }

    void CreateOverlays()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("GForceEffects: Canvas not found, cannot create effect overlays");
            return;
        }

        if (blackoutOverlay == null)
        {
            GameObject blackoutGO = new GameObject("BlackoutOverlay");
            blackoutGO.transform.SetParent(canvas.transform, false);
            blackoutOverlay = blackoutGO.AddComponent<Image>();
            blackoutOverlay.color = new Color(0, 0, 0, 0);
            blackoutOverlay.raycastTarget = false;

            RectTransform rt = blackoutGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        if (redoutOverlay == null)
        {
            GameObject redoutGO = new GameObject("RedoutOverlay");
            redoutGO.transform.SetParent(canvas.transform, false);
            redoutOverlay = redoutGO.AddComponent<Image>();
            redoutOverlay.color = new Color(1, 0, 0, 0);
            redoutOverlay.raycastTarget = false;

            RectTransform rt = redoutGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    void Update()
    {
        if (plane == null)
            return;

        if (isPausedHidden)
            return;

        float gForce = plane.LocalGForce.y / 9.81f;

        bool gforceEffectsEnabled = PlayerPrefs.GetInt("GForceEffects", 1) == 1;
        if (!gforceEffectsEnabled)
        {
            ClearEffects();
            StopAllGForceAudio();

            if (gForce >= blackoutStartG || gForce <= redoutStartG)
            {
                debugLogTimer += Time.deltaTime;
                if (debugLogTimer >= 1.0f)
                {
                    debugLogTimer = 0f;
                    string type = gForce > 0 ? "Blackout" : "Redout";
                    Debug.Log(
                        $"[GForceEffects] High G detected ({gForce:F1}G) - {type} visual effect prevented (Setting is OFF)"
                    );
                }
            }
            else
            {
                debugLogTimer = 0f;
            }

            return;
        }

        blackoutOverGTimer = gForce >= blackoutStartG ? (blackoutOverGTimer + Time.deltaTime) : 0f;
        redoutOverGTimer = gForce <= redoutStartG ? (redoutOverGTimer + Time.deltaTime) : 0f;

        UpdateBlackout(gForce);
        UpdateRedout(gForce);
        ApplyVisualEffects();
        CheckBlackoutState(gForce);
    }

    void UpdateBlackout(float gForce)
    {
        if (gForce >= blackoutStartG && blackoutOverGTimer >= triggerDelaySeconds)
        {
            float t = Mathf.InverseLerp(blackoutStartG, blackoutFullG, gForce);
            currentBlackout = Mathf.MoveTowards(
                currentBlackout,
                t,
                Time.deltaTime * Mathf.Max(0f, blackoutBuildRate)
            );
        }
        else
        {
            float recoveryRate = Mathf.Max(0f, blackoutRecoveryRate);
            if (gForce <= blackoutStartG - Mathf.Abs(recoveryBoostBufferG))
            {
                recoveryRate *= Mathf.Max(1f, recoveryBoostMultiplier);
            }
            currentBlackout = Mathf.MoveTowards(currentBlackout, 0f, Time.deltaTime * recoveryRate);
        }
    }

    void UpdateRedout(float gForce)
    {
        if (gForce <= redoutStartG && redoutOverGTimer >= triggerDelaySeconds)
        {
            float t = Mathf.InverseLerp(redoutStartG, redoutFullG, gForce);
            currentRedout = Mathf.MoveTowards(
                currentRedout,
                t,
                Time.deltaTime * Mathf.Max(0f, redoutBuildRate)
            );
        }
        else
        {
            float recoveryRate = Mathf.Max(0f, blackoutRecoveryRate);
            if (gForce >= redoutStartG + Mathf.Abs(recoveryBoostBufferG))
            {
                recoveryRate *= Mathf.Max(1f, recoveryBoostMultiplier);
            }
            currentRedout = Mathf.MoveTowards(currentRedout, 0f, Time.deltaTime * recoveryRate);
        }
    }

    void ApplyVisualEffects()
    {
        if (blackoutOverlay != null)
        {
            Color c = blackoutOverlay.color;
            c.a = currentBlackout * 0.60f;
            blackoutOverlay.color = c;
        }

        if (redoutOverlay != null)
        {
            Color c = redoutOverlay.color;
            c.a = currentRedout * 0.7f;
            redoutOverlay.color = c;
        }

        if (vignetteOverlay != null)
        {
            Color c = vignetteOverlay.color;
            c.a = Mathf.Max(currentBlackout, currentRedout) * 0.5f;
            vignetteOverlay.color = c;
        }
    }

    void CheckBlackoutState(float gForce)
    {
        if (glocStateCooldown > 0f)
        {
            glocStateCooldown -= Time.deltaTime;
        }

        bool isBlackedOut;
        if (wasBlackedOut)
        {
            isBlackedOut = currentBlackout > GLOC_EXIT_THRESHOLD;
        }
        else
        {
            isBlackedOut = currentBlackout >= GLOC_ENTRY_THRESHOLD;
        }

        float combinedEffect = Mathf.Max(currentBlackout, currentRedout);

        float gAudioIntensity = 0f;
        if (gForce >= blackoutStartG)
        {
            gAudioIntensity = Mathf.InverseLerp(blackoutStartG, blackoutFullG, gForce);
        }
        else if (gForce <= redoutStartG)
        {
            gAudioIntensity = Mathf.InverseLerp(redoutStartG, redoutFullG, gForce);
        }
        gAudioIntensity = Mathf.Clamp01(gAudioIntensity);

        bool isOverG = gForce >= blackoutStartG || gForce <= redoutStartG;

        if (isOverG)
        {
            if (useAudioManager && AudioManager.Instance != null)
            {
                AudioManager.Instance.StartGForceLoop(gAudioIntensity);
                gForceLoopActive = gAudioIntensity > 0.05f;
            }
            else if (
                audioSource != null
                && breathingHeavy != null
                && combinedEffect > 0.5f
                && Time.time - lastGForceAudioTime > 2f
            )
            {
                audioSource.PlayOneShot(breathingHeavy);
                lastGForceAudioTime = Time.time;
            }
        }
        else
        {
            StopGForceLoopIfActive();
        }

        if (isBlackedOut && !wasBlackedOut && glocStateCooldown <= 0f)
        {
            Debug.Log(" G-LOC! Pilot lost consciousness!");
            glocStateCooldown = GLOC_STATE_COOLDOWN;
            if (useAudioManager && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayGLocEffect();
            }
        }
        else if (!isBlackedOut && wasBlackedOut && glocStateCooldown <= 0f)
        {
            Debug.Log("Pilot regained consciousness");
            glocStateCooldown = GLOC_STATE_COOLDOWN;
            if (useAudioManager && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopGLocEffect();
            }
        }

        wasBlackedOut = isBlackedOut;
    }

    public float BlackoutLevel => currentBlackout;
    public float RedoutLevel => currentRedout;
    public bool IsBlackedOut => wasBlackedOut;

    public void HideOverlays()
    {
        isPausedHidden = true;

        if (blackoutOverlay != null)
        {
            cachedBlackoutAlpha = blackoutOverlay.color.a;
            Color c = blackoutOverlay.color;
            c.a = 0f;
            blackoutOverlay.color = c;
        }

        if (redoutOverlay != null)
        {
            cachedRedoutAlpha = redoutOverlay.color.a;
            Color c = redoutOverlay.color;
            c.a = 0f;
            redoutOverlay.color = c;
        }

        if (vignetteOverlay != null)
        {
            cachedVignetteAlpha = vignetteOverlay.color.a;
            Color c = vignetteOverlay.color;
            c.a = 0f;
            vignetteOverlay.color = c;
        }
    }

    public void ShowOverlays()
    {
        isPausedHidden = false;

        if (blackoutOverlay != null)
        {
            Color c = blackoutOverlay.color;
            c.a = cachedBlackoutAlpha;
            blackoutOverlay.color = c;
        }

        if (redoutOverlay != null)
        {
            Color c = redoutOverlay.color;
            c.a = cachedRedoutAlpha;
            redoutOverlay.color = c;
        }

        if (vignetteOverlay != null)
        {
            Color c = vignetteOverlay.color;
            c.a = cachedVignetteAlpha;
            vignetteOverlay.color = c;
        }
    }

    private void ClearEffects()
    {
        currentBlackout = 0f;
        currentRedout = 0f;
        blackoutOverGTimer = 0f;
        redoutOverGTimer = 0f;

        if (blackoutOverlay != null)
        {
            Color c = blackoutOverlay.color;
            c.a = 0f;
            blackoutOverlay.color = c;
        }

        if (redoutOverlay != null)
        {
            Color c = redoutOverlay.color;
            c.a = 0f;
            redoutOverlay.color = c;
        }

        if (vignetteOverlay != null)
        {
            Color c = vignetteOverlay.color;
            c.a = 0f;
            vignetteOverlay.color = c;
        }

        StopAllGForceAudio();
    }

    private void StopGForceLoopIfActive()
    {
        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGForceLoop();
        }
        gForceLoopActive = false;
    }

    private void StopAllGForceAudio()
    {
        StopGForceLoopIfActive();
        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGLocEffect();
        }
    }

    void OnDisable()
    {
        StopAllGForceAudio();
    }

    void OnDestroy()
    {
        StopAllGForceAudio();
    }
}
