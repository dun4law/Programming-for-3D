using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThreatWarningSystem : MonoBehaviour
{
    [Header("Target")]
    [SerializeField]
    private Plane playerPlane;

    [SerializeField]
    private Target playerTarget;

    [Header("Low Health Vignette")]
    [SerializeField]
    private Image lowHealthVignette;

    [SerializeField]
    private Color lowHealthColor = new Color(1f, 0f, 0f, 0.5f);

    [SerializeField]
    private float lowHealthThreshold = 0.3f;

    [SerializeField]
    private float vignettePulseSpeed = 2f;

    [Header("Missile Warning")]
    [SerializeField]
    private GameObject missileWarningPanel;

    [SerializeField]
    private TMP_Text missileWarningText;

    [SerializeField]
    private Text missileWarningTextLegacy;

    [SerializeField]
    private Image missileWarningIcon;

    [SerializeField]
    private Image missileWarningBackground;

    [SerializeField]
    private Color warningColor = new Color(1f, 0.5f, 0f, 1f);

    [SerializeField]
    private float warningBlinkSpeed = 4f;

    [SerializeField]
    private float missileDetectionRange = 2000f;

    [Header("Audio (Now uses AudioManager)")]
    [Tooltip("Optional: Fallback AudioSource if AudioManager not available")]
    [SerializeField]
    private AudioSource audioSource;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip lowHealthSound;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip missileWarningSound;

    [Tooltip("Optional: Fallback clip if not in AudioManager")]
    [SerializeField]
    private AudioClip missileLockSound;

    [SerializeField]
    private float soundCooldown = 1f;

    [Header("Debug")]
    [SerializeField]
    private bool enableDebugLogs = true;

    private List<Missile> trackingMissiles = new List<Missile>();
    private float lastLowHealthSoundTime = -999f;
    private float lastWatchYourSixTime = -999f;
    private bool wasLowHealth = false;
    private bool useAudioManager = true;

    private List<ThreatLevel> activeThreats = new List<ThreatLevel>();
    private int currentThreatIndex = 0;
    private float threatSwitchTimer = 0f;
    private const float THREAT_SWITCH_INTERVAL = 0.5f;
    private ThreatLevel lastLoggedThreat = ThreatLevel.None;
    private bool lastHadThreat = false;
    private ThreatLevel lastAudioThreatState = ThreatLevel.None;

    private float cachedVignetteAlpha = 0f;
    private bool isPausedHidden = false;

    void Start()
    {
        if (playerPlane == null)
        {
            var playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                var planeCamera = playerController.GetComponent<PlaneCamera>();
                if (planeCamera != null)
                {
                    playerPlane = planeCamera.GetComponentInChildren<Plane>();
                }
            }
        }

        if (playerPlane == null)
        {
            Debug.LogError(
                "[ThreatWarningSystem] CRITICAL: Player Plane NOT FOUND! System cannot function."
            );
        }

        if (playerTarget == null && playerPlane != null)
        {
            playerTarget = playerPlane.GetComponent<Target>();
        }

        if (lowHealthVignette == null)
        {
            var foundObj = GameObject.Find("LowHealthVignette");
            if (foundObj == null)
                foundObj = GameObject.Find("Vignette");
            if (foundObj == null)
                foundObj = GameObject.Find("Low Health Vignette");

            if (foundObj != null)
            {
                lowHealthVignette = foundObj.GetComponent<Image>();
            }

            if (lowHealthVignette == null)
            {
                var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    var images = canvas.GetComponentsInChildren<Image>(true);
                    foreach (var img in images)
                    {
                        if (img.name.Contains("LowHealthVignette") || img.name.Contains("Vignette"))
                        {
                            lowHealthVignette = img;
                            break;
                        }
                    }
                    if (lowHealthVignette != null)
                        break;
                }
            }
        }

        if (missileWarningPanel == null)
        {
            var p = GameObject.Find("MissileWarningPanel");
            if (p == null)
                p = GameObject.Find("Missile Warning Panel");
            if (p == null)
                p = GameObject.Find("MissileWarning");

            if (p != null)
            {
                missileWarningPanel = p;

                if (missileWarningText == null)
                    missileWarningText = p.GetComponentInChildren<TMP_Text>();
                if (missileWarningText == null && missileWarningTextLegacy == null)
                    missileWarningTextLegacy = p.GetComponentInChildren<Text>();
                if (missileWarningIcon == null)
                    missileWarningIcon = p.GetComponentInChildren<Image>();
                if (missileWarningBackground == null)
                {
                    missileWarningBackground = p.GetComponent<Image>();
                }
            }
            else
            {
                var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    Transform[] children = canvas.GetComponentsInChildren<Transform>(true);
                    foreach (Transform t in children)
                    {
                        if (t.name.Contains("MissileWarning"))
                        {
                            missileWarningPanel = t.gameObject;

                            if (missileWarningText == null)
                                missileWarningText = t.GetComponentInChildren<TMP_Text>();
                            if (missileWarningText == null && missileWarningTextLegacy == null)
                                missileWarningTextLegacy = t.GetComponentInChildren<Text>();
                            if (missileWarningBackground == null)
                                missileWarningBackground = t.GetComponent<Image>();
                            if (missileWarningIcon == null)
                            {
                                var images = t.GetComponentsInChildren<Image>();
                                foreach (var img in images)
                                {
                                    if (img.gameObject != t.gameObject)
                                    {
                                        missileWarningIcon = img;
                                        break;
                                    }
                                }
                            }
                            break;
                        }
                    }
                    if (missileWarningPanel != null)
                        break;
                }
            }

            if (missileWarningPanel != null && enableDebugLogs)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] Auto-found Missile Warning Panel: {missileWarningPanel.name}"
                );
                Debug.Log(
                    $"[ThreatWarningSystem] TMPro Text: {(missileWarningText != null ? missileWarningText.name : "NULL")}, Legacy Text: {(missileWarningTextLegacy != null ? missileWarningTextLegacy.name : "NULL")}"
                );
            }
            else if (missileWarningPanel == null)
            {
                Debug.LogWarning(
                    "[ThreatWarningSystem] Missile Warning Panel not found! Missile alerts will not be visible."
                );
            }
        }

        if (lowHealthVignette != null)
        {
            if (enableDebugLogs)
                Debug.Log(
                    $"[ThreatWarningSystem] Found LowHealthVignette: {lowHealthVignette.name}"
                );
            lowHealthVignette.gameObject.SetActive(true);
            lowHealthVignette.transform.SetAsLastSibling();
            lowHealthVignette.color = new Color(
                lowHealthColor.r,
                lowHealthColor.g,
                lowHealthColor.b,
                0f
            );

            if (lowHealthVignette.sprite == null)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] LowHealthVignette Image has NO SPRITE! Attempting to load default..."
                );

                Sprite defaultSprite = Resources.Load<Sprite>("UISprite");
                if (defaultSprite != null)
                {
                    lowHealthVignette.sprite = defaultSprite;
                }
                else
                {
                    int size = 64;
                    Texture2D texture = new Texture2D(size, size);
                    Color[] pixels = new Color[size * size];
                    Vector2 center = new Vector2(size / 2f, size / 2f);
                    float maxRadius = size / 1.42f;

                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            float dist = Vector2.Distance(new Vector2(x, y), center);
                            float t = Mathf.Clamp01(dist / maxRadius);

                            float alpha = Mathf.SmoothStep(0.3f, 1.0f, t);

                            pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                        }
                    }

                    texture.SetPixels(pixels);
                    texture.Apply();
                    lowHealthVignette.sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, size, size),
                        new Vector2(0.5f, 0.5f)
                    );
                    lowHealthVignette.type = Image.Type.Simple;

                    if (enableDebugLogs)
                        Debug.Log($"[ThreatWarningSystem] Generated procedural vignette sprite.");
                }
            }

            lowHealthVignette.raycastTarget = false;
        }
        else
        {
            Debug.LogError(
                "[ThreatWarningSystem] CRITICAL: LowHealthVignette Image NOT FOUND! Please assign it in the Inspector or name it 'LowHealthVignette'."
            );
        }

        if (missileWarningPanel != null)
        {
            if (missileWarningText == null)
            {
                missileWarningText = missileWarningPanel.GetComponentInChildren<TMP_Text>(true);
            }

            if (missileWarningText == null && missileWarningTextLegacy == null)
            {
                missileWarningTextLegacy = missileWarningPanel.GetComponentInChildren<Text>(true);
            }

            if (missileWarningBackground == null)
            {
                missileWarningBackground = missileWarningPanel.GetComponent<Image>();
            }

            if (enableDebugLogs)
            {
                Debug.Log($"[ThreatWarningSystem] MissileWarningPanel components:");
                Debug.Log(
                    $"  - TMPro Text: {(missileWarningText != null ? missileWarningText.name : "NULL")}"
                );
                Debug.Log(
                    $"  - Legacy Text: {(missileWarningTextLegacy != null ? missileWarningTextLegacy.name : "NULL")}"
                );
                Debug.Log(
                    $"  - Background: {(missileWarningBackground != null ? missileWarningBackground.name : "NULL")}"
                );
            }

            missileWarningPanel.SetActive(false);
        }
        else
        {
            Debug.LogError(
                "[ThreatWarningSystem] CRITICAL: MissileWarningPanel NOT FOUND! Missile warnings will not work."
            );
        }

        useAudioManager = AudioManager.Instance != null;

        if (!useAudioManager && audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }
        else if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
        }

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[ThreatWarningSystem] Initialized - Player: {(playerPlane != null ? playerPlane.DisplayName : "NULL")}"
            );
        }

        SetupCanvasSorting();

        if (lowHealthVignette != null)
        {
            lowHealthVignette.color = new Color(
                lowHealthColor.r,
                lowHealthColor.g,
                lowHealthColor.b,
                0f
            );
        }
    }

    private void SetupCanvasSorting()
    {
        if (lowHealthVignette == null)
            return;

        Canvas parentCanvas = lowHealthVignette.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] Vignette is on Canvas: {parentCanvas.name}, SortingOrder: {parentCanvas.sortingOrder}, RenderMode: {parentCanvas.renderMode}"
                );
            }

            if (
                parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                && parentCanvas.sortingOrder < 100
            )
            {
                var overrideCanvas = lowHealthVignette.gameObject.GetComponent<Canvas>();
                if (overrideCanvas == null)
                    overrideCanvas = lowHealthVignette.gameObject.AddComponent<Canvas>();

                overrideCanvas.overrideSorting = true;
                overrideCanvas.sortingOrder = 1000;

                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[ThreatWarningSystem] Added override canvas with sorting order {overrideCanvas.sortingOrder}"
                    );
                }
            }
        }
        else
        {
            Debug.LogError(
                "[ThreatWarningSystem] Vignette has NO Parent Canvas! It will not be visible."
            );
        }
    }

    void Update()
    {
        if (playerPlane == null || playerPlane.Dead)
            return;

        if (isPausedHidden)
            return;

        UpdateLowHealthVignette();
        UpdateMissileWarning();
    }

    void UpdateLowHealthVignette()
    {
        if (lowHealthVignette == null)
            return;

        float healthPercent = playerPlane.Health / playerPlane.MaxHealth;

        bool isLowHealth = healthPercent <= (lowHealthThreshold + 0.01f);

        if (enableDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log(
                $"[ThreatWarningSystem Check] HP: {playerPlane.Health}/{playerPlane.MaxHealth} = {healthPercent:F4}. Threshold: {lowHealthThreshold}. IsLow: {isLowHealth}"
            );
        }

        if (isLowHealth)
        {
            float pulse = Mathf.PingPong(Time.time * vignettePulseSpeed, 1f);
            float minAlpha = 0.3f;
            float maxAlpha = lowHealthColor.a;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);

            float intensity = 1f - (healthPercent / lowHealthThreshold);
            intensity = Mathf.Clamp01(intensity + 0.2f);
            alpha *= intensity;

            alpha = Mathf.Max(alpha, 0.2f);

            lowHealthVignette.color = new Color(
                lowHealthColor.r,
                lowHealthColor.g,
                lowHealthColor.b,
                alpha
            );

            if (enableDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] Low Health Active! HP%: {healthPercent:P1} Alpha: {alpha:F2}"
                );
            }

            if (!wasLowHealth && Time.time - lastLowHealthSoundTime > soundCooldown)
            {
                PlayWarningSound(WarningSound.LowHealth);
                lastLowHealthSoundTime = Time.time;

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayHeartbeat();
                    AudioManager.Instance.PlayBreathingHeavy();
                }
            }
        }
        else
        {
            Color currentColor = lowHealthVignette.color;
            if (currentColor.a > 0.01f)
            {
                float fadeSpeed = 2f;
                currentColor.a = Mathf.Lerp(currentColor.a, 0f, Time.deltaTime * fadeSpeed);
                lowHealthVignette.color = currentColor;
            }

            if (wasLowHealth && AudioManager.Instance != null)
            {
                AudioManager.Instance.StopLowHealthWarning();
            }
        }

        wasLowHealth = isLowHealth;
    }

    void UpdateMissileWarning()
    {
        if (playerTarget == null || playerTarget.gameObject == null)
        {
            if (playerPlane != null)
            {
                playerTarget = playerPlane.GetComponent<Target>();
            }

            if (playerTarget == null)
            {
                if (enableDebugLogs && Time.frameCount % 120 == 0)
                {
                    Debug.LogWarning(
                        "[ThreatWarningSystem] playerTarget is NULL! Cannot detect threats."
                    );
                }
                return;
            }
        }

        if (enableDebugLogs && Time.frameCount % 120 == 0)
        {
            Debug.Log(
                $"[ThreatWarningSystem] Status: panel={missileWarningPanel != null}, text={missileWarningText != null}, legacyText={missileWarningTextLegacy != null}, bg={missileWarningBackground != null}"
            );
        }

        int removedCount = trackingMissiles.RemoveAll(m => m == null);
        if (removedCount > 0 && enableDebugLogs)
        {
            Debug.Log(
                $"[ThreatWarningSystem] Cleaned up {removedCount} destroyed missiles. Remaining: {trackingMissiles.Count}"
            );
        }

        Missile incomingFromTarget = null;
        try
        {
            incomingFromTarget = playerTarget.GetIncomingMissile();
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
            {
                Debug.LogError(
                    $"[ThreatWarningSystem] Error getting incoming missile: {e.Message}"
                );
            }
        }

        if (incomingFromTarget != null && !trackingMissiles.Contains(incomingFromTarget))
        {
            trackingMissiles.Add(incomingFromTarget);
            if (enableDebugLogs)
                Debug.Log(
                    $"[ThreatWarningSystem] Synced incoming missile from Target: {incomingFromTarget.GetEntityId()}"
                );
        }

        DetectIncomingMissiles();

        bool isBeingLockedByEnemy = CheckEnemyLockOn();

        bool hasIncomingMissiles = trackingMissiles.Count > 0;

        bool isStalling = StallWarning.Instance != null && StallWarning.Instance.IsStalling;
        bool isCriticalStall = StallWarning.Instance != null && StallWarning.Instance.IsCritical;

        activeThreats.Clear();
        if (hasIncomingMissiles)
        {
            activeThreats.Add(ThreatLevel.MissileIncoming);
        }
        if (isBeingLockedByEnemy && !hasIncomingMissiles)
        {
            activeThreats.Add(ThreatLevel.BeingLocked);
        }
        if (isStalling)
        {
            activeThreats.Add(ThreatLevel.Stall);
        }

        if (activeThreats.Count > 1)
        {
            threatSwitchTimer += Time.deltaTime;
            if (threatSwitchTimer >= THREAT_SWITCH_INTERVAL)
            {
                threatSwitchTimer = 0f;
                currentThreatIndex = (currentThreatIndex + 1) % activeThreats.Count;
            }
        }
        else
        {
            currentThreatIndex = 0;
            threatSwitchTimer = 0f;
        }

        ThreatLevel currentThreat =
            activeThreats.Count > 0
                ? activeThreats[currentThreatIndex % activeThreats.Count]
                : ThreatLevel.None;

        bool hasThreat = activeThreats.Count > 0;

        if (enableDebugLogs && (hasThreat != lastHadThreat || currentThreat != lastLoggedThreat))
        {
            if (hasThreat)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] THREAT DETECTED! Level={currentThreat}, isLocked={isBeingLockedByEnemy}, hasIncoming={hasIncomingMissiles}, missileCount={trackingMissiles.Count}"
                );
            }
            else
            {
                Debug.Log("[ThreatWarningSystem] All threats cleared.");
            }
            lastLoggedThreat = currentThreat;
            lastHadThreat = hasThreat;
        }

        ThreatLevel audioThreatState = GetAudioThreatState(currentThreat);
        if (audioThreatState != lastAudioThreatState)
        {
            OnAudioThreatStateChanged(lastAudioThreatState, audioThreatState);
            lastAudioThreatState = audioThreatState;
        }

        if (missileWarningPanel != null)
        {
            bool wasActive = missileWarningPanel.activeSelf;
            missileWarningPanel.SetActive(hasThreat);

            if (hasThreat)
            {
                if (missileWarningText != null && !missileWarningText.gameObject.activeInHierarchy)
                {
                    missileWarningText.gameObject.SetActive(true);
                    if (enableDebugLogs)
                        Debug.Log("[ThreatWarningSystem] Activated TMPro text object");
                }
                if (
                    missileWarningTextLegacy != null
                    && !missileWarningTextLegacy.gameObject.activeInHierarchy
                )
                {
                    missileWarningTextLegacy.gameObject.SetActive(true);
                    if (enableDebugLogs)
                        Debug.Log("[ThreatWarningSystem] Activated Legacy text object");
                }
            }

            if (enableDebugLogs && wasActive != hasThreat)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] Panel visibility changed: {wasActive} -> {hasThreat}"
                );
            }
        }
        else if (hasThreat && enableDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.LogWarning("[ThreatWarningSystem] WARNING: Panel is NULL but threat exists!");
        }

        if (hasThreat)
        {
            UpdateWarningDisplay(currentThreat);
        }
        else
        {
            ClearWarningDisplay();
        }
    }

    private enum ThreatLevel
    {
        None,
        Stall,
        BeingLocked,
        MissileIncoming,
    }

    bool CheckEnemyLockOn()
    {
        if (playerPlane == null)
            return false;

        var allPlanes = FindObjectsByType<Plane>(FindObjectsSortMode.None);
        foreach (var plane in allPlanes)
        {
            if (plane == null || plane == playerPlane)
                continue;
            if (plane.Dead)
                continue;
            if (plane.team == playerPlane.team)
                continue;

            if (enableDebugLogs && Time.frameCount % 180 == 0)
            {
                var enemyTarget = plane.Target;
                Debug.Log(
                    $"[ThreatWarningSystem] Enemy '{plane.DisplayName}': Target={enemyTarget?.Name ?? "NULL"}, MissileTracking={plane.MissileTracking}, MissileLocked={plane.MissileLocked}, LockingPlayer={(enemyTarget == playerTarget)}"
                );
            }

            if (plane.Target != null && plane.Target == playerTarget)
            {
                if (plane.MissileTracking || plane.MissileLocked)
                {
                    if (AudioManager.Instance != null && Time.time - lastWatchYourSixTime > 8f)
                    {
                        AudioManager.Instance.PlayWatchYourSix();
                        lastWatchYourSixTime = Time.time;
                    }
                    return true;
                }
            }
        }

        return false;
    }

    void UpdateWarningDisplay(ThreatLevel threat)
    {
        float blink = Mathf.PingPong(Time.time * warningBlinkSpeed, 1f);

        string warningTextStr;
        Color baseColor;

        if (threat == ThreatLevel.MissileIncoming)
        {
            int count = trackingMissiles.Count;
            warningTextStr = count == 1 ? "MISSILE INCOMING!" : $"MISSILES x{count}!";
            baseColor = new Color(1f, 0.2f, 0f, 1f);
        }
        else if (threat == ThreatLevel.BeingLocked)
        {
            warningTextStr = "MISSILE LOCK";
            baseColor = new Color(1f, 0.7f, 0f, 1f);
        }
        else if (threat == ThreatLevel.Stall)
        {
            bool isCritical = StallWarning.Instance != null && StallWarning.Instance.IsCritical;
            warningTextStr = isCritical ? "STALL STALL!" : "STALL";
            baseColor = new Color(1f, 1f, 0f, 1f);

            if (isCritical)
            {
                blink = Mathf.PingPong(Time.time * warningBlinkSpeed * 2f, 1f);
            }
        }
        else
        {
            warningTextStr = "";
            baseColor = Color.white;
        }

        if (enableDebugLogs && Time.frameCount % 60 == 0)
        {
            Debug.Log(
                $"[ThreatWarningSystem] Setting text to: '{warningTextStr}', TMPro={missileWarningText != null}, Legacy={missileWarningTextLegacy != null}"
            );
        }

        if (missileWarningText != null)
        {
            Color textColor = Color.white;
            missileWarningText.color = textColor;
            missileWarningText.text = warningTextStr;

            missileWarningText.enableAutoSizing = true;
            missileWarningText.fontSizeMin = 10;
            missileWarningText.fontSizeMax = 36;

            if (enableDebugLogs && Time.frameCount % 120 == 0)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] TMPro text set: '{missileWarningText.text}', enabled={missileWarningText.enabled}, active={missileWarningText.gameObject.activeInHierarchy}"
                );
            }
        }
        else if (missileWarningTextLegacy != null)
        {
            Color textColor = Color.white;
            missileWarningTextLegacy.color = textColor;
            missileWarningTextLegacy.text = warningTextStr;

            missileWarningTextLegacy.horizontalOverflow = HorizontalWrapMode.Overflow;
            missileWarningTextLegacy.verticalOverflow = VerticalWrapMode.Overflow;
            missileWarningTextLegacy.resizeTextForBestFit = true;
            missileWarningTextLegacy.resizeTextMinSize = 10;
            missileWarningTextLegacy.resizeTextMaxSize = 36;

            if (enableDebugLogs && Time.frameCount % 120 == 0)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] Legacy text set: '{missileWarningTextLegacy.text}', enabled={missileWarningTextLegacy.enabled}, active={missileWarningTextLegacy.gameObject.activeInHierarchy}, fontSize={missileWarningTextLegacy.fontSize}"
                );
            }
        }
        else
        {
            if (enableDebugLogs && Time.frameCount % 120 == 0)
            {
                Debug.LogWarning(
                    "[ThreatWarningSystem] NO TEXT COMPONENT FOUND! Cannot display warning text."
                );
            }
        }

        if (missileWarningBackground != null)
        {
            Color bgColor = baseColor;
            bgColor.a = Mathf.Lerp(0.3f, 0.7f, blink);
            missileWarningBackground.color = bgColor;
        }

        if (missileWarningIcon != null)
        {
            Color iconColor = baseColor;
            iconColor.a = Mathf.Lerp(0.3f, 1f, blink);
            missileWarningIcon.color = iconColor;
        }
    }

    void ClearWarningDisplay()
    {
        if (missileWarningText != null)
        {
            missileWarningText.text = "";
        }
        if (missileWarningTextLegacy != null)
        {
            missileWarningTextLegacy.text = "";
        }
    }

    void DetectIncomingMissiles()
    {
        var allMissiles = FindObjectsByType<Missile>(FindObjectsSortMode.None);

        foreach (var missile in allMissiles)
        {
            if (missile == null || trackingMissiles.Contains(missile))
                continue;

            if (IsTargetingPlayer(missile))
            {
                trackingMissiles.Add(missile);

                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[ThreatWarningSystem] Missile detected via scan! ID: {missile.GetEntityId()}"
                    );
                }
            }
        }

        int removedCount = 0;
        trackingMissiles.RemoveAll(m =>
        {
            if (m == null)
            {
                removedCount++;
                return true;
            }

            if (!IsStillThreatening(m))
            {
                removedCount++;
                return true;
            }
            return false;
        });

        if (removedCount > 0 && enableDebugLogs)
        {
            Debug.Log(
                $"[ThreatWarningSystem] Removed {removedCount} missile(s) no longer threatening. Remaining: {trackingMissiles.Count}"
            );
        }
    }

    bool IsStillThreatening(Missile missile)
    {
        if (missile == null || playerPlane == null)
            return false;
        if (playerPlane.Rigidbody == null)
            return false;

        var missileRb = missile.Rigidbody;
        if (missileRb == null || missileRb.isKinematic)
            return false;

        float distance = Vector3.Distance(missileRb.position, playerPlane.Rigidbody.position);

        if (distance > missileDetectionRange * 1.5f)
            return false;

        Vector3 relativeVelocity = missileRb.linearVelocity - playerPlane.Rigidbody.linearVelocity;
        Vector3 relativeDirection = (
            playerPlane.Rigidbody.position - missileRb.position
        ).normalized;
        float approachSpeed = Vector3.Dot(relativeVelocity, relativeDirection);

        if (approachSpeed < -50f && distance > 200f)
            return false;

        return true;
    }

    bool IsTargetingPlayer(Missile missile)
    {
        if (missile == null || playerPlane == null)
            return false;
        if (playerPlane.Rigidbody == null)
            return false;

        var missileRb = missile.Rigidbody;
        if (missileRb == null || missileRb.isKinematic)
            return false;

        float distance = Vector3.Distance(missileRb.position, playerPlane.Rigidbody.position);

        if (distance > missileDetectionRange)
            return false;

        Vector3 missileDirection = missileRb.rotation * Vector3.forward;
        Vector3 toPlayer = (playerPlane.Rigidbody.position - missileRb.position).normalized;

        float angle = Vector3.Angle(missileDirection, toPlayer);
        if (angle > 60f)
            return false;

        Vector3 relativeVelocity = missileRb.linearVelocity - playerPlane.Rigidbody.linearVelocity;
        Vector3 relativeDirection = (
            playerPlane.Rigidbody.position - missileRb.position
        ).normalized;
        float approachSpeed = Vector3.Dot(relativeVelocity, relativeDirection);

        if (approachSpeed < 10f)
            return false;

        return true;
    }

    ThreatLevel GetAudioThreatState(ThreatLevel visualThreat)
    {
        if (trackingMissiles.Count > 0)
            return ThreatLevel.MissileIncoming;
        if (CheckEnemyLockOn())
            return ThreatLevel.BeingLocked;
        return ThreatLevel.None;
    }

    void OnAudioThreatStateChanged(ThreatLevel oldState, ThreatLevel newState)
    {
        if (AudioManager.Instance == null)
            return;

        if (enableDebugLogs)
        {
            Debug.Log($"[ThreatWarningSystem] Audio state changed: {oldState} -> {newState}");
        }

        if (oldState == ThreatLevel.MissileIncoming)
        {
            AudioManager.Instance.StopMissileIncoming();
        }
        else if (oldState == ThreatLevel.BeingLocked)
        {
            AudioManager.Instance.StopMissileLock();
        }

        if (newState == ThreatLevel.MissileIncoming)
        {
            AudioManager.Instance.PlayMissileIncoming();
        }
        else if (newState == ThreatLevel.BeingLocked)
        {
            AudioManager.Instance.PlayMissileLock();
        }
    }

    private enum WarningSound
    {
        LowHealth,
        MissileIncoming,
        MissileLock,
    }

    void PlayWarningSound(WarningSound soundType)
    {
        if (soundType == WarningSound.LowHealth) { }

        Debug.Log($"[ThreatWarningSystem] PlayWarningSound: {soundType}");

        if (useAudioManager && AudioManager.Instance != null)
        {
            switch (soundType)
            {
                case WarningSound.LowHealth:
                    AudioManager.Instance.PlayLowHealthWarning();
                    break;
                case WarningSound.MissileIncoming:

                    AudioManager.Instance.StopMissileLock();
                    AudioManager.Instance.PlayMissileIncoming();
                    break;
                case WarningSound.MissileLock:
                    AudioManager.Instance.PlayMissileLock();
                    break;
            }
        }
        else if (audioSource != null)
        {
            AudioClip clip = soundType switch
            {
                WarningSound.LowHealth => lowHealthSound,
                WarningSound.MissileIncoming => missileWarningSound,
                WarningSound.MissileLock => missileLockSound,
                _ => null,
            };
            if (clip != null)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] PlayWarningSound (Fallback): Playing '{clip.name}'"
                );
                audioSource.PlayOneShot(clip);
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null)
            return;

        if (useAudioManager && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void OnMissileLaunched(Missile missile, bool launched)
    {
        if (missile == null)
            return;

        if (launched && !IsMissileIntendedForPlayer(missile))
        {
            return;
        }

        if (launched)
        {
            if (!trackingMissiles.Contains(missile))
            {
                trackingMissiles.Add(missile);

                if (enableDebugLogs)
                {
                    Debug.Log(
                        $"[ThreatWarningSystem] Missile incoming! ID: {missile.GetEntityId()}, Total tracking: {trackingMissiles.Count}"
                    );
                }

                if (missileWarningPanel != null && !missileWarningPanel.activeSelf)
                {
                    missileWarningPanel.SetActive(true);

                    if (missileWarningText != null && !missileWarningText.gameObject.activeSelf)
                    {
                        missileWarningText.gameObject.SetActive(true);
                    }
                    if (
                        missileWarningTextLegacy != null
                        && !missileWarningTextLegacy.gameObject.activeSelf
                    )
                    {
                        missileWarningTextLegacy.gameObject.SetActive(true);
                    }

                    if (enableDebugLogs)
                    {
                        Debug.Log(
                            "[ThreatWarningSystem] Panel activated immediately on missile lock!"
                        );
                        var rt = missileWarningPanel.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            Debug.Log(
                                $"[ThreatWarningSystem] Panel RectTransform: position={rt.anchoredPosition}, size={rt.sizeDelta}"
                            );
                        }
                        Debug.Log(
                            $"[ThreatWarningSystem] Panel activeInHierarchy={missileWarningPanel.activeInHierarchy}"
                        );
                    }
                }
            }
        }
        else
        {
            trackingMissiles.Remove(missile);

            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[ThreatWarningSystem] Missile threat cleared. ID: {missile.GetEntityId()}, Remaining: {trackingMissiles.Count}"
                );
            }

            if (trackingMissiles.Count == 0)
            {
                if (
                    !CheckEnemyLockOn()
                    && missileWarningPanel != null
                    && missileWarningPanel.activeSelf
                )
                {
                    missileWarningPanel.SetActive(false);
                    if (enableDebugLogs)
                        Debug.Log("[ThreatWarningSystem] Panel deactivated - no more threats.");
                }
            }
        }
    }

    private bool IsMissileIntendedForPlayer(Missile missile)
    {
        if (missile == null || playerTarget == null)
            return false;

        if (missile.WarningTarget != null)
            return missile.WarningTarget == playerTarget;
        if (missile.Target != null)
            return missile.Target == playerTarget;

        return IsTargetingPlayer(missile);
    }

    public void AddThreat(Missile missile)
    {
        if (missile != null && !trackingMissiles.Contains(missile))
        {
            trackingMissiles.Add(missile);
        }
    }

    public void RemoveThreat(Missile missile)
    {
        trackingMissiles.Remove(missile);
    }

    public int ThreatCount => trackingMissiles.Count;

    public void HideOverlays()
    {
        isPausedHidden = true;

        if (lowHealthVignette != null)
        {
            cachedVignetteAlpha = lowHealthVignette.color.a;
            Color c = lowHealthVignette.color;
            c.a = 0f;
            lowHealthVignette.color = c;
        }

        if (missileWarningPanel != null)
        {
            missileWarningPanel.SetActive(false);
        }
    }

    public void ShowOverlays()
    {
        isPausedHidden = false;

        if (lowHealthVignette != null)
        {
            Color c = lowHealthVignette.color;
            c.a = cachedVignetteAlpha;
            lowHealthVignette.color = c;
        }
    }
}
