using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField]
    private UIDocument uiDocument;

    [Header("Settings")]
    [SerializeField]
    private string gameplaySceneName = "Main";

    [SerializeField]
    private string loadingSceneName = "Loading";

    [SerializeField]
    private bool startWithMenuOpen = true;

    [Header("References")]
    [SerializeField]
    private SettingsController settingsController;

    [SerializeField]
    private HangarController hangarController;

    [SerializeField]
    private PlayerStatsController playerStatsController;

    [SerializeField]
    private HowToPlayController howToPlayController;

    [SerializeField]
    private StoryIntroController storyIntroController;

    [SerializeField]
    private GameObject gameUI;

    private VisualElement root;
    private VisualElement mainMenuOverlay;
    private VisualElement creditsPanel;
    private Button creditsTabGeneral;
    private Button creditsTabAircraft;
    private Button creditsTabAudio;
    private VisualElement creditsPageGeneral;
    private VisualElement creditsPageAircraft;
    private VisualElement creditsPageAudio;
    private Button startButton;
    private Button profileButton;
    private Button settingsButton;
    private Button howToPlayButton;
    private Button creditsButton;
    private Button creditsCloseButton;
    private Button quitButton;

    private Button githubButton;
    private Button youtubeButton;

    private VisualElement newBadgesPopup;
    private VisualElement newBadgesContainer;
    private Label newBadgesSubtitle;
    private Button badgesCloseButton;
    private Button badgesViewAllButton;

    private VisualElement[] flyingJetsLTR;
    private VisualElement[] flyingJetsRTL;
    private bool jetsAnimationStarted = false;
    private Texture2D[] jetTextures;
    private Texture2D explosionTexture;

    private const string GITHUB_URL = "https://github.com/dun4law/Programming-for-3D";
    private const string YOUTUBE_URL = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

    private bool gameStarted = false;
    private bool menuShown = false;
    private bool skipHangarSelection = false;

    private static MainMenuController activeInstance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        activeInstance = null;
        ShouldStartImmediately = false;
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

        bool needsSetup = uiDocument == null;
        if (needsSetup)
        {
            if (
                uiDocument != null
                && uiDocument.visualTreeAsset != null
                && uiDocument.visualTreeAsset.name != "MainMenu"
            )
            {
                GameObject childGO = new GameObject("MainMenuUI");
                childGO.transform.SetParent(transform);
                uiDocument = childGO.AddComponent<UIDocument>();
            }
            else if (uiDocument == null)
            {
                uiDocument = gameObject.AddComponent<UIDocument>();
            }
        }

        if (uiDocument != null)
        {
            uiDocument.sortingOrder = 100;

            if (uiDocument.visualTreeAsset == null || uiDocument.visualTreeAsset.name != "MainMenu")
            {
                if (uiDocument.panelSettings == null)
                {
                    var panelSettings = Resources.Load<PanelSettings>("PanelSettings");
                    if (panelSettings == null)
                    {
                        var settings = Resources.LoadAll<PanelSettings>("");
                        if (settings.Length > 0)
                            panelSettings = settings[0];
                    }
                    if (panelSettings != null)
                        uiDocument.panelSettings = panelSettings;
                }

                var mainMenuUxml = Resources.Load<VisualTreeAsset>("UI/MainMenu");
                if (mainMenuUxml == null)
                {
#if UNITY_EDITOR
                    mainMenuUxml = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        "Assets/UI/UXML/MainMenu.uxml"
                    );
#endif
                }

                if (mainMenuUxml != null)
                {
                    uiDocument.visualTreeAsset = mainMenuUxml;
                    Debug.Log("[MainMenu] Auto-loaded MainMenu.uxml");
                }
                else
                {
                    Debug.LogError("[MainMenu] Could not load MainMenu.uxml!");
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
        if (uiDocument == null)
            return;

        root = uiDocument.rootVisualElement;
        if (root == null)
            return;

        root.style.display = DisplayStyle.None;

        mainMenuOverlay = root.Q<VisualElement>("left-panel");
        creditsPanel = root.Q<VisualElement>("credits-panel");
        startButton = root.Q<Button>("new-game-button");
        profileButton = root.Q<Button>("profile-button");
        settingsButton = root.Q<Button>("settings-button");
        howToPlayButton = root.Q<Button>("how-to-play-button");
        creditsButton = root.Q<Button>("credits-button");
        creditsCloseButton = root.Q<Button>("credits-close-button");
        creditsTabGeneral = root.Q<Button>("credits-tab-general");
        creditsTabAircraft = root.Q<Button>("credits-tab-aircraft");
        creditsTabAudio = root.Q<Button>("credits-tab-audio");
        creditsPageGeneral = root.Q<VisualElement>("credits-page-general");
        creditsPageAircraft = root.Q<VisualElement>("credits-page-aircraft");
        creditsPageAudio = root.Q<VisualElement>("credits-page-audio");
        quitButton = root.Q<Button>("exit-button");

        githubButton = root.Q<Button>("github-button");
        youtubeButton = root.Q<Button>("youtube-button");

        newBadgesPopup = root.Q<VisualElement>("new-badges-popup");
        newBadgesContainer = root.Q<VisualElement>("new-badges-container");
        newBadgesSubtitle = root.Q<Label>("new-badges-subtitle");
        badgesCloseButton = root.Q<Button>("badges-close-button");
        badgesViewAllButton = root.Q<Button>("badges-view-all-button");

        flyingJetsLTR = new VisualElement[5];
        flyingJetsRTL = new VisualElement[5];
        for (int i = 0; i < 5; i++)
        {
            flyingJetsLTR[i] = root.Q<VisualElement>(className: $"jet-ltr-{i + 1}");
            flyingJetsRTL[i] = root.Q<VisualElement>(className: $"jet-rtl-{i + 1}");
        }

        jetTextures = new Texture2D[3];
        jetTextures[0] = Resources.Load<Texture2D>("UI/Icons/flying-jet-new");
        jetTextures[1] = Resources.Load<Texture2D>("UI/Icons/flying-jet-f16");
        jetTextures[2] = Resources.Load<Texture2D>("UI/Icons/flying-jet-su27");

        explosionTexture = Resources.Load<Texture2D>("UI/Icons/explosion-fx");

        Debug.Log(
            $"[MainMenu] Loaded jet textures: F15={jetTextures[0] != null}, F16={jetTextures[1] != null}, Su27={jetTextures[2] != null}, Explosion={explosionTexture != null}"
        );

        for (int i = 0; i < 5; i++)
        {
            VisualElement jetLtr = flyingJetsLTR[i];
            VisualElement jetRtl = flyingJetsRTL[i];

            if (jetLtr != null)
            {
                jetLtr.pickingMode = PickingMode.Position;
                jetLtr.RegisterCallback<PointerDownEvent>(evt => OnJetClicked(jetLtr));
            }
            if (jetRtl != null)
            {
                jetRtl.pickingMode = PickingMode.Position;
                jetRtl.RegisterCallback<PointerDownEvent>(evt => OnJetClicked(jetRtl));
            }
        }

        var jetsContainer = root.Q<VisualElement>(className: "flying-jets-container");
        if (jetsContainer != null)
        {
            jetsContainer.pickingMode = PickingMode.Ignore;
            jetsContainer.BringToFront();
        }

        Debug.Log(
            $"[MainMenu] Registered click events for {flyingJetsLTR.Length + flyingJetsRTL.Length} jets"
        );

        RegisterButtonSounds(startButton);
        RegisterButtonSounds(profileButton);
        RegisterButtonSounds(settingsButton);
        RegisterButtonSounds(howToPlayButton);
        RegisterButtonSounds(creditsButton);
        RegisterButtonSounds(creditsCloseButton);
        RegisterButtonSounds(creditsTabGeneral);
        RegisterButtonSounds(creditsTabAircraft);
        RegisterButtonSounds(creditsTabAudio);
        RegisterButtonSounds(quitButton);
        RegisterButtonSounds(githubButton);
        RegisterButtonSounds(youtubeButton);
        RegisterButtonSounds(badgesCloseButton);
        RegisterButtonSounds(badgesViewAllButton);

        startButton?.RegisterCallback<ClickEvent>(evt => OnStartGame());
        profileButton?.RegisterCallback<ClickEvent>(evt => OnProfile());
        settingsButton?.RegisterCallback<ClickEvent>(evt => OnSettings());
        howToPlayButton?.RegisterCallback<ClickEvent>(evt => OnHowToPlay());
        creditsButton?.RegisterCallback<ClickEvent>(evt => OnCredits());
        creditsCloseButton?.RegisterCallback<ClickEvent>(evt => OnCloseCredits());
        creditsTabGeneral?.RegisterCallback<ClickEvent>(evt => SwitchCreditsTab(0));
        creditsTabAircraft?.RegisterCallback<ClickEvent>(evt => SwitchCreditsTab(1));
        creditsTabAudio?.RegisterCallback<ClickEvent>(evt => SwitchCreditsTab(2));
        quitButton?.RegisterCallback<ClickEvent>(evt => OnQuitGame());

        githubButton?.RegisterCallback<ClickEvent>(evt => OnOpenGitHub());
        youtubeButton?.RegisterCallback<ClickEvent>(evt => OnOpenYouTube());

        badgesCloseButton?.RegisterCallback<ClickEvent>(evt => CloseBadgesPopup());
        badgesViewAllButton?.RegisterCallback<ClickEvent>(evt => OnViewAllBadges());

        startButton?.RegisterCallback<NavigationSubmitEvent>(evt => OnStartGame());
        profileButton?.RegisterCallback<NavigationSubmitEvent>(evt => OnProfile());
        settingsButton?.RegisterCallback<NavigationSubmitEvent>(evt => OnSettings());
        howToPlayButton?.RegisterCallback<NavigationSubmitEvent>(evt => OnHowToPlay());
        creditsButton?.RegisterCallback<NavigationSubmitEvent>(evt => OnCredits());
        quitButton?.RegisterCallback<NavigationSubmitEvent>(evt => OnQuitGame());
    }

    public static bool ShouldStartImmediately = false;

    private void Start()
    {
        EnsureReferences();

        if (ShouldStartImmediately)
        {
            ShouldStartImmediately = false;
            startWithMenuOpen = false;
            skipHangarSelection = true;
            OnStartGame();
            return;
        }

        if (startWithMenuOpen)
        {
            if (StartupLogoSequenceUIToolkit.IsPlaying)
            {
                StartCoroutine(WaitForStartupLogo());
            }
            else
            {
                ShowMainMenu();
            }
        }
        else
        {
            OnStartGame();
        }
    }

    private System.Collections.IEnumerator WaitForStartupLogo()
    {
        while (StartupLogoSequenceUIToolkit.IsPlaying)
        {
            yield return null;
        }

        yield return null;

        ShowMainMenu();
    }

    private void Update()
    {
        if (!gameStarted && IsStartKeyPressedThisFrame())
        {
            OnStartGame();
        }
    }

    private bool IsStartKeyPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return false;
        return keyboard.enterKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Return);
