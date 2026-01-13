using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MissionResultController : MonoBehaviour
{
    [Header("UI Documents")]
    [SerializeField]
    private UIDocument gameOverDocument;

    [SerializeField]
    private UIDocument victoryDocument;

    [Header("References")]
    [SerializeField]
    private MainMenuController mainMenuController;

    public static bool HasPendingResult { get; private set; }
    public static bool WasVictory { get; private set; }
    public static MissionResultData ResultData { get; private set; }

    private VisualElement gameOverRoot;
    private VisualElement goCommanderPortrait;
    private Label goCommanderDialogue;
    private Label deathReason;
    private Label deathLocation;
    private Label goRankGrade;
    private Label goStars;
    private Label goKillsValue;
    private Label goTimeValue;
    private Label goAccuracyValue;
    private Button retryButton;
    private Button goMenuButton;

    private VisualElement victoryRoot;
    private VisualElement vCommanderPortrait;
    private Label vCommanderDialogue;
    private Label missionName;
    private Label rankGrade;
    private Label stars;
    private Label vKillsValue;
    private Label vTimeValue;
    private Label vAccuracyValue;
    private Label completionTime;
    private Label xpReward;
    private Button continueButton;
    private Button vMenuButton;

    private enum CommanderMood
    {
        Angry,
        Disappointed,
        Neutral,
        Happy,
        Proud,
    }

    private readonly string[] angryDialogues =
    {
        "Your negativity has wounded me deeply... I gave you everything, and this is how you repay me?",
        "Do you have any idea how much your failure HURTS me personally? I believed in you...",
        "This is what happens when you let doubt into your heart. YOUR doubt killed those people.",
    };

    private readonly string[] disappointedDialogues =
    {
        "I'm not angry... just disappointed. Your negative energy attracted this outcome.",
        "Perhaps if you had more faith in yourself—in ME—this wouldn't have happened.",
        "I forgive you, of course. But know that your failure has caused me great pain.",
    };

    private readonly string[] neutralDialogues =
    {
        "An acceptable outcome. Though I sense you could have done better with more positive thinking.",
        "The mission is complete. I choose to focus on the positives, despite your... limitations.",
        "You survived. Let's be grateful for small blessings, shall we?",
    };

    private readonly string[] happyDialogues =
    {
        "See what happens when you embrace the light? I knew you had it in you, my dear pilot.",
        "Your positive energy has manifested this victory! Aren't you glad you trusted me?",
        "Wonderful! This proves that love and faith triumph over all negativity!",
    };

    private readonly string[] proudDialogues =
    {
        "GLORIOUS! You have finally become the instrument of light I always knew you could be!",
        "Perfect! Your complete surrender to positive energy has brought us this miracle!",
        "This is what happens when you stop questioning and simply BELIEVE! I'm so proud of you!",
    };

    [System.Serializable]
    public class MissionResultData
    {
        public bool victory;
        public string missionName;
        public string deathReason;
        public string deathLocation;
        public int kills;
        public float accuracy;
        public float gameTime;
        public int xpEarned;
        public int missilesFired;
        public int missilesHit;
        public int bulletsFired;
        public int bulletsHit;
    }

    public static void SetMissionResult(
        bool victory,
        int kills,
        float accuracy,
        float gameTime,
        string deathReason = null,
        string deathLocation = null,
        string missionName = null,
        int xpEarned = 0,
        int missilesFired = 0,
        int missilesHit = 0,
        int bulletsFired = 0,
        int bulletsHit = 0
    )
    {
        HasPendingResult = true;
        WasVictory = victory;
        ResultData = new MissionResultData
        {
            victory = victory,
            missionName = missionName ?? "Hong Kong Defense",
            deathReason = deathReason ?? "Killed in action",
            deathLocation = deathLocation ?? "Over the battlefield",
            kills = kills,
            accuracy = accuracy,
            gameTime = gameTime,
            xpEarned = xpEarned,
            missilesFired = missilesFired,
            missilesHit = missilesHit,
            bulletsFired = bulletsFired,
            bulletsHit = bulletsHit,
        };

        Debug.Log(
            $"[MissionResult] Set pending result: victory={victory}, kills={kills}, missiles={missilesHit}/{missilesFired}, bullets={bulletsHit}/{bulletsFired}"
        );
    }

    public static void ClearResult()
    {
        HasPendingResult = false;
        WasVictory = false;
        ResultData = null;
    }

    void Start()
    {
        if (mainMenuController == null)
        {
            mainMenuController = FindFirstObjectByType<MainMenuController>();
        }

        Debug.Log(
            $"[MissionResult] Start: MainMenuController={mainMenuController != null}, gameOverDocument={gameOverDocument != null}, victoryDocument={victoryDocument != null}"
        );

        if (gameOverDocument != null)
        {
            var goDocRoot = gameOverDocument.rootVisualElement;
            if (goDocRoot != null)
            {
                gameOverRoot = goDocRoot.Q<VisualElement>("root") ?? goDocRoot;

                goCommanderPortrait = goDocRoot.Q<VisualElement>("commander-portrait");
                goCommanderDialogue = goDocRoot.Q<Label>("commander-dialogue");

                deathReason = goDocRoot.Q<Label>("death-reason");
                deathLocation = goDocRoot.Q<Label>("death-location");
                goRankGrade = goDocRoot.Q<Label>("rank-grade");
                goStars = goDocRoot.Q<Label>("stars");
                goKillsValue = goDocRoot.Q<Label>("kills-value");
                goTimeValue = goDocRoot.Q<Label>("time-value");
                goAccuracyValue = goDocRoot.Q<Label>("accuracy-value");
                retryButton = goDocRoot.Q<Button>("retry-button");
                goMenuButton = goDocRoot.Q<Button>("menu-button");

                if (retryButton != null)
                    retryButton.clicked += OnRetry;
                if (goMenuButton != null)
                    goMenuButton.clicked += OnBackToMenu;

                if (gameOverRoot != null)
                    gameOverRoot.style.display = DisplayStyle.None;

                Debug.Log(
                    $"[MissionResult] Game Over UI initialized: gameOverRoot={gameOverRoot != null}, commander={goCommanderPortrait != null}"
                );
            }
        }
        else
        {
            Debug.LogWarning("[MissionResult] gameOverDocument is not assigned!");
        }

        if (victoryDocument != null)
        {
            var vDocRoot = victoryDocument.rootVisualElement;
            if (vDocRoot != null)
            {
                victoryRoot = vDocRoot.Q<VisualElement>("root") ?? vDocRoot;

                vCommanderPortrait = vDocRoot.Q<VisualElement>("commander-portrait");
                vCommanderDialogue = vDocRoot.Q<Label>("commander-dialogue");

                missionName = vDocRoot.Q<Label>("mission-name");
                rankGrade = vDocRoot.Q<Label>("rank-grade");
                stars = vDocRoot.Q<Label>("stars");
                vKillsValue = vDocRoot.Q<Label>("kills-value");
                vTimeValue = vDocRoot.Q<Label>("time-value");
                vAccuracyValue = vDocRoot.Q<Label>("accuracy-value");
                completionTime = vDocRoot.Q<Label>("completion-time");
                xpReward = vDocRoot.Q<Label>("xp-reward");
                continueButton = vDocRoot.Q<Button>("continue-button");
                vMenuButton = vDocRoot.Q<Button>("menu-button");

                if (continueButton != null)
                    continueButton.clicked += OnBackToMenu;
                if (vMenuButton != null)
                    vMenuButton.clicked += OnBackToMenu;

                if (victoryRoot != null)
                    victoryRoot.style.display = DisplayStyle.None;

                Debug.Log(
                    $"[MissionResult] Victory UI initialized: victoryRoot={victoryRoot != null}, commander={vCommanderPortrait != null}"
                );
            }
        }
        else
        {
            Debug.LogWarning("[MissionResult] victoryDocument is not assigned!");
        }

        Debug.Log("[MissionResult] UI initialized and hidden");

        if (HasPendingResult)
        {
            Debug.Log($"[MissionResult] Showing pending result: victory={WasVictory}");
            ShowResult();
        }
    }

    void ShowResult()
    {
        if (ResultData == null)
        {
            Debug.LogError("[MissionResult] ShowResult called but ResultData is null!");
            return;
        }

        Debug.Log(
            $"[MissionResult] ShowResult: victory={ResultData.victory}, kills={ResultData.kills}"
        );

        Debug.Log("[MissionResult] ====== CHECKING BADGES ON RESULT SCREEN ======");
        BadgeTracker.CheckForNewBadges(
            $"MissionResultController.ShowResult(victory={ResultData.victory})"
        );

        if (mainMenuController != null)
        {
            mainMenuController.HideMainMenu();
        }

        if (ResultData.victory)
        {
            ShowVictory();
        }
        else
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        if (gameOverRoot == null)
        {
            Debug.LogError(
                "[MissionResult] ShowGameOver: gameOverRoot is NULL! Make sure GameOver UIDocument is assigned in Inspector."
            );
            return;
        }

        Debug.Log("[MissionResult] Showing Game Over screen...");
        gameOverRoot.style.display = DisplayStyle.Flex;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDefeatMusic();
        }

        if (deathReason != null)
            deathReason.text = ResultData.deathReason;

        if (deathLocation != null)
            deathLocation.text = ResultData.deathLocation;

        int score = CalculateScore(ResultData.kills, ResultData.accuracy, ResultData.gameTime);
        CalculateRank(ResultData.kills, ResultData.accuracy, ResultData.gameTime, isVictory: false);

        CommanderMood mood = score >= 40 ? CommanderMood.Disappointed : CommanderMood.Angry;
        SetCommanderMood(goCommanderPortrait, goCommanderDialogue, mood, isVictory: false);

        if (goKillsValue != null)
            goKillsValue.text = ResultData.kills.ToString();

        if (goTimeValue != null)
        {
            int minutes = Mathf.FloorToInt(ResultData.gameTime / 60f);
            int seconds = Mathf.FloorToInt(ResultData.gameTime % 60f);
            goTimeValue.text = $"{minutes}:{seconds:D2}";
        }

        if (goAccuracyValue != null)
            goAccuracyValue.text = $"{ResultData.accuracy:F1}%";

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Time.timeScale = 1f;

        Debug.Log("[MissionResult] Showing Game Over screen");
    }

    void ShowVictory()
    {
        if (victoryRoot == null)
        {
            Debug.LogError(
                "[MissionResult] ShowVictory: victoryRoot is NULL! Make sure Victory UIDocument is assigned in Inspector."
            );

            if (victoryDocument != null)
            {
                var vDocRoot = victoryDocument.rootVisualElement;
                if (vDocRoot != null)
                {
                    victoryRoot = vDocRoot.Q<VisualElement>("root") ?? vDocRoot;
                    vCommanderPortrait = vDocRoot.Q<VisualElement>("commander-portrait");
                    vCommanderDialogue = vDocRoot.Q<Label>("commander-dialogue");
                    Debug.Log($"[MissionResult] Re-acquired victoryRoot: {victoryRoot != null}");
                }
            }

            if (victoryRoot == null)
            {
                Debug.LogError(
                    "[MissionResult] Could not acquire victoryRoot! Victory screen will not show."
                );
                return;
            }
        }

        Debug.Log("[MissionResult] Showing Victory screen...");
        victoryRoot.style.display = DisplayStyle.Flex;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayVictoryMusic();
            AudioManager.Instance.PlayRTB();
        }

        if (missionName != null)
            missionName.text = ResultData.missionName;

        if (vKillsValue != null)
            vKillsValue.text = ResultData.kills.ToString();

        if (vTimeValue != null)
        {
            int minutes = Mathf.FloorToInt(ResultData.gameTime / 60f);
            int seconds = Mathf.FloorToInt(ResultData.gameTime % 60f);
            vTimeValue.text = $"{minutes}:{seconds:D2}";
        }

        if (completionTime != null)
        {
            int minutes = Mathf.FloorToInt(ResultData.gameTime / 60f);
            int seconds = Mathf.FloorToInt(ResultData.gameTime % 60f);
            completionTime.text = $"{minutes}:{seconds:D2}";
        }

        if (vAccuracyValue != null)
            vAccuracyValue.text = $"{ResultData.accuracy:F1}%";

        int score = CalculateScore(ResultData.kills, ResultData.accuracy, ResultData.gameTime);
        CalculateRank(ResultData.kills, ResultData.accuracy, ResultData.gameTime, isVictory: true);

        CommanderMood mood;
        if (score >= 80)
            mood = CommanderMood.Proud;
        else if (score >= 60)
            mood = CommanderMood.Happy;
        else
            mood = CommanderMood.Neutral;

        SetCommanderMood(vCommanderPortrait, vCommanderDialogue, mood, isVictory: true);

        if (xpReward != null)
            xpReward.text = $"+{ResultData.xpEarned} XP";

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Time.timeScale = 1f;

        Debug.Log("[MissionResult] Showing Victory screen");
    }

    int CalculateScore(int kills, float accuracy, float time)
    {
        int score = 0;
        score += kills * 10;
        score += Mathf.FloorToInt(accuracy * 0.3f);
        if (time < 300)
            score += 30;
        else if (time < 600)
            score += 15;
        return score;
    }

    void SetCommanderMood(
        VisualElement portrait,
        Label dialogue,
        CommanderMood mood,
        bool isVictory
    )
    {
        if (portrait == null)
            return;

        portrait.RemoveFromClassList("happy");
        portrait.RemoveFromClassList("proud");
        portrait.RemoveFromClassList("neutral");
        portrait.RemoveFromClassList("disappointed");
        portrait.RemoveFromClassList("angry");

        string moodClass = mood.ToString().ToLower();
        portrait.AddToClassList(moodClass);

        if (dialogue != null)
        {
            string[] dialogues = mood switch
            {
                CommanderMood.Proud => proudDialogues,
                CommanderMood.Happy => happyDialogues,
                CommanderMood.Neutral => neutralDialogues,
                CommanderMood.Disappointed => disappointedDialogues,
                CommanderMood.Angry => angryDialogues,
                _ => neutralDialogues,
            };

            dialogue.text = dialogues[UnityEngine.Random.Range(0, dialogues.Length)];
        }

        Debug.Log($"[MissionResult] Commander mood set to: {mood}");
    }

    void CalculateRank(int kills, float accuracy, float time, bool isVictory = true)
    {
        int score = 0;
        score += kills * 10;
        score += Mathf.FloorToInt(accuracy * 0.3f);
        if (time < 300)
            score += 30;
        else if (time < 600)
            score += 15;

        string grade;
        string starDisplay;

        if (score >= 80)
        {
            grade = "S";
            starDisplay = "";
        }
        else if (score >= 60)
        {
            grade = "A";
            starDisplay = "";
        }
        else if (score >= 40)
        {
            grade = "B";
            starDisplay = "";
        }
        else
        {
            grade = "C";
            starDisplay = "";
        }

        if (isVictory)
        {
            if (rankGrade != null)
                rankGrade.text = grade;

            if (stars != null)
                stars.text = starDisplay;
        }
        else
        {
            if (goRankGrade != null)
                goRankGrade.text = grade;

            if (goStars != null)
                goStars.text = starDisplay;
        }
    }

    void OnRetry()
    {
        Debug.Log("[MissionResult] Retry button clicked - restarting mission");
        ClearResult();

        if (gameOverRoot != null)
            gameOverRoot.style.display = DisplayStyle.None;
        if (victoryRoot != null)
            victoryRoot.style.display = DisplayStyle.None;

        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartMission();
        }
        else
        {
            Debug.Log("[MissionResult] GameManager not found, loading Main scene directly");
            SceneManager.LoadScene("Main");
        }
    }

    void OnBackToMenu()
    {
        ClearResult();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSounds();
        }

        if (gameOverRoot != null)
            gameOverRoot.style.display = DisplayStyle.None;
        if (victoryRoot != null)
            victoryRoot.style.display = DisplayStyle.None;

        if (mainMenuController == null)
        {
            mainMenuController = FindFirstObjectByType<MainMenuController>();
            Debug.Log(
                $"[MissionResult] OnBackToMenu: Found MainMenuController = {mainMenuController != null}"
            );
        }

        if (mainMenuController != null)
        {
            mainMenuController.ResetMenuState();
            mainMenuController.ShowMainMenu();
            Debug.Log("[MissionResult] Showing main menu via controller");
        }
        else
        {
            Debug.LogWarning("[MissionResult] MainMenuController not found! Loading menu scene...");
            MainMenuController.ShouldStartImmediately = false;
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu and story");
        }
    }

    void OnDestroy()
    {
        if (retryButton != null)
            retryButton.clicked -= OnRetry;
        if (goMenuButton != null)
            goMenuButton.clicked -= OnBackToMenu;
        if (continueButton != null)
            continueButton.clicked -= OnBackToMenu;
        if (vMenuButton != null)
            vMenuButton.clicked -= OnBackToMenu;
    }
}
