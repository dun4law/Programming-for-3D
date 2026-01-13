using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class StartupLogoSequenceUIToolkit : MonoBehaviour
{
    private const string ResourcesFolderPath = "StartupLogos";
    private const string AudioResourcesFolderPath = "StartupLogos/Audio";

    private const float FadeSeconds = 0.6f;
    private const float HoldSeconds = 1.5f;
    private const float BetweenLogosSeconds = 0.2f;

    private static bool hasRun;
    private static bool isPlaying;

    public static bool IsPlaying => isPlaying;

    private AudioSource audioSource;
    private List<AudioClip> logoSounds = new List<AudioClip>();

    private Canvas canvas;
    private UnityEngine.UI.RawImage backgroundImage;
    private UnityEngine.UI.RawImage logoImage;
    private CanvasGroup logoCanvasGroup;

    private List<Canvas> disabledCanvases = new List<Canvas>();
    private List<UIDocument> disabledUIDocuments = new List<UIDocument>();
    private Dictionary<UIDocument, DisplayStyle> hiddenUIRoots =
        new Dictionary<UIDocument, DisplayStyle>();

    private static readonly HashSet<string> ExcludedNames = new HashSet<string>(
        System.StringComparer.OrdinalIgnoreCase
    )
    {
        "bg",
        "game paused",
        "settiing ui",
        "setting ui",
        "settings ui",
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        hasRun = false;
        isPlaying = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OnAfterFirstSceneLoad()
    {
        if (hasRun)
            return;

        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex != 0)
            return;

        hasRun = true;
        isPlaying = true;

        var host = new GameObject("StartupLogoSequence");
        host.hideFlags = HideFlags.DontSave;
        DontDestroyOnLoad(host);
        host.AddComponent<StartupLogoSequenceUIToolkit>();
    }

    private void Awake()
    {
        HideAllOtherUI();

        BuildUGUIOverlay();
    }

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    private void HideAllOtherUI()
    {
        var allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in allCanvases)
        {
            if (c != canvas && c.enabled)
            {
                c.enabled = false;
                disabledCanvases.Add(c);
            }
        }

        var allUIDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        Debug.Log(
            $"[StartupLogo] Hidden {disabledCanvases.Count} Canvases, found {allUIDocuments.Length} UIDocuments (not hidden - controllers manage themselves)"
        );
    }

    private void BuildUGUIOverlay()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        var scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(transform, false);
        backgroundImage = bgObj.AddComponent<UnityEngine.UI.RawImage>();
        backgroundImage.color = Color.black;

        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        var logoContainer = new GameObject("LogoContainer");
        logoContainer.transform.SetParent(transform, false);

        var containerRect = logoContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        logoCanvasGroup = logoContainer.AddComponent<CanvasGroup>();
        logoCanvasGroup.alpha = 0f;

        var logoObj = new GameObject("Logo");
        logoObj.transform.SetParent(logoContainer.transform, false);
        logoImage = logoObj.AddComponent<UnityEngine.UI.RawImage>();
        logoImage.color = Color.white;

        var logoRect = logoObj.GetComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.1f, 0.1f);
        logoRect.anchorMax = new Vector2(0.9f, 0.9f);
        logoRect.offsetMin = Vector2.zero;
        logoRect.offsetMax = Vector2.zero;

        var aspectFitter = logoObj.AddComponent<UnityEngine.UI.AspectRatioFitter>();
        aspectFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
        aspectFitter.aspectRatio = 1.0f;

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.ignoreListenerPause = true;

        Debug.Log("[StartupLogo] UGUI overlay built");
    }

    private List<Texture2D> LoadLogos()
    {
        var textures = Resources.LoadAll<Texture2D>(ResourcesFolderPath);
        if (textures == null || textures.Length == 0)
        {
            Debug.LogWarning(
                $"[StartupLogo] No textures found in Resources/{ResourcesFolderPath}/"
            );
            return new List<Texture2D>();
        }

        var logos = textures
            .Where(t => t != null && !ExcludedNames.Contains(t.name))
            .OrderBy(t => t.name, System.StringComparer.OrdinalIgnoreCase)
            .ToList();

        LoadLogoSounds(logos.Count);

        Debug.Log(
            $"[StartupLogo] Loaded {logos.Count} logos (excluded {textures.Length - logos.Count} menu backgrounds)"
        );
        return logos;
    }

    private void LoadLogoSounds(int logoCount)
    {
        logoSounds.Clear();

        Debug.Log(
            $"[StartupLogo] LoadLogoSounds called - AudioManager.Instance={(AudioManager.Instance != null ? "EXISTS" : "NULL")}"
        );

        if (AudioManager.Instance != null)
        {
            var managerSounds = AudioManager.Instance.GetStartupLogoSounds();
            Debug.Log(
                $"[StartupLogo] AudioManager.GetStartupLogoSounds returned: {(managerSounds != null ? managerSounds.Length + " clips" : "NULL")}"
            );
            if (managerSounds != null && managerSounds.Length > 0)
            {
                foreach (var clip in managerSounds)
                {
                    if (clip != null)
                    {
                        logoSounds.Add(clip);
                        Debug.Log($"[StartupLogo] Added sound from AudioManager: {clip.name}");
                    }
                }
                Debug.Log($"[StartupLogo] Loaded {logoSounds.Count} audio clips from AudioManager");
                return;
            }
            else
            {
                Debug.LogWarning(
                    "[StartupLogo] AudioManager exists but startupLogoSounds array is empty! Assign sounds in Inspector."
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[StartupLogo] AudioManager.Instance is NULL - it may not be initialized yet. Falling back to Resources folder."
            );
        }

        var audioClips = Resources.LoadAll<AudioClip>(AudioResourcesFolderPath);
        Debug.Log(
            $"[StartupLogo] Resources.LoadAll from '{AudioResourcesFolderPath}' found: {(audioClips != null ? audioClips.Length + " clips" : "NULL")}"
        );
        if (audioClips != null && audioClips.Length > 0)
        {
            var sortedClips = audioClips
                .Where(c => c != null)
                .OrderBy(c => c.name, System.StringComparer.OrdinalIgnoreCase)
                .ToList();

            logoSounds.AddRange(sortedClips);
            foreach (var clip in sortedClips)
            {
                Debug.Log($"[StartupLogo] Added sound from Resources: {clip.name}");
            }
            Debug.Log($"[StartupLogo] Loaded {logoSounds.Count} audio clips from Resources");
        }
        else
        {
            Debug.LogWarning(
                $"[StartupLogo] No audio clips found! Either:\n"
                    + $"  1. Assign clips to AudioManager's 'Startup Logo Sounds' in Inspector\n"
                    + $"  2. Place audio files in Resources/{AudioResourcesFolderPath}/"
            );
        }
    }

    private IEnumerator PlaySequence()
    {
        yield return null;

        var logos = LoadLogos();

        if (logos.Count == 0)
        {
            Debug.LogWarning("[StartupLogo] No logos to display");
            Cleanup();
            yield break;
        }

        int logoIndex = 0;
        foreach (var logo in logos)
        {
            if (IsSkipPressed())
                break;

            Debug.Log($"[StartupLogo] Displaying: {logo.name}");

            PlayLogoSound(logoIndex);

            logoImage.texture = logo;

            var aspectFitter = logoImage.GetComponent<UnityEngine.UI.AspectRatioFitter>();
            if (aspectFitter != null && logo.height > 0)
            {
                aspectFitter.aspectRatio = (float)logo.width / logo.height;
            }

            yield return Fade(0f, 1f, FadeSeconds);

            float holdTimer = HoldSeconds;
            while (holdTimer > 0f)
            {
                if (IsSkipPressed())
                    break;
                holdTimer -= Time.unscaledDeltaTime;
                yield return null;
            }

            yield return Fade(1f, 0f, FadeSeconds);

            if (BetweenLogosSeconds > 0f)
            {
                float betweenTimer = BetweenLogosSeconds;
                while (betweenTimer > 0f)
                {
                    if (IsSkipPressed())
                        break;
                    betweenTimer -= Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            logoIndex++;
        }

        Cleanup();
    }

    private void PlayLogoSound(int index)
    {
        if (audioSource == null)
            return;
        if (logoSounds == null || logoSounds.Count == 0)
            return;

        if (index < logoSounds.Count && logoSounds[index] != null)
        {
            audioSource.PlayOneShot(logoSounds[index]);
            Debug.Log($"[StartupLogo] Playing sound: {logoSounds[index].name}");
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (logoCanvasGroup == null)
            yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (IsSkipPressed())
            {
                logoCanvasGroup.alpha = to;
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            logoCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        logoCanvasGroup.alpha = to;
    }

    private void Cleanup()
    {
        isPlaying = false;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var c in disabledCanvases)
        {
            if (c != null)
                c.enabled = true;
        }
        disabledCanvases.Clear();

        hiddenUIRoots.Clear();
        disabledUIDocuments.Clear();

        Debug.Log("[StartupLogo] Sequence complete, all UI restored");

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (isPlaying)
        {
            isPlaying = false;

            foreach (var c in disabledCanvases)
            {
                if (c != null)
                    c.enabled = true;
            }
        }
    }

    private static bool IsSkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
            return true;

        var mouse = Mouse.current;
        if (
            mouse != null
            && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
        )
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.anyKeyDown)
            return true;
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            return true;
#endif

        return false;
    }
}
