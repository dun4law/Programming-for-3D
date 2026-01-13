using UnityEngine;

public class StallWarning : MonoBehaviour
{
    public static StallWarning Instance { get; private set; }

    [Header("References")]
    [SerializeField]
    private Plane plane;

    [Header("Stall Settings")]
    [SerializeField]
    private float stallAOA = 15f;

    [SerializeField]
    private float criticalAOA = 25f;

    [SerializeField]
    private float minSpeed = 50f;

    [SerializeField]
    private float criticalSpeed = 30f;

    [Header("Audio (Now uses AudioManager)")]
    [Tooltip("Optional: Fallback AudioSource if AudioManager not available")]
    [SerializeField]
    private AudioSource warningAudio;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip stallWarningSound;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip stallHornSound;

    [Header("Effects")]
    [SerializeField]
    private bool enableBuffeting = true;
#pragma warning disable 0414
    [SerializeField]
    private float buffetIntensity = 0.5f;
#pragma warning restore 0414

    [Header("Debug")]
    [SerializeField]
    private bool enableDebugLogs = true;

    private bool isStalling = false;
    private bool isCritical = false;
    private bool hasPlayedWarning = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponent<Plane>();
        }

        Debug.Log(
            $"[StallWarning] Initialized - plane={(plane != null ? plane.DisplayName : "NULL")}, "
                + $"AudioManager={(AudioManager.Instance != null ? "OK" : "NULL")}, stallAOA={stallAOA}, minSpeed={minSpeed}"
        );
    }

    public void ApplyTuning(AircraftTuning tuning)
    {
        if (tuning == null)
            return;

        stallAOA = tuning.stallAOA;
        criticalAOA = tuning.criticalAOA;
        minSpeed = tuning.stallMinSpeed;
        criticalSpeed = tuning.stallCriticalSpeed;
    }

    void Update()
    {
        if (plane == null || plane.Dead)
            return;

        float aoa = Mathf.Abs(plane.AngleOfAttack * Mathf.Rad2Deg);
        float forwardSpeed = plane.LocalVelocity.z;
        float airspeed = plane.Velocity.magnitude;

        bool wasStalling = isStalling;

        float stallAOAExit = stallAOA - 3f;
        float minSpeedExit = minSpeed + 10f;

        if (wasStalling)
        {
            isStalling = (aoa > stallAOAExit) || (airspeed < minSpeedExit && airspeed > 5f);
        }
        else
        {
            isStalling = (aoa > stallAOA) || (airspeed < minSpeed && airspeed > 5f);
        }

        isCritical =
            isStalling && ((aoa > criticalAOA) || (airspeed < criticalSpeed && airspeed > 5f));

        if (enableDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log(
                $"[StallWarning] Status: AOA={aoa:F1}° (threshold={stallAOA}°), "
                    + $"Airspeed={airspeed:F1} (min={minSpeed}), ForwardSpeed={forwardSpeed:F1}, "
                    + $"isStalling={isStalling}, isCritical={isCritical}"
            );
        }

        if (isStalling && !wasStalling)
        {
            OnStallBegin();
        }
        else if (!isStalling && wasStalling)
        {
            OnStallEnd();
        }

        bool cameraShakeEnabled = PlayerPrefs.GetInt("CameraShake", 1) == 1;
        if (enableBuffeting && isStalling && cameraShakeEnabled)
        {
            ApplyCameraBuffeting();
        }
    }

    void OnStallBegin()
    {
        Debug.LogWarning(
            $"[StallWarning] STALL DETECTED! hasPlayedWarning={hasPlayedWarning}, isCritical={isCritical}"
        );

        if (hasPlayedWarning)
        {
            Debug.Log("[StallWarning] Warning already played, skipping sound");
            return;
        }
        hasPlayedWarning = true;

        bool canUseAudioManager = AudioManager.Instance != null;
        Debug.Log(
            $"[StallWarning] Playing stall warning - canUseAudioManager={canUseAudioManager}"
        );

        if (canUseAudioManager)
        {
            Debug.Log("[StallWarning] Calling AudioManager.Instance.PlayStallWarning()");
            AudioManager.Instance.PlayStallWarning();
        }
        else if (warningAudio != null)
        {
            Debug.Log(
                $"[StallWarning] Using fallback audio - isCritical={isCritical}, "
                    + $"stallWarningSound={(stallWarningSound != null ? stallWarningSound.name : "NULL")}, "
                    + $"stallHornSound={(stallHornSound != null ? stallHornSound.name : "NULL")}"
            );
            if (isCritical && stallWarningSound != null)
            {
                warningAudio.clip = stallWarningSound;
                warningAudio.loop = true;
                warningAudio.Play();
                Debug.Log($"[StallWarning] Playing critical stall sound: {stallWarningSound.name}");
            }
            else if (stallHornSound != null)
            {
                warningAudio.clip = stallHornSound;
                warningAudio.loop = true;
                warningAudio.Play();
                Debug.Log($"[StallWarning] Playing stall horn: {stallHornSound.name}");
            }
            else
            {
                Debug.LogWarning("[StallWarning] No fallback audio clips assigned!");
            }
        }
        else
        {
            Debug.LogError(
                "[StallWarning] Cannot play stall warning - no AudioManager and no fallback AudioSource!"
            );
        }
    }

    void OnStallEnd()
    {
        Debug.Log("[StallWarning] Stall condition cleared, stopping warning sound");
        hasPlayedWarning = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopStallWarning();
        }

        if (warningAudio != null && warningAudio.isPlaying)
        {
            warningAudio.Stop();
        }
    }

    void ApplyCameraBuffeting() { }

    public bool IsStalling => isStalling;
    public bool IsCritical => isCritical;
}
