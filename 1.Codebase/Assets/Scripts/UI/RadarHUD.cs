using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadarHUD : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private RectTransform radarRoot;

    [SerializeField]
    private RectTransform markerPrefab;

    [SerializeField]
    private Sprite markerSprite;

    [SerializeField]
    private Vector2 markerDefaultSize = new Vector2(10f, 10f);

    [Header("Tracking")]
    [SerializeField]
    private Plane playerPlane;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private bool rotateWithPlayer = true;

    [SerializeField]
    private bool clampToRadarEdge = true;

    [SerializeField]
    private float radarRangeMeters = 2000f;

    [SerializeField]
    private float rescanIntervalSeconds = 0.5f;

    [Header("Marker Appearance")]
    [SerializeField]
    private bool colorByDistance = true;

    [SerializeField]
    private Color nearColor = new Color(1f, 0.25f, 0.25f, 1f);

    [SerializeField]
    private Color farColor = new Color(1f, 0.25f, 0.25f, 0.4f);

    [SerializeField]
    private bool sizeByDistance = true;

    [SerializeField]
    private float nearSizeMultiplier = 1.3f;

    [SerializeField]
    private float farSizeMultiplier = 0.8f;

    [Header("Highlight")]
    [SerializeField]
    private bool highlightCurrentTarget = true;

    [SerializeField]
    private bool highlightLockedTargetOnly = false;

    [SerializeField]
    private Color currentTargetColor = new Color(1f, 0.85f, 0.15f, 1f);

    [SerializeField]
    private float currentTargetSizeMultiplier = 1.6f;

    [SerializeField]
    private Color lockedTargetColor = new Color(0.15f, 1f, 1f, 1f);

    [SerializeField]
    private float lockedTargetSizeMultiplier = 1.9f;

    [Header("Missile Tracking")]
    [SerializeField]
    private bool showMissiles = true;

    [SerializeField]
    private RectTransform missileMarkerPrefab;

    [SerializeField]
    private Sprite missileMarkerSprite;

    [SerializeField]
    private Vector2 missileMarkerSize = new Vector2(8f, 8f);

    [SerializeField]
    private Color missileColor = new Color(0f, 1f, 1f, 1f);

    [SerializeField]
    private Color missileColorAlt = new Color(1f, 0.3f, 0f, 1f);

    [SerializeField]
    private float missileBlinkSpeed = 6f;

    [SerializeField]
    private float missileDetectionRange = 2000f;

    [SerializeField]
    private float missileTrackingAngle = 60f;

    [Header("Filtering (Optional)")]
    [SerializeField]
    private bool requireRadarTrackable = false;

    [SerializeField]
    private RadarTrackable.TrackType requiredType = RadarTrackable.TrackType.Enemy;

    [SerializeField]
    private bool filterByTag = false;

    [SerializeField]
    private string targetTag = "Enemy";

    private sealed class MarkerState
    {
        public RectTransform Rect;
        public Graphic Graphic;
        public Vector2 BaseSize;
    }

    private readonly Dictionary<Target, MarkerState> markerByTarget =
        new Dictionary<Target, MarkerState>();
    private readonly List<Target> targets = new List<Target>();

    private readonly Dictionary<Missile, MarkerState> missileMarkers =
        new Dictionary<Missile, MarkerState>();
    private readonly List<Missile> trackedMissiles = new List<Missile>();

    private Target playerTarget;
    private float nextRescanTime;
    private bool warnedMissingMarkerTemplate;

    private void Awake()
    {
        if (radarRoot == null)
            radarRoot = transform as RectTransform;
    }

    private void OnEnable()
    {
        TryResolvePlayer();
        ForceRescan();
    }

    private void OnDisable()
    {
        ClearAllMarkers();
        ClearAllMissileMarkers();
        targets.Clear();
        trackedMissiles.Clear();
        playerTarget = null;
    }

    private void Update()
    {
        if (radarRoot == null)
            return;
        if (markerPrefab == null && markerSprite == null)
        {
            if (!warnedMissingMarkerTemplate)
            {
                warnedMissingMarkerTemplate = true;
                Debug.LogWarning(
                    $"{nameof(RadarHUD)} on '{name}': assign either '{nameof(markerPrefab)}' (a UI prefab with RectTransform) or '{nameof(markerSprite)}' (a Sprite) to render markers."
                );
            }
            return;
        }
        if (!TryResolvePlayer())
            return;

        ApplySettings();

        if (Time.unscaledTime >= nextRescanTime)
        {
            RescanTargets();
            nextRescanTime = Time.unscaledTime + Mathf.Max(0.05f, rescanIntervalSeconds);
        }

        if (showEnemiesOnRadar)
        {
            UpdateMarkers();
        }
        else
        {
            foreach (var kvp in markerByTarget)
            {
                if (kvp.Value?.Rect != null)
                    kvp.Value.Rect.gameObject.SetActive(false);
            }
        }

        if (showMissiles && showMissilesOnRadar)
        {
            DetectIncomingMissiles();
            UpdateMissileMarkers();
        }
        else
        {
            foreach (var kvp in missileMarkers)
            {
                if (kvp.Value?.Rect != null)
                    kvp.Value.Rect.gameObject.SetActive(false);
            }
        }
    }

    private bool showEnemiesOnRadar = true;
    private bool showMissilesOnRadar = true;
    private float settingsMarkerSize = 1f;
    private float settingsRadarRange = 4000f;
    private float lastSettingsCheckTime = 0f;
    private bool useFixedRadarRange = false;
    private float fixedRadarRange = 0f;

    private void ApplySettings()
    {
        if (Time.unscaledTime - lastSettingsCheckTime < 0.5f)
            return;
        lastSettingsCheckTime = Time.unscaledTime;

        showEnemiesOnRadar = PlayerPrefs.GetInt("RadarShowEnemies", 1) == 1;
        showMissilesOnRadar = PlayerPrefs.GetInt("RadarShowMissiles", 1) == 1;
        settingsMarkerSize = PlayerPrefs.GetFloat("RadarMarkerSize", 1f);
        settingsRadarRange = PlayerPrefs.GetFloat("RadarRange", 4000f);

        if (useFixedRadarRange)
        {
            radarRangeMeters = fixedRadarRange;
            missileDetectionRange = fixedRadarRange;
        }
        else
        {
            radarRangeMeters = settingsRadarRange;
            missileDetectionRange = settingsRadarRange;
        }
    }

    public void SetFixedRadarRange(float rangeMeters)
    {
        fixedRadarRange = Mathf.Max(1f, rangeMeters);
        useFixedRadarRange = true;
        radarRangeMeters = fixedRadarRange;
        missileDetectionRange = fixedRadarRange;
        PlayerPrefs.SetFloat("RadarRange", fixedRadarRange);
        ForceRescan();
    }

    public void SetPlayer(Transform playerTransform)
    {
        playerPlane = playerTransform != null ? playerTransform.GetComponent<Plane>() : null;
        player = playerTransform;
        playerTarget = player != null ? player.GetComponent<Target>() : null;
        ForceRescan();
    }

    public void SetPlayerPlane(Plane plane)
    {
        playerPlane = plane;
        player = playerPlane != null ? playerPlane.transform : player;
        playerTarget = playerPlane != null ? playerPlane.GetComponent<Target>() : playerTarget;
        ForceRescan();
    }

    private bool TryResolvePlayer()
    {
        if (playerPlane != null)
        {
            player = playerPlane.transform;
            if (playerTarget == null)
                playerTarget = playerPlane.GetComponent<Target>();
            return true;
        }

        if (player != null)
            return true;

        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
            player = pc.transform;
        if (player != null)
        {
            playerPlane = player.GetComponent<Plane>();
            playerTarget = player.GetComponent<Target>();
            return true;
        }

        if (playerPlane == null)
        {
            playerPlane = FindFirstObjectByType<Plane>();
            if (playerPlane != null)
            {
                player = playerPlane.transform;
                playerTarget = playerPlane.GetComponent<Target>();
                return true;
            }
        }

        return false;
    }

    private void ForceRescan()
    {
        nextRescanTime = 0f;
    }

    private void RescanTargets()
    {
        targets.Clear();

        var found = FindObjectsByType<Target>(FindObjectsSortMode.None);
        foreach (var t in found)
        {
            if (t == null)
                continue;
            if (t == playerTarget)
                continue;
            if (t.Plane != null && t.Plane.Dead)
                continue;
            if (filterByTag && !string.IsNullOrWhiteSpace(targetTag) && !t.CompareTag(targetTag))
                continue;
            if (requireRadarTrackable)
            {
                var trackable = t.GetComponent<RadarTrackable>();
                if (trackable == null)
                    continue;
                if (trackable.Type != requiredType)
                    continue;
            }

            targets.Add(t);
        }

        CleanupMarkers();
    }

    private void CleanupMarkers()
    {
        if (markerByTarget.Count == 0)
            return;

        var toRemove = new List<Target>();
        foreach (var kvp in markerByTarget)
        {
            var target = kvp.Key;
            if (target == null || !targets.Contains(target))
            {
                toRemove.Add(target);
            }
        }

        foreach (var target in toRemove)
        {
            if (!markerByTarget.TryGetValue(target, out var marker))
                continue;
            if (marker != null && marker.Rect != null)
                Destroy(marker.Rect.gameObject);
            markerByTarget.Remove(target);
        }
    }

    private void ClearAllMarkers()
    {
        foreach (var kvp in markerByTarget)
        {
            if (kvp.Value != null && kvp.Value.Rect != null)
                Destroy(kvp.Value.Rect.gameObject);
        }
        markerByTarget.Clear();
    }

    private void UpdateMarkers()
    {
        var rect = radarRoot.rect;
        var radius = 0.5f * Mathf.Min(rect.width, rect.height);
        if (radius <= 0.01f)
            return;

        var range = Mathf.Max(1f, radarRangeMeters);
        var scale = radius / range;

        var currentTarget = playerPlane != null ? playerPlane.Target : null;
        var currentLocked = playerPlane != null && playerPlane.MissileLocked;

        foreach (var t in targets)
        {
            if (t == null)
                continue;
            if (t.Plane != null && t.Plane.Dead)
                continue;

            var marker = GetOrCreateMarker(t);
            if (marker == null || marker.Rect == null)
                continue;

            var delta = t.Position - player.position;
            delta.y = 0f;

            Vector3 local = rotateWithPlayer ? player.InverseTransformDirection(delta) : delta;
            var pos = new Vector2(local.x, local.z);

            var distMeters = pos.magnitude;
            var normalized = Mathf.Clamp01(distMeters / range);

            if (distMeters > range)
            {
                if (!clampToRadarEdge)
                {
                    marker.Rect.gameObject.SetActive(false);
                    continue;
                }
                pos = pos.normalized * range;
            }

            marker.Rect.gameObject.SetActive(true);
            marker.Rect.anchoredPosition = pos * scale;

            ApplyMarkerStyle(
                marker,
                normalized,
                t == currentTarget,
                currentLocked && t == currentTarget
            );
        }
    }

    private void ApplyMarkerStyle(
        MarkerState marker,
        float normalizedDistance,
        bool isCurrentTarget,
        bool isLockedTarget
    )
    {
        if (marker == null || marker.Rect == null)
            return;

        float sizeMultiplier = 1f;
        if (sizeByDistance)
        {
            sizeMultiplier = Mathf.Lerp(nearSizeMultiplier, farSizeMultiplier, normalizedDistance);
        }

        Color color = Color.white;
        if (colorByDistance)
        {
            color = Color.Lerp(nearColor, farColor, normalizedDistance);
        }

        bool highlightCurrent =
            highlightCurrentTarget
            && isCurrentTarget
            && (!highlightLockedTargetOnly || isLockedTarget);
        bool highlightLocked = highlightCurrentTarget && isLockedTarget;

        if (highlightCurrent)
        {
            color = currentTargetColor;
            sizeMultiplier *= Mathf.Max(0.01f, currentTargetSizeMultiplier);
        }

        if (highlightLocked)
        {
            color = lockedTargetColor;
            sizeMultiplier *= Mathf.Max(0.01f, lockedTargetSizeMultiplier);
        }

        sizeMultiplier *= settingsMarkerSize;

        marker.Rect.sizeDelta = marker.BaseSize * sizeMultiplier;
        if (marker.Graphic != null)
            marker.Graphic.color = color;
    }

    private MarkerState GetOrCreateMarker(Target target)
    {
        MarkerState existing;
        if (
            markerByTarget.TryGetValue(target, out existing)
            && existing != null
            && existing.Rect != null
        )
            return existing;

        RectTransform inst;
        Graphic g;

        if (markerPrefab != null)
        {
            inst = Instantiate(markerPrefab, radarRoot);
            g = inst.GetComponent<Graphic>();
        }
        else
        {
            var go = new GameObject(
                $"RadarMarker_{target.name}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            inst = go.GetComponent<RectTransform>();
            inst.SetParent(radarRoot, false);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = markerSprite;
            img.preserveAspect = true;
            g = img;
        }

        inst.name = $"RadarMarker_{target.name}";

        var baseSize = ResolveBaseSize(inst);

        inst.anchorMin = new Vector2(0.5f, 0.5f);
        inst.anchorMax = new Vector2(0.5f, 0.5f);
        inst.pivot = new Vector2(0.5f, 0.5f);
        inst.anchoredPosition = Vector2.zero;
        inst.sizeDelta = baseSize;

        if (g == null)
            g = inst.GetComponentInChildren<Graphic>();
        if (g != null)
        {
            g.raycastTarget = false;
            if (markerSprite != null && g is Image image)
                image.sprite = markerSprite;
        }

        var state = new MarkerState
        {
            Rect = inst,
            Graphic = g,
            BaseSize = baseSize,
        };

        markerByTarget[target] = state;
        return state;
    }

    private Vector2 ResolveBaseSize(RectTransform rectTransform)
    {
        if (rectTransform == null)
            return markerDefaultSize;

        var stretched = rectTransform.anchorMin != rectTransform.anchorMax;

        var size = rectTransform.sizeDelta;
        if (!stretched && size.sqrMagnitude > 0.001f)
            return size;

        size = rectTransform.rect.size;
        if (!stretched && size.sqrMagnitude > 0.001f)
            return size;

        var preferredW = LayoutUtility.GetPreferredWidth(rectTransform);
        var preferredH = LayoutUtility.GetPreferredHeight(rectTransform);
        size = new Vector2(preferredW, preferredH);
        if (size.sqrMagnitude > 0.001f)
            return size;

        if (markerDefaultSize.sqrMagnitude > 0.001f)
            return markerDefaultSize;
        return new Vector2(10f, 10f);
    }

    #region Missile Tracking

    private void DetectIncomingMissiles()
    {
        trackedMissiles.RemoveAll(m => m == null);

        var allMissiles = FindObjectsByType<Missile>(FindObjectsSortMode.None);

        foreach (var missile in allMissiles)
        {
            if (missile == null)
                continue;
            if (trackedMissiles.Contains(missile))
                continue;

            if (IsMissileTargetingPlayer(missile))
            {
                trackedMissiles.Add(missile);
            }
        }

        CleanupMissileMarkers();
    }

    private bool IsMissileTargetingPlayer(Missile missile)
    {
        if (missile == null || player == null)
            return false;

        if (missile.Owner != null && missile.Owner == playerPlane)
            return false;

        var missileRb = missile.Rigidbody;
        if (missileRb == null)
            return false;

        float distance = Vector3.Distance(missileRb.position, player.position);
        if (distance > missileDetectionRange)
            return false;

        Vector3 missileDirection = missileRb.rotation * Vector3.forward;
        Vector3 toPlayer = (player.position - missileRb.position).normalized;

        float angle = Vector3.Angle(missileDirection, toPlayer);
        if (angle > missileTrackingAngle)
            return false;

        Vector3 playerVelocity =
            playerPlane != null && playerPlane.Rigidbody != null
                ? playerPlane.Rigidbody.linearVelocity
                : Vector3.zero;
        Vector3 relativeVelocity = missileRb.linearVelocity - playerVelocity;
        float approachSpeed = Vector3.Dot(relativeVelocity, toPlayer);

        return approachSpeed > 0;
    }

    private void UpdateMissileMarkers()
    {
        if (radarRoot == null || player == null)
            return;

        var rect = radarRoot.rect;
        var radius = 0.5f * Mathf.Min(rect.width, rect.height);
        if (radius <= 0.01f)
            return;

        var range = Mathf.Max(1f, missileDetectionRange);
        var scale = radius / range;

        float blink = Mathf.PingPong(Time.unscaledTime * missileBlinkSpeed, 1f);
        Color currentMissileColor = Color.Lerp(missileColor, missileColorAlt, blink);

        foreach (var missile in trackedMissiles)
        {
            if (missile == null)
                continue;

            var missileRb = missile.Rigidbody;
            if (missileRb == null)
                continue;

            var marker = GetOrCreateMissileMarker(missile);
            if (marker == null || marker.Rect == null)
                continue;

            var delta = missileRb.position - player.position;
            delta.y = 0f;

            Vector3 local = rotateWithPlayer ? player.InverseTransformDirection(delta) : delta;
            var pos = new Vector2(local.x, local.z);

            var distMeters = pos.magnitude;

            if (distMeters > range)
            {
                if (!clampToRadarEdge)
                {
                    marker.Rect.gameObject.SetActive(false);
                    continue;
                }
                pos = pos.normalized * range;
            }

            marker.Rect.gameObject.SetActive(true);
            marker.Rect.anchoredPosition = pos * scale;

            float sizeMultiplier = Mathf.Lerp(1f, 1.3f, blink) * settingsMarkerSize;
            marker.Rect.sizeDelta = marker.BaseSize * sizeMultiplier;
            if (marker.Graphic != null)
                marker.Graphic.color = currentMissileColor;
        }
    }

    private MarkerState GetOrCreateMissileMarker(Missile missile)
    {
        if (
            missileMarkers.TryGetValue(missile, out var existing)
            && existing != null
            && existing.Rect != null
        )
            return existing;

        RectTransform inst;
        Graphic g;

        if (missileMarkerPrefab != null)
        {
            inst = Instantiate(missileMarkerPrefab, radarRoot);
            g = inst.GetComponent<Graphic>();
        }
        else
        {
            var go = new GameObject(
                $"MissileMarker_{missile.GetEntityId()}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );
            inst = go.GetComponent<RectTransform>();
            inst.SetParent(radarRoot, false);

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;
            img.sprite = missileMarkerSprite != null ? missileMarkerSprite : markerSprite;
            img.preserveAspect = true;
            g = img;
        }

        inst.name = $"MissileMarker_{missile.GetEntityId()}";

        var baseSize = missileMarkerPrefab != null ? ResolveBaseSize(inst) : missileMarkerSize;

        inst.anchorMin = new Vector2(0.5f, 0.5f);
        inst.anchorMax = new Vector2(0.5f, 0.5f);
        inst.pivot = new Vector2(0.5f, 0.5f);
        inst.anchoredPosition = Vector2.zero;
        inst.sizeDelta = baseSize;

        if (g == null)
            g = inst.GetComponentInChildren<Graphic>();
        if (g != null)
        {
            g.raycastTarget = false;
            g.color = missileColor;
        }

        var state = new MarkerState
        {
            Rect = inst,
            Graphic = g,
            BaseSize = baseSize,
        };

        missileMarkers[missile] = state;
        return state;
    }

    private void CleanupMissileMarkers()
    {
        if (missileMarkers.Count == 0)
            return;

        var toRemove = new List<Missile>();
        foreach (var kvp in missileMarkers)
        {
            if (kvp.Key == null || !trackedMissiles.Contains(kvp.Key))
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var missile in toRemove)
        {
            if (missileMarkers.TryGetValue(missile, out var marker))
            {
                if (marker != null && marker.Rect != null)
                    Destroy(marker.Rect.gameObject);
                missileMarkers.Remove(missile);
            }
        }
    }

    private void ClearAllMissileMarkers()
    {
        foreach (var kvp in missileMarkers)
        {
            if (kvp.Value != null && kvp.Value.Rect != null)
                Destroy(kvp.Value.Rect.gameObject);
        }
        missileMarkers.Clear();
    }

    public int TrackedMissileCount => trackedMissiles.Count;

    #endregion
}