#else
        return false;
#endif
    }

    public void OnStartGame()
    {
        var activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.Equals(activeSceneName, gameplaySceneName, System.StringComparison.Ordinal))
        {
            if (!skipHangarSelection && hangarController != null)
            {
                menuShown = false;
                SetMenuVisible(false);
                hangarController.Show();
                Debug.Log("[MainMenu] Redirecting to hangar for aircraft selection");
                return;
            }

            skipHangarSelection = false;

            Time.timeScale = 1f;
            menuShown = false;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetSessionStats();
            }

            bool shouldShowStory = StoryIntroController.ShouldShowStory();
            Debug.Log(
                $"[MainMenu] Story check: shouldShow={shouldShowStory}, controllerAssigned={storyIntroController != null}"
            );

            if (shouldShowStory && storyIntroController != null)
            {
                SetMenuVisible(false);
                storyIntroController.Show();
                Debug.Log("Showing new player story intro");
                return;
            }
            else if (shouldShowStory && storyIntroController == null)
            {
                Debug.LogWarning(
                    "[MainMenu] Story should show but StoryIntroController is not assigned in Inspector!"
                );
            }

            if (
                !string.IsNullOrWhiteSpace(loadingSceneName)
                && Application.CanStreamedLevelBeLoaded(loadingSceneName)
            )
            {
                LoadingSceneState.TargetSceneName = gameplaySceneName;
                ShouldStartImmediately = true;
                SceneManager.LoadScene(loadingSceneName);
                return;
            }

            ShouldStartImmediately = true;
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        gameStarted = true;
        menuShown = false;

        SetMenuVisible(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        Time.timeScale = 1f;

        Debug.Log("Game started!");
    }

    public void StartGameFromHangar()
    {
        skipHangarSelection = true;
        OnStartGame();
    }

    public void OnSettings()
    {
        if (settingsController != null)
        {
            menuShown = false;
            SetMenuVisible(false);
            settingsController.OpenSettings();
        }
    }

    public void OnHowToPlay()
    {
        if (howToPlayController != null)
        {
            menuShown = false;
            SetMenuVisible(false);
            howToPlayController.Show();
        }
        else
        {
            Debug.LogWarning("HowToPlayController not assigned");
        }
    }

    public void OnProfile()
    {
        if (playerStatsController != null)
        {
            menuShown = false;
            SetMenuVisible(false);
            playerStatsController.Show();
        }
        else
        {
            Debug.LogWarning("PlayerStatsController not assigned");
        }
    }

    public void Show()
    {
        ShowMainMenu();
    }

    public void ResetMenuState()
    {
        menuShown = false;
        gameStarted = false;
        Debug.Log("[MainMenu] Menu state reset");
    }

    public void OnCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.AddToClassList("visible");
            SwitchCreditsTab(0);
            Debug.Log("Showing credits");
        }
    }

    public void OnCloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.RemoveFromClassList("visible");
        }
    }

    private void SwitchCreditsTab(int tabIndex)
    {
        creditsTabGeneral?.RemoveFromClassList("credits-tab-active");
        creditsTabAircraft?.RemoveFromClassList("credits-tab-active");
        creditsTabAudio?.RemoveFromClassList("credits-tab-active");

        creditsPageGeneral?.RemoveFromClassList("credits-page-visible");
        creditsPageAircraft?.RemoveFromClassList("credits-page-visible");
        creditsPageAudio?.RemoveFromClassList("credits-page-visible");

        switch (tabIndex)
        {
            case 0:
                creditsTabGeneral?.AddToClassList("credits-tab-active");
                creditsPageGeneral?.AddToClassList("credits-page-visible");
                break;
            case 1:
                creditsTabAircraft?.AddToClassList("credits-tab-active");
                creditsPageAircraft?.AddToClassList("credits-page-visible");
                break;
            case 2:
                creditsTabAudio?.AddToClassList("credits-tab-active");
                creditsPageAudio?.AddToClassList("credits-page-visible");
                break;
        }
    }

    public void OnQuitGame()
    {
        Debug.Log("Exiting game");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ShowMainMenu()
    {
        if (menuShown && !gameStarted)
        {
            return;
        }

        ShouldStartImmediately = false;
        gameStarted = false;
        menuShown = true;

        if (root == null && uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
        }

        SetMenuVisible(true);

        StartFlyingJetsAnimation();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        if (gameUI != null)
            gameUI.SetActive(false);

        Time.timeScale = 0f;

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        startButton?.Focus();

        CheckAndShowNewBadges();

        Debug.Log("Main menu shown");
    }

    private void CheckAndShowNewBadges()
    {
        BadgeTracker.CheckForNewBadges("MainMenuController.ShowMainMenu");

        if (BadgeTracker.HasPendingBadges())
        {
            ShowNewBadgesPopup();
        }
    }

    private void ShowNewBadgesPopup()
    {
        var newBadges = BadgeTracker.GetPendingNewBadges();
        if (newBadges.Count == 0)
            return;

        if (newBadgesContainer != null)
        {
            newBadgesContainer.Clear();

            foreach (var badge in newBadges)
            {
                var badgeItem = new VisualElement();
                badgeItem.name = badge.id;
                badgeItem.AddToClassList("new-badge-item");

                var badgeImage = new VisualElement();
                badgeImage.AddToClassList("new-badge-image");
                badgeImage.AddToClassList("badge-image-placeholder");
                badgeItem.Add(badgeImage);

                var badgeName = new Label(badge.name);
                badgeName.AddToClassList("new-badge-name");
                badgeItem.Add(badgeName);

                var badgeDesc = new Label(badge.description);
                badgeDesc.AddToClassList("new-badge-desc");
                badgeItem.Add(badgeDesc);

                newBadgesContainer.Add(badgeItem);
            }
        }

        if (newBadgesSubtitle != null)
        {
            string badgeWord = newBadges.Count == 1 ? "badge" : "badges";
            newBadgesSubtitle.text = $"You earned {newBadges.Count} new {badgeWord}!";
        }

        if (newBadgesPopup != null)
        {
            newBadgesPopup.AddToClassList("visible");
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuOpen();
        }

        Debug.Log($"[MainMenu] Showing {newBadges.Count} new badges popup");
    }

    private void CloseBadgesPopup()
    {
        if (newBadgesPopup != null)
        {
            newBadgesPopup.RemoveFromClassList("visible");
        }

        BadgeTracker.ClearPendingBadges();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuClose();
        }
    }

    private void OnViewAllBadges()
    {
        CloseBadgesPopup();

        if (playerStatsController != null)
        {
            menuShown = false;
            SetMenuVisible(false);
            playerStatsController.Show();
            playerStatsController.SwitchMainSection(1);
        }
    }

    private void StartFlyingJetsAnimation()
    {
        if (jetsAnimationStarted)
            return;
        jetsAnimationStarted = true;

        StartCoroutine(FlyingJetsAnimationLoop());
    }

    private System.Collections.IEnumerator FlyingJetsAnimationLoop()
    {
        float[] ltrDelays = { 0f, 2f, 5f, 8f, 11f };
        float[] rtlDelays = { 1f, 4f, 6f, 10f, 13f };

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            StartCoroutine(AnimateSingleJet(flyingJetsLTR[index], true, ltrDelays[index]));
            StartCoroutine(AnimateSingleJet(flyingJetsRTL[index], false, rtlDelays[index]));
        }

        yield break;
    }

    private System.Collections.IEnumerator AnimateSingleJet(
        VisualElement jet,
        bool leftToRight,
        float initialDelay
    )
    {
        if (jet == null)
            yield break;

        float waited = 0f;
        while (waited < initialDelay)
        {
            waited += Time.unscaledDeltaTime;
            yield return null;
        }

        StartCoroutine(FlashJetOpacity(jet));

        while (true)
        {
            float directionRoll = Random.value;
            string flightClass;
            float rotationAngle;

            if (directionRoll < 0.2f)
            {
                flightClass = "jet-ltr";
                rotationAngle = 90f;
            }
            else if (directionRoll < 0.4f)
            {
                flightClass = "jet-rtl";
                rotationAngle = -90f;
            }
            else if (directionRoll < 0.55f)
            {
                flightClass = "jet-diagonal-1";
                rotationAngle = 135f;
            }
            else if (directionRoll < 0.7f)
            {
                flightClass = "jet-diagonal-2";
                rotationAngle = 45f;
            }
            else if (directionRoll < 0.85f)
            {
                flightClass = "jet-diagonal-3";
                rotationAngle = -135f;
            }
            else
            {
                flightClass = "jet-diagonal-4";
                rotationAngle = -45f;
            }

            jet.style.rotate = new Rotate(rotationAngle);

            if (jetTextures != null && jetTextures.Length > 0)
            {
                int textureIndex = Random.Range(0, jetTextures.Length);
                if (jetTextures[textureIndex] != null)
                {
                    jet.style.backgroundImage = new StyleBackground(jetTextures[textureIndex]);
                }
            }

            if (flightClass.Contains("diagonal-1"))
            {
                jet.style.left = -100;
                jet.style.top = Length.Percent(Random.Range(-10f, 10f));
            }
            else if (flightClass.Contains("diagonal-2"))
            {
                jet.style.left = -100;
                jet.style.top = Length.Percent(Random.Range(80f, 100f));
            }
            else if (flightClass.Contains("diagonal-3"))
            {
                jet.style.left = StyleKeyword.Auto;
                jet.style.right = -100;
                jet.style.top = Length.Percent(Random.Range(-10f, 10f));
            }
            else if (flightClass.Contains("diagonal-4"))
            {
                jet.style.left = StyleKeyword.Auto;
                jet.style.right = -100;
                jet.style.top = Length.Percent(Random.Range(80f, 100f));
            }
            else if (flightClass == "jet-ltr")
            {
                jet.style.left = -100;
                jet.style.right = StyleKeyword.Auto;
                jet.style.top = Length.Percent(Random.Range(5f, 85f));
            }
            else
            {
                jet.style.left = StyleKeyword.Auto;
                jet.style.right = -100;
                jet.style.top = Length.Percent(Random.Range(5f, 85f));
            }

            jet.AddToClassList(flightClass);
            jet.AddToClassList("flying");

            float duration = Random.Range(12f, 22f);
            waited = 0f;
            while (waited < duration)
            {
                if (jet.userData is string status && status == "exploded")
                {
                    yield return new WaitForSeconds(1.5f);
                    break;
                }

                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            jet.RemoveFromClassList("flying");
            jet.RemoveFromClassList(flightClass);

            jet.userData = null;
            jet.style.translate = StyleKeyword.Null;
            jet.style.opacity = StyleKeyword.Null;

            float pauseDuration = Random.Range(3f, 8f);
            waited = 0f;
            while (waited < pauseDuration)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }

    private void OnJetClicked(VisualElement jet)
    {
        Debug.Log(
            $"[MainMenu] Jet clicked! userData={jet.userData}, hasFlying={jet.ClassListContains("flying")}"
        );

        if (jet.userData is string status && status == "exploded")
        {
            Debug.Log("[MainMenu] Jet already exploded, ignoring click");
            return;
        }

        jet.userData = "exploded";

        jet.style.translate = jet.resolvedStyle.translate;
        jet.RemoveFromClassList("flying");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosion();
        }

        if (explosionTexture != null)
        {
            jet.style.backgroundImage = new StyleBackground(explosionTexture);
            jet.style.rotate = new Rotate(0f);
            jet.style.opacity = 1f;
            jet.style.width = 150;
            jet.style.height = 150;
            Debug.Log("[MainMenu] Jet exploded!");
        }
        else
        {
            Debug.LogWarning("[MainMenu] Explosion texture is null!");
        }

        StartCoroutine(ExplodeSequence(jet));
    }

    private System.Collections.IEnumerator ExplodeSequence(VisualElement jet)
    {
        yield return new WaitForSecondsRealtime(1.0f);

        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            jet.style.opacity = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        jet.style.opacity = 0f;
    }

    private System.Collections.IEnumerator FlashJetOpacity(VisualElement jet)
    {
        if (jet == null)
            yield break;

        float minOpacity = 0.5f;
        float maxOpacity = 1.0f;
        float flashSpeed = Random.Range(1.5f, 3.5f);

        while (true)
        {
            if (jet.userData is string status && status == "exploded")
            {
                yield break;
            }

            float elapsed = 0f;
            float duration = flashSpeed;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float opacity = Mathf.Lerp(minOpacity, maxOpacity, Mathf.PingPong(t * 2, 1));
                jet.style.opacity = opacity;
                yield return null;
            }
        }
    }

    public void HideMainMenu()
    {
        SetMenuVisible(false);
        Debug.Log("Main menu hidden");
    }

    public void OnBackToMenu()
    {
        SetMenuVisible(true);
        startButton?.Focus();
    }

    public bool IsGameStarted() => gameStarted;

    private void SetMenuVisible(bool visible)
    {
        if (root != null)
        {
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;

            if (visible)
            {
                root.style.position = Position.Absolute;
                root.style.top = 0;
                root.style.left = 0;
                root.style.right = 0;
                root.style.bottom = 0;
                root.style.width = Length.Percent(100);
                root.style.height = Length.Percent(100);
                root.style.opacity = 1f;
                root.BringToFront();
            }
        }
    }

    private void EnsureReferences()
    {
        if (settingsController == null)
            settingsController = FindFirstObjectByType<SettingsController>();
        if (hangarController == null)
            hangarController = FindFirstObjectByType<HangarController>();
        if (playerStatsController == null)
            playerStatsController = FindFirstObjectByType<PlayerStatsController>();
    }

    private void OnOpenGitHub()
    {
        Application.OpenURL(GITHUB_URL);
        Debug.Log($"Opening GitHub: {GITHUB_URL}");
    }

    private void OnOpenYouTube()
    {
        Application.OpenURL(YOUTUBE_URL);
        Debug.Log($"Opening YouTube: {YOUTUBE_URL}");
    }

    private void RegisterButtonSounds(Button button)
    {
        if (button == null)
            return;

        button.RegisterCallback<MouseEnterEvent>(evt =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayButtonHover();
            }
        });

        button.RegisterCallback<ClickEvent>(
            evt =>
            {
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayButtonClick();
                }
            },
            TrickleDown.TrickleDown
        );
    }
}
