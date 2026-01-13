using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalFontManager : MonoBehaviour
{
    [Header("Font Settings")]
    [Tooltip(
        "The TMP Font Asset to apply globally. Create this from Ticketing.ttf using Window > TextMeshPro > Font Asset Creator"
    )]
    [SerializeField]
    private TMP_FontAsset globalFont;

    [Tooltip("Apply font to newly instantiated TMP components")]
    [SerializeField]
    private bool applyToNewComponents = true;

    [Header("Timing Settings")]
    [Tooltip("Delay before first font application after scene load (seconds)")]
    [SerializeField]
    private float initialDelay = 0.1f;

    [Tooltip(
        "How often to check for new TMP components (seconds). Set to 0 to disable periodic checks."
    )]
    [SerializeField]
    private float periodicCheckInterval = 1.0f;

    [Tooltip("How many periodic checks to perform after scene load. Set to 0 for unlimited.")]
    [SerializeField]
    private int maxPeriodicChecks = 5;

    private static GlobalFontManager instance;
    private static TMP_FontAsset cachedFont;
    private int periodicCheckCount = 0;
    private Coroutine periodicCheckCoroutine;

    public static TMP_FontAsset GlobalFont => cachedFont;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (globalFont != null)
        {
            cachedFont = globalFont;
        }
        else
        {
            Debug.LogWarning(
                "[GlobalFontManager] No global font assigned! Please create a TMP Font Asset from Ticketing.ttf and assign it."
            );
        }
    }

    private void Start()
    {
        if (globalFont != null)
        {
            StartCoroutine(ApplyFontWithDelay(initialDelay));
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        StopPeriodicCheck();
    }

    private void OnSceneLoaded(
        UnityEngine.SceneManagement.Scene scene,
        UnityEngine.SceneManagement.LoadSceneMode mode
    )
    {
        if (globalFont != null)
        {
            periodicCheckCount = 0;
            StartCoroutine(ApplyFontWithDelay(initialDelay));
        }
    }

    private IEnumerator ApplyFontWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyFontToAllTMPComponents();

        if (applyToNewComponents && periodicCheckInterval > 0)
        {
            StartPeriodicCheck();
        }
    }

    private void StartPeriodicCheck()
    {
        StopPeriodicCheck();
        periodicCheckCoroutine = StartCoroutine(PeriodicFontCheck());
    }

    private void StopPeriodicCheck()
    {
        if (periodicCheckCoroutine != null)
        {
            StopCoroutine(periodicCheckCoroutine);
            periodicCheckCoroutine = null;
        }
    }

    private IEnumerator PeriodicFontCheck()
    {
        while (maxPeriodicChecks == 0 || periodicCheckCount < maxPeriodicChecks)
        {
            yield return new WaitForSeconds(periodicCheckInterval);
            periodicCheckCount++;

            int applied = ApplyFontToAllTMPComponentsInternal(logIfZero: false);
            if (applied > 0)
            {
                Debug.Log(
                    $"[GlobalFontManager] Periodic check #{periodicCheckCount}: Applied font to {applied} new TMP components."
                );
            }
        }

        Debug.Log($"[GlobalFontManager] Periodic checks completed ({maxPeriodicChecks} checks).");
        periodicCheckCoroutine = null;
    }

    public void ApplyFontToAllTMPComponents()
    {
        ApplyFontToAllTMPComponentsInternal(logIfZero: true);
    }

    private int ApplyFontToAllTMPComponentsInternal(bool logIfZero)
    {
        if (globalFont == null)
        {
            Debug.LogError("[GlobalFontManager] Cannot apply font - globalFont is null!");
            return 0;
        }

        TMP_Text[] allTMPTexts = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        int count = 0;
        foreach (TMP_Text tmpText in allTMPTexts)
        {
            if (tmpText.font != globalFont)
            {
                tmpText.font = globalFont;
                count++;
            }
        }

        if (count > 0 || logIfZero)
        {
            Debug.Log(
                $"[GlobalFontManager] Applied '{globalFont.name}' font to {count} TMP components. (Total found: {allTMPTexts.Length})"
            );
        }

        return count;
    }

    public static void ApplyGlobalFont(TMP_Text tmpText)
    {
        if (cachedFont != null && tmpText != null)
        {
            tmpText.font = cachedFont;
        }
    }

    public static void ForceRefresh()
    {
        if (instance != null)
        {
            instance.ApplyFontToAllTMPComponents();
        }
    }

    [ContextMenu("Refresh All Fonts")]
    public void RefreshAllFonts()
    {
        ApplyFontToAllTMPComponents();
    }
}
