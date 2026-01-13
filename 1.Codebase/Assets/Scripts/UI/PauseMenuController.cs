using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PauseMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField]
    private UIDocument uiDocument;

    [Header("References")]
    [SerializeField]
    private MainMenuController mainMenuController;

    [SerializeField]
    private SettingsController settingsController;

    [Header("Scene Names")]
    [SerializeField]
    private string mainMenuSceneName = "Menu and story";

    private VisualElement root;
    private VisualElement pauseMenuOverlay;
    private Button resumeButton;
    private Button restartButton;
    private Button settingsButton;
    private Button mainMenuButton;
    private Button quitButton;

    private bool isPaused = false;

    private static PauseMenuController activeInstance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        activeInstance = null;
    }

    private void Awake()
    {
        if (activeInstance != null && activeInstance != this && activeInstance.enabled)
        {
            enabled = false;
            return;
        }
        activeInstance = this;

        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument != null && uiDocument.visualTreeAsset != null)
        {
            string assetName = uiDocument.visualTreeAsset.name;
            if (
                assetName.Contains("Settings")
                || (!assetName.Contains("Pause") && !assetName.Contains("Menu"))
            )
            {
                Debug.LogWarning(
                    $"[PauseMenu] UIDocument has incorrect asset assigned: '{assetName}'. Creating separate child UIDocument for Pause Menu."
                );

                GameObject childGO = new GameObject("PauseMenuUI");
                childGO.transform.SetParent(transform);
                uiDocument = childGO.AddComponent<UIDocument>();
            }
        }

        bool needsSetup = uiDocument == null;
        if (needsSetup)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        if (uiDocument != null)
        {
            uiDocument.sortingOrder = 999;

            if (uiDocument.visualTreeAsset == null)
            {
                Debug.Log("[PauseMenu] UIDocument has no UXML assigned, attempting to load...");

                if (uiDocument.panelSettings == null)
                {
                    var panelSettings = Resources.Load<PanelSettings>("PanelSettings");
                    if (panelSettings == null)
                    {
                        var settings = Resources.LoadAll<PanelSettings>("");
                        if (settings.Length > 0)
                            panelSettings = settings[0];
                    }

                    if (panelSettings == null)
                    {
#if UNITY_EDITOR
                        panelSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<PanelSettings>(
                            "Assets/UI Toolkit/PanelSettings.asset"
                        );
#endif
                    }

                    if (panelSettings != null)
                    {
                        uiDocument.panelSettings = panelSettings;
                    }
                }

                var pauseMenuUxml = Resources.Load<VisualTreeAsset>("UI/PauseMenu");
                if (pauseMenuUxml == null)
                {
#if UNITY_EDITOR
                    pauseMenuUxml = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        "Assets/UI/UXML/PauseMenu.uxml"
                    );
#endif
                }
                if (pauseMenuUxml != null)
                {
                    uiDocument.visualTreeAsset = pauseMenuUxml;
                    Debug.Log("[PauseMenu] Auto-loaded PauseMenu.uxml successfully!");
                }
                else
                {
                    Debug.LogError(
                        "[PauseMenu] Could not load PauseMenu.uxml! Please assign UIDocument.sourceAsset manually in the Inspector."
                    );
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
            activeInstance = null;
    }

    private void OnEnable()
    {
        StartCoroutine(DelayedInit());
    }

    private System.Collections.IEnumerator DelayedInit()
    {
        yield return null;

        if (uiDocument == null)
        {
            Debug.LogError("[PauseMenu] UIDocument is null!");
            yield break;
        }

        root = uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[PauseMenu] Root visual element is null!");
            yield break;
        }

        BindUIElements();
        RegisterCallbacks();

        SetPauseMenuVisible(false);
    }

    private void Start()
    {
        if (!enabled)
            return;

        EnsureReferences();

        isPaused = false;

        Debug.Log("[PauseMenu] Initialized and hidden");
    }

    private void Update()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene != "Main")
            return;

        if (mainMenuController != null && !mainMenuController.IsGameStarted())
            return;

        if (settingsController != null && settingsController.IsSettingsOpen)
        {
            return;
        }

        if (IsPausePressedThisFrame())
        {
            Debug.Log($"[PauseMenu] ESC pressed: isPaused={isPaused}");
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    private VisualElement pauseMenuRoot;

    private void BindUIElements()
    {
        pauseMenuRoot = root.Q<VisualElement>("root");

        if (pauseMenuRoot == null)
        {
            Debug.LogWarning(
                "[PauseMenu] Could not find inner 'root' element, using document root"
            );
            pauseMenuRoot = root;
        }

        pauseMenuOverlay = root.Q<VisualElement>("overlay");
        resumeButton = root.Q<Button>("resume-button");
        restartButton = root.Q<Button>("restart-button");
        settingsButton = root.Q<Button>("settings-button");
        mainMenuButton = root.Q<Button>("main-menu-button");

        quitButton = null;

        Debug.Log(
            $"[PauseMenu] BindUIElements: pauseMenuRoot={pauseMenuRoot != null}, overlay={pauseMenuOverlay != null}, resumeBtn={resumeButton != null}"
        );
    }

    private void RegisterCallbacks()
    {
        resumeButton?.RegisterCallback<ClickEvent>(evt => ResumeGame());
        restartButton?.RegisterCallback<ClickEvent>(evt => RestartMission());
        settingsButton?.RegisterCallback<ClickEvent>(evt => OpenSettings());
        mainMenuButton?.RegisterCallback<ClickEvent>(evt => ReturnToMainMenu());
        quitButton?.RegisterCallback<ClickEvent>(evt => QuitGame());

        resumeButton?.RegisterCallback<NavigationSubmitEvent>(evt => ResumeGame());
        restartButton?.RegisterCallback<NavigationSubmitEvent>(evt => RestartMission());
        settingsButton?.RegisterCallback<NavigationSubmitEvent>(evt => OpenSettings());
        mainMenuButton?.RegisterCallback<NavigationSubmitEvent>(evt => ReturnToMainMenu());
        quitButton?.RegisterCallback<NavigationSubmitEvent>(evt => QuitGame());
    }

    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PauseGameAudio();
        }

        HideGameOverlays();

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        ShowPauseMenu();

        Debug.Log("Game paused");
    }

    public void ResumeGame()
    {
        isPaused = false;

        SetPauseMenuVisible(false);

        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResumeGameAudio();
        }

        ShowGameOverlays();

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;

        Debug.Log("Game resumed");
    }

    public void RestartMission()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetPauseMenuVisible(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("Restarting mission");
    }

    public void OpenSettings()
    {
        if (!isPaused)
            PauseGame();

        SetPauseMenuVisible(false);

        if (settingsController != null)
        {
            settingsController.OpenSettings();
        }
    }

    public void ReturnToMainMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        SetPauseMenuVisible(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllGameSounds();
        }

        MainMenuController.ShouldStartImmediately = false;

        if (mainMenuController != null)
        {
            mainMenuController.ShowMainMenu();
            return;
        }

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            MainMenuController.ShouldStartImmediately = false;
            SceneManager.LoadScene(mainMenuSceneName);
        }

        Debug.Log("Returning to main menu");
    }

    public void QuitGame()
    {
        Debug.Log("Exiting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowPauseMenu()
    {
        SetPauseMenuVisible(true);
        resumeButton?.Focus();
    }

    public bool IsPaused() => isPaused;

    private void SetPauseMenuVisible(bool visible)
    {
        var displayStyle = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.style.display = displayStyle;

            if (visible)
            {
                pauseMenuRoot.style.position = Position.Absolute;
                pauseMenuRoot.style.top = 0;
                pauseMenuRoot.style.left = 0;
                pauseMenuRoot.style.right = 0;
                pauseMenuRoot.style.bottom = 0;
                pauseMenuRoot.style.width = Length.Percent(100);
                pauseMenuRoot.style.height = Length.Percent(100);
                pauseMenuRoot.style.opacity = 1f;
                pauseMenuRoot.BringToFront();

                if (root != null)
                {
                    root.style.display = DisplayStyle.Flex;
                    root.style.opacity = 1f;
                }
            }

            Debug.Log(
                $"[PauseMenu] SetPauseMenuVisible({visible}): pauseMenuRoot display = {displayStyle}"
            );
        }
        else
        {
            Debug.LogError(
                "[PauseMenu] SetPauseMenuVisible: pauseMenuRoot is NULL! Cannot show/hide pause menu."
            );
        }
    }

    private void EnsureReferences()
    {
        if (mainMenuController == null)
            mainMenuController = FindFirstObjectByType<MainMenuController>();
        if (settingsController == null)
            settingsController = FindFirstObjectByType<SettingsController>();
    }

    private void HideGameOverlays()
    {
        var gForceEffects = FindFirstObjectByType<GForceEffects>();
        if (gForceEffects != null)
        {
            gForceEffects.HideOverlays();
        }

        var threatWarning = FindFirstObjectByType<ThreatWarningSystem>();
        if (threatWarning != null)
        {
            threatWarning.HideOverlays();
        }

        var boundaryWarning = FindFirstObjectByType<MapBoundaryWarning>();
        if (boundaryWarning != null)
        {
            boundaryWarning.HideOverlays();
        }
    }

    private void ShowGameOverlays()
    {
        var gForceEffects = FindFirstObjectByType<GForceEffects>();
        if (gForceEffects != null)
        {
            gForceEffects.ShowOverlays();
        }

        var threatWarning = FindFirstObjectByType<ThreatWarningSystem>();
        if (threatWarning != null)
        {
            threatWarning.ShowOverlays();
        }

        var boundaryWarning = FindFirstObjectByType<MapBoundaryWarning>();
        if (boundaryWarning != null)
        {
            boundaryWarning.ShowOverlays();
        }
    }

    private static bool IsPausePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }
}
