using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapBoundaryWarning : MonoBehaviour
{
    [Header("Boundary Settings")]
    [SerializeField]
    private Vector3 mapCenter = Vector3.zero;

    [SerializeField]
    private Vector3 mapSize = new Vector3(10000, 5000, 10000);

    [SerializeField]
    private float warningDistance = 500f;

    [SerializeField]
    private float killDistance = 1000f;

    [Header("Player UI")]
    [SerializeField]
    private GameObject warningPanel;

    [SerializeField]
    private TextMeshProUGUI warningText;

    [SerializeField]
    private Image warningBackground;

    [SerializeField]
    private bool autoCreateUI = false;

    [Header("Warning Settings")]
    [SerializeField]
    private Color warningColor = new Color(1f, 0.5f, 0f, 0.5f);

    [SerializeField]
    private Color dangerColor = new Color(1f, 0f, 0f, 0.7f);

    [SerializeField]
    private float flashSpeed = 2f;

    [Header("Audio")]
    [SerializeField]
    private AudioSource warningAudio;

    [SerializeField]
    private AudioClip warningSound;

    [Header("Player Reference (Optional - will auto-detect if empty)")]
    [SerializeField]
    private Plane playerPlaneRef;

    private List<Plane> allPlanes = new List<Plane>();
    private Plane playerPlane;
    private Transform playerTransform;

    private Vector3 minBounds;
    private Vector3 maxBounds;

    private bool isPlayerOutOfBounds = false;
    private bool isPlayerInDanger = false;
    private float playerOutOfBoundsTimer = 0f;
    private bool isAIControlling = false;

    private Dictionary<string, float> lastAiLogTime = new Dictionary<string, float>();
    private Dictionary<string, float> lastAiTurnBackLogTime = new Dictionary<string, float>();

    private bool uiCreationAttempted = false;

    private bool isPausedHidden = false;
    private bool wasWarningPanelActive = false;

    void Start()
    {
        var currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Menu and story" || currentScene == "Loading")
        {
            enabled = false;
            return;
        }

        minBounds = mapCenter - mapSize / 2f;
        maxBounds = mapCenter + mapSize / 2f;

        if (autoCreateUI && warningPanel == null)
        {
            CreateWarningUI();
        }

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
            Debug.Log($"[MapBoundary] Warning panel found/created: {warningPanel.name}");
        }
        else
        {
            Debug.LogWarning("[MapBoundary] Warning panel is NULL after initialization!");
        }

        FindAllPlanes();

        Debug.Log(
            $"[MapBoundary] Initialized. Bounds: {minBounds} to {maxBounds}, Warning distance: {warningDistance}m, Kill distance: {killDistance}m, Player: {(playerPlane != null ? playerPlane.DisplayName : "NOT FOUND")}"
        );
    }

    private void CreateWarningUI()
    {
        if (uiCreationAttempted)
            return;
        uiCreationAttempted = true;

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[MapBoundary] No Canvas found in scene, cannot create warning UI.");
            return;
        }

        Debug.Log("[MapBoundary] Auto-creating warning UI panel...");

        warningPanel = new GameObject("BoundaryWarningPanel");
        warningPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = warningPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -100f);
        panelRect.sizeDelta = new Vector2(600f, 120f);

        warningBackground = warningPanel.AddComponent<Image>();
        warningBackground.color = warningColor;
        warningBackground.raycastTarget = false;

        GameObject textObj = new GameObject("WarningText");
        textObj.transform.SetParent(warningPanel.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(20f, 10f);
        textRect.offsetMax = new Vector2(-20f, -10f);

        warningText = textObj.AddComponent<TextMeshProUGUI>();
        warningText.text = " WARNING: Approaching map boundary";
        warningText.fontSize = 28;
        warningText.fontStyle = FontStyles.Bold;
        warningText.color = Color.white;
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.textWrappingMode = TextWrappingModes.Normal;
        warningText.raycastTarget = false;

        GlobalFontManager.ApplyGlobalFont(warningText);

        var outline = warningPanel.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        warningPanel.SetActive(false);
        Debug.Log("[MapBoundary] Warning UI panel created successfully.");
    }

    void Update()
    {
        if (isPausedHidden)
            return;

        if (playerPlane == null)
        {
            FindAllPlanes();
        }

        foreach (var plane in allPlanes)
        {
            if (plane == null || plane.Dead)
                continue;

            bool isPlayer = (plane == playerPlane);
            CheckPlaneBoundary(plane, isPlayer);
        }

        if (Time.frameCount % 180 == 0 && playerPlane != null)
        {
            float distanceOutside = CalculateDistanceOutsideBounds(playerPlane.transform.position);
            Debug.Log(
                $"[MapBoundary] Player '{playerPlane.DisplayName}' status: OutOfBounds={isPlayerOutOfBounds}, InDanger={isPlayerInDanger}, AIControl={isAIControlling}, DistanceOutside={distanceOutside:F0}m, Panel={(warningPanel != null ? (warningPanel.activeSelf ? "ACTIVE" : "hidden") : "NULL")}"
            );
        }

        UpdatePlayerWarningUI();
    }

    private void FindAllPlanes()
    {
        allPlanes.Clear();

        if (playerPlaneRef != null)
        {
            playerPlane = playerPlaneRef;
            playerTransform = playerPlane.transform;
            Debug.Log($"[MapBoundary] Using manually assigned player: {playerPlane.DisplayName}");
        }

        Plane[] planes = FindObjectsByType<Plane>(FindObjectsSortMode.None);
        foreach (var plane in planes)
        {
            allPlanes.Add(plane);
        }

        if (playerPlane == null)
        {
            foreach (var plane in allPlanes)
            {
                if (plane == null)
                    continue;

                var aiController = plane.GetComponent<AIController>();
                if (aiController == null)
                {
                    playerPlane = plane;
                    playerTransform = plane.transform;
                    Debug.Log(
                        $"[MapBoundary] Found player (no AIController): {playerPlane.DisplayName}"
                    );
                    break;
                }
            }
        }

        if (playerPlane == null)
        {
            var playerController = FindAnyObjectByType<PlayerController>();
            if (playerController != null)
            {
                var planeCamera = playerController.GetComponent<PlaneCamera>();
                if (planeCamera != null)
                {
                    var foundPlane = planeCamera.GetComponentInParent<Plane>();
                    if (foundPlane == null)
                        foundPlane = planeCamera.GetComponentInChildren<Plane>();
                    if (foundPlane != null)
                    {
                        playerPlane = foundPlane;
                        playerTransform = playerPlane.transform;
                        Debug.Log(
                            $"[MapBoundary] Found player via PlaneCamera hierarchy: {playerPlane.DisplayName}"
                        );
                    }
                }
            }
        }

        if (playerPlane != null && !allPlanes.Contains(playerPlane))
        {
            allPlanes.Add(playerPlane);
        }

        if (playerPlane != null)
        {
            Debug.Log(
                $"[MapBoundary] Total: {allPlanes.Count} planes, player: {playerPlane.DisplayName}"
            );
        }
        else
        {
            Debug.LogWarning(
                $"[MapBoundary] Found {allPlanes.Count} planes, but player NOT FOUND! Please assign playerPlaneRef in Inspector."
            );
        }
    }

    private void CheckPlaneBoundary(Plane plane, bool isPlayer)
    {
        Transform planeTransform = plane.transform;
        float distanceOutside = CalculateDistanceOutsideBounds(planeTransform.position);

        if (isPlayer)
        {
            if (distanceOutside > killDistance)
            {
                isPlayerOutOfBounds = true;
                isPlayerInDanger = true;
                isAIControlling = true;
                playerOutOfBoundsTimer += Time.deltaTime;
                plane.ApplyDamage(Time.deltaTime * 50f, null, "Boundary", "OutOfBounds");

                ForcePlayerReturn(plane, planeTransform);
            }
            else if (distanceOutside > warningDistance)
            {
                isPlayerOutOfBounds = true;
                isPlayerInDanger = true;
                isAIControlling = true;
                playerOutOfBoundsTimer += Time.deltaTime;

                ForcePlayerReturn(plane, planeTransform);
            }
            else if (distanceOutside > 0)
            {
                isPlayerOutOfBounds = true;
                isPlayerInDanger = false;
                isAIControlling = false;
                playerOutOfBoundsTimer += Time.deltaTime;
            }
            else
            {
                isPlayerOutOfBounds = false;
                isPlayerInDanger = false;
                isAIControlling = false;
                playerOutOfBoundsTimer = 0f;
            }
        }
        else
        {
            if (distanceOutside > 0)
            {
                ForceAIReturn(plane, planeTransform);

                if (
                    !lastAiLogTime.ContainsKey(plane.DisplayName)
                    || Time.time - lastAiLogTime[plane.DisplayName] > 3f
                )
                {
                    string zoneType =
                        distanceOutside > killDistance ? "KILL ZONE"
                        : distanceOutside > warningDistance ? "DANGER ZONE"
                        : "WARNING ZONE";
                    Debug.LogWarning(
                        $"[MapBoundary]  AI OUT OF BOUNDS: '{plane.DisplayName}' | Zone: {zoneType} | Distance: {distanceOutside:F0}m | Position: {planeTransform.position}"
                    );
                    lastAiLogTime[plane.DisplayName] = Time.time;
                }
            }

            if (distanceOutside > killDistance)
            {
                float damage = Time.deltaTime * 50f;
                plane.ApplyDamage(damage, null, "Boundary", "OutOfBounds");

                if (
                    !lastAiTurnBackLogTime.ContainsKey(plane.DisplayName)
                    || Time.time - lastAiTurnBackLogTime[plane.DisplayName] > 2f
                )
                {
                    Debug.LogWarning(
                        $"[MapBoundary]  AI TAKING DAMAGE: '{plane.DisplayName}' | HP: {plane.Health:F0}/{plane.MaxHealth:F0} | Distance outside: {distanceOutside:F0}m"
                    );
                    lastAiTurnBackLogTime[plane.DisplayName] = Time.time;
                }
            }
        }
    }

    private void ForceAIReturn(Plane plane, Transform planeTransform)
    {
        Vector3 returnDir = GetReturnDirection(planeTransform.position);

        Vector3 localReturnDir = planeTransform.InverseTransformDirection(returnDir);

        float pitch = -Mathf.Clamp(localReturnDir.y * 2f, -1f, 1f);
        float yaw = Mathf.Clamp(localReturnDir.x * 2f, -1f, 1f);
        float roll = -Mathf.Clamp(localReturnDir.x, -1f, 1f);

        AIController ai = plane.GetComponent<AIController>();
        if (ai != null && ai.enabled)
        {
            plane.SetControlInput(new Vector3(pitch, yaw, roll));
        }
        else
        {
            plane.SetControlInput(new Vector3(pitch, yaw, roll));
        }

        plane.SetThrottleInput(1f);

        string key = plane.DisplayName + "_turnback";
        if (!lastAiTurnBackLogTime.ContainsKey(key) || Time.time - lastAiTurnBackLogTime[key] > 5f)
        {
            string returnDirText = GetDirectionText(returnDir);
            Debug.Log(
                $"[MapBoundary]  AI FORCED TURN-BACK: '{plane.DisplayName}' | Return direction: {returnDirText} | Controls(P/Y/R): {pitch:F2}/{yaw:F2}/{roll:F2}"
            );
            lastAiTurnBackLogTime[key] = Time.time;
        }
    }

    private void ForcePlayerReturn(Plane plane, Transform planeTransform)
    {
        Vector3 returnDir = GetReturnDirection(planeTransform.position);

        Vector3 localReturnDir = planeTransform.InverseTransformDirection(returnDir);

        float pitch = -Mathf.Clamp(localReturnDir.y * 2f, -1f, 1f);
        float yaw = Mathf.Clamp(localReturnDir.x * 2f, -1f, 1f);
        float roll = -Mathf.Clamp(localReturnDir.x, -1f, 1f);

        plane.SetControlInput(new Vector3(pitch, yaw, roll));
        plane.SetThrottleInput(1f);
    }

    private float CalculateDistanceOutsideBounds(Vector3 pos)
    {
        float distanceX = 0f,
            distanceY = 0f,
            distanceZ = 0f;

        if (pos.x < minBounds.x)
            distanceX = minBounds.x - pos.x;
        else if (pos.x > maxBounds.x)
            distanceX = pos.x - maxBounds.x;

        if (pos.y < minBounds.y)
            distanceY = minBounds.y - pos.y;
        else if (pos.y > maxBounds.y)
            distanceY = pos.y - maxBounds.y;

        if (pos.z < minBounds.z)
            distanceZ = minBounds.z - pos.z;
        else if (pos.z > maxBounds.z)
            distanceZ = pos.z - maxBounds.z;

        return Mathf.Max(distanceX, distanceY, distanceZ);
    }

    private Vector3 GetReturnDirection(Vector3 pos)
    {
        Vector3 clampedPos = new Vector3(
            Mathf.Clamp(pos.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(pos.y, minBounds.y, maxBounds.y),
            Mathf.Clamp(pos.z, minBounds.z, maxBounds.z)
        );
        return (clampedPos - pos).normalized;
    }

    private void UpdatePlayerWarningUI()
    {
        if (playerTransform == null)
        {
            FindAllPlanes();
            return;
        }

        if (warningPanel == null)
        {
            if (autoCreateUI && !uiCreationAttempted)
            {
                CreateWarningUI();
            }
            if (warningPanel == null && isPlayerOutOfBounds && Time.frameCount % 300 == 0)
            {
                Debug.LogError(
                    "[MapBoundary] Warning UI Panel reference is MISSING! Cannot show warning."
                );
            }
            return;
        }

        if (isPlayerOutOfBounds)
        {
            warningPanel.SetActive(true);

            if (warningText != null && !warningText.gameObject.activeInHierarchy)
            {
                warningText.gameObject.SetActive(true);
            }

            Vector3 returnDirection = GetReturnDirection(playerTransform.position);
            string directionText = GetDirectionText(returnDirection);

            if (warningText != null)
            {
                string newText;
                if (isAIControlling)
                {
                    newText = $"LEAVING COMBAT ZONE!\n{directionText}\nAUTO-PILOT ENGAGED";
                }
                else if (isPlayerInDanger)
                {
                    newText = $"DANGER! OUT OF BOUNDS!\n{directionText}\nTAKING DAMAGE!";
                }
                else
                {
                    newText = $"WARNING: MAP BOUNDARY\n{directionText}";
                }

                warningText.text = newText;

                warningText.color = Color.white;
                warningText.enableAutoSizing = true;
                warningText.fontSizeMin = 14;
                warningText.fontSizeMax = 32;

                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log(
                        $"[MapBoundary] Text set to: '{newText}', Component: {warningText.name}, Actual text: '{warningText.text}'"
                    );
                }
            }
            else
            {
                if (Time.frameCount % 60 == 0)
                {
                    Debug.LogWarning("[MapBoundary] warningText is NULL! Cannot set warning text.");
                }
            }

            if (warningBackground != null)
            {
                Color targetColor;
                if (isAIControlling)
                {
                    targetColor = new Color(0.8f, 0.2f, 0.8f, 0.7f);
                }
                else if (isPlayerInDanger)
                {
                    targetColor = dangerColor;
                }
                else
                {
                    targetColor = warningColor;
                }

                float flash = (Mathf.Sin(Time.time * flashSpeed * Mathf.PI) + 1f) / 2f;
                targetColor.a *= (0.5f + flash * 0.5f);
                warningBackground.color = targetColor;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayBoundaryWarning();
            }
            else if (warningAudio != null && warningSound != null && !warningAudio.isPlaying)
            {
                warningAudio.clip = warningSound;
                warningAudio.loop = true;
                warningAudio.Play();
            }
        }
        else
        {
            warningPanel.SetActive(false);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopBoundaryWarning();
            }
            if (warningAudio != null && warningAudio.isPlaying)
            {
                warningAudio.Stop();
            }
        }
    }

    private string GetDirectionText(Vector3 direction)
    {
        string text = "TURN: ";
        bool hasDirection = false;

        if (direction.x > 0.3f)
        {
            text += "RIGHT ";
            hasDirection = true;
        }
        else if (direction.x < -0.3f)
        {
            text += "LEFT ";
            hasDirection = true;
        }

        if (direction.z > 0.3f)
        {
            text += "NORTH ";
            hasDirection = true;
        }
        else if (direction.z < -0.3f)
        {
            text += "SOUTH ";
            hasDirection = true;
        }

        if (direction.y > 0.3f)
        {
            text += "DOWN";
            hasDirection = true;
        }
        else if (direction.y < -0.3f)
        {
            text += "UP";
            hasDirection = true;
        }

        return hasDirection ? text.Trim() : "TURN AROUND";
    }

    public void RegisterPlane(Plane plane)
    {
        if (!allPlanes.Contains(plane))
        {
            allPlanes.Add(plane);
        }
    }

    public void UnregisterPlane(Plane plane)
    {
        allPlanes.Remove(plane);
    }

    public void HideOverlays()
    {
        isPausedHidden = true;

        if (warningPanel != null)
        {
            wasWarningPanelActive = warningPanel.activeSelf;
            warningPanel.SetActive(false);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBoundaryWarning();
        }
        if (warningAudio != null && warningAudio.isPlaying)
        {
            warningAudio.Pause();
        }
    }

    public void ShowOverlays()
    {
        isPausedHidden = false;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 center = mapCenter;
        Vector3 size = mapSize;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(center, size + Vector3.one * warningDistance * 2);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size + Vector3.one * killDistance * 2);
    }
}
