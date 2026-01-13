using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatsController : MonoBehaviour
{
    [SerializeField]
    private UIDocument uiDocument;

    private VisualElement root;
    private Button backButton;
    private Button editCallsignButton;

    private Label callsignLabel;
    private Label rankLabel;
    private Label xpText;
    private VisualElement xpBar;
    private Label totalKillsLabel;
    private Label totalWinsLabel;
    private Label flightTimeLabel;
    private Label missilesFiredLabel;
    private Label accuracyLabel;
    private Label kdRatioLabel;
    private Label achievementCountLabel;

    private Label f15KillsLabel;
    private Label f15TimeLabel;
    private Label f15SortiesLabel;
    private Label su27KillsLabel;
    private Label su27TimeLabel;
    private Label su27SortiesLabel;
    private Label mig29KillsLabel;
    private Label mig29TimeLabel;
    private Label mig29SortiesLabel;
    private Label fa18eKillsLabel;
    private Label fa18eTimeLabel;
    private Label fa18eSortiesLabel;
    private Label hawk200KillsLabel;
    private Label hawk200TimeLabel;
    private Label hawk200SortiesLabel;
    private Label mig21KillsLabel;
    private Label mig21TimeLabel;
    private Label mig21SortiesLabel;
    private Label tornadoKillsLabel;
    private Label tornadoTimeLabel;
    private Label tornadoSortiesLabel;
    private Label rafaleKillsLabel;
    private Label rafaleTimeLabel;
    private Label rafaleSortiesLabel;
    private Label typhoonKillsLabel;
    private Label typhoonTimeLabel;
    private Label typhoonSortiesLabel;

    private Button tabF15;
    private Button tabSU27;
    private Button tabMIG29;
    private Button tabFA18E;
    private Button tabHawk200;
    private Button tabMIG21;
    private Button tabTornado;
    private Button tabRafale;
    private Button tabTyphoon;
    private VisualElement panelF15;
    private VisualElement panelSU27;
    private VisualElement panelMIG29;
    private VisualElement panelFA18E;
    private VisualElement panelHawk200;
    private VisualElement panelMIG21;
    private VisualElement panelTornado;
    private VisualElement panelRafale;
    private VisualElement panelTyphoon;

    private VisualElement achFirstBlood;
    private VisualElement achAce;
    private VisualElement achDoubleAce;
    private VisualElement achVeteran;
    private VisualElement achVictor;
    private VisualElement achTopgun;
    private VisualElement achCenturion;

    private Button mainTabStats;
    private Button mainTabBadges;
    private VisualElement sectionStats;
    private VisualElement sectionBadges;
    private Label badgeCountLabel;

    void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            root = uiDocument.rootVisualElement.Q<VisualElement>("root");
            if (root != null)
            {
                root.style.display = DisplayStyle.None;
            }
        }
    }

    void Start()
    {
        SetupUI();
    }

    void SetupUI()
    {
        if (uiDocument == null)
            return;

        root = uiDocument.rootVisualElement.Q<VisualElement>("root");
        if (root == null)
            return;

        backButton = root.Q<Button>("back-button");
        editCallsignButton = root.Q<Button>("edit-callsign-button");

        callsignLabel = root.Q<Label>("callsign");
        rankLabel = root.Q<Label>("rank");
        xpText = root.Q<Label>("xp-text");
        xpBar = root.Q<VisualElement>("xp-bar");
        totalKillsLabel = root.Q<Label>("total-kills");
        totalWinsLabel = root.Q<Label>("total-wins");
        flightTimeLabel = root.Q<Label>("flight-time");
        missilesFiredLabel = root.Q<Label>("missiles-fired");
        accuracyLabel = root.Q<Label>("accuracy");
        kdRatioLabel = root.Q<Label>("kd-ratio");
        achievementCountLabel = root.Q<Label>("achievement-count");

        f15KillsLabel = root.Q<Label>("f15-kills");
        f15TimeLabel = root.Q<Label>("f15-time");
        f15SortiesLabel = root.Q<Label>("f15-sorties");

        su27KillsLabel = root.Q<Label>("su27-kills");
        su27TimeLabel = root.Q<Label>("su27-time");
        su27SortiesLabel = root.Q<Label>("su27-sorties");

        mig29KillsLabel = root.Q<Label>("mig29-kills");
        mig29TimeLabel = root.Q<Label>("mig29-time");
        mig29SortiesLabel = root.Q<Label>("mig29-sorties");

        fa18eKillsLabel = root.Q<Label>("fa18e-kills");
        fa18eTimeLabel = root.Q<Label>("fa18e-time");
        fa18eSortiesLabel = root.Q<Label>("fa18e-sorties");

        hawk200KillsLabel = root.Q<Label>("hawk200-kills");
        hawk200TimeLabel = root.Q<Label>("hawk200-time");
        hawk200SortiesLabel = root.Q<Label>("hawk200-sorties");

        mig21KillsLabel = root.Q<Label>("mig21-kills");
        mig21TimeLabel = root.Q<Label>("mig21-time");
        mig21SortiesLabel = root.Q<Label>("mig21-sorties");

        tornadoKillsLabel = root.Q<Label>("tornado-kills");
        tornadoTimeLabel = root.Q<Label>("tornado-time");
        tornadoSortiesLabel = root.Q<Label>("tornado-sorties");

        rafaleKillsLabel = root.Q<Label>("rafale-kills");
        rafaleTimeLabel = root.Q<Label>("rafale-time");
        rafaleSortiesLabel = root.Q<Label>("rafale-sorties");

        typhoonKillsLabel = root.Q<Label>("typhoon-kills");
        typhoonTimeLabel = root.Q<Label>("typhoon-time");
        typhoonSortiesLabel = root.Q<Label>("typhoon-sorties");

        tabF15 = root.Q<Button>("tab-f15");
        tabSU27 = root.Q<Button>("tab-su27");
        tabMIG29 = root.Q<Button>("tab-mig29");
        tabFA18E = root.Q<Button>("tab-fa18e");
        tabHawk200 = root.Q<Button>("tab-hawk200");
        tabMIG21 = root.Q<Button>("tab-mig21");
        tabTornado = root.Q<Button>("tab-tornado");
        tabRafale = root.Q<Button>("tab-rafale");
        tabTyphoon = root.Q<Button>("tab-typhoon");

        panelF15 = root.Q<VisualElement>("panel-f15");
        panelSU27 = root.Q<VisualElement>("panel-su27");
        panelMIG29 = root.Q<VisualElement>("panel-mig29");
        panelFA18E = root.Q<VisualElement>("panel-fa18e");
        panelHawk200 = root.Q<VisualElement>("panel-hawk200");
        panelMIG21 = root.Q<VisualElement>("panel-mig21");
        panelTornado = root.Q<VisualElement>("panel-tornado");
        panelRafale = root.Q<VisualElement>("panel-rafale");
        panelTyphoon = root.Q<VisualElement>("panel-typhoon");

        achFirstBlood = root.Q<VisualElement>("ach-first-blood");
        achAce = root.Q<VisualElement>("ach-ace");
        achDoubleAce = root.Q<VisualElement>("ach-double-ace");
        achVeteran = root.Q<VisualElement>("ach-veteran");
        achVictor = root.Q<VisualElement>("ach-victor");
        achTopgun = root.Q<VisualElement>("ach-topgun");
        achCenturion = root.Q<VisualElement>("ach-centurion");

        mainTabStats = root.Q<Button>("main-tab-stats");
        mainTabBadges = root.Q<Button>("main-tab-badges");
        sectionStats = root.Q<VisualElement>("section-stats");
        sectionBadges = root.Q<VisualElement>("section-badges");
        badgeCountLabel = root.Q<Label>("badge-count");

        if (backButton != null)
        {
            backButton.clicked += OnBackClicked;
        }

        if (editCallsignButton != null)
        {
            editCallsignButton.clicked += OnEditCallsignClicked;
        }

        if (mainTabStats != null)
        {
            mainTabStats.clicked += () => SwitchMainSection(0);
            RegisterTabButtonSounds(mainTabStats);
        }

        if (mainTabBadges != null)
        {
            mainTabBadges.clicked += () => SwitchMainSection(1);
            RegisterTabButtonSounds(mainTabBadges);
        }

        if (tabF15 != null)
        {
            tabF15.clicked += () => SwitchAircraftTab(0);
            RegisterTabButtonSounds(tabF15);
        }

        if (tabSU27 != null)
        {
            tabSU27.clicked += () => SwitchAircraftTab(1);
            RegisterTabButtonSounds(tabSU27);
        }

        if (tabMIG29 != null)
        {
            tabMIG29.clicked += () => SwitchAircraftTab(2);
            RegisterTabButtonSounds(tabMIG29);
        }

        if (tabFA18E != null)
        {
            tabFA18E.clicked += () => SwitchAircraftTab(3);
            RegisterTabButtonSounds(tabFA18E);
        }

        if (tabHawk200 != null)
        {
            tabHawk200.clicked += () => SwitchAircraftTab(4);
            RegisterTabButtonSounds(tabHawk200);
        }

        if (tabMIG21 != null)
        {
            tabMIG21.clicked += () => SwitchAircraftTab(5);
            RegisterTabButtonSounds(tabMIG21);
        }

        if (tabTornado != null)
        {
            tabTornado.clicked += () => SwitchAircraftTab(6);
            RegisterTabButtonSounds(tabTornado);
        }

        if (tabRafale != null)
        {
            tabRafale.clicked += () => SwitchAircraftTab(7);
            RegisterTabButtonSounds(tabRafale);
        }

        if (tabTyphoon != null)
        {
            tabTyphoon.clicked += () => SwitchAircraftTab(8);
            RegisterTabButtonSounds(tabTyphoon);
        }
    }

    void RegisterTabButtonSounds(Button button)
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

    void SwitchAircraftTab(int tabIndex)
    {
        tabF15?.RemoveFromClassList("aircraft-tab-active");
        tabSU27?.RemoveFromClassList("aircraft-tab-active");
        tabMIG29?.RemoveFromClassList("aircraft-tab-active");
        tabFA18E?.RemoveFromClassList("aircraft-tab-active");
        tabHawk200?.RemoveFromClassList("aircraft-tab-active");
        tabMIG21?.RemoveFromClassList("aircraft-tab-active");
        tabTornado?.RemoveFromClassList("aircraft-tab-active");
        tabRafale?.RemoveFromClassList("aircraft-tab-active");
        tabTyphoon?.RemoveFromClassList("aircraft-tab-active");

        panelF15?.RemoveFromClassList("aircraft-panel-visible");
        panelSU27?.RemoveFromClassList("aircraft-panel-visible");
        panelMIG29?.RemoveFromClassList("aircraft-panel-visible");
        panelFA18E?.RemoveFromClassList("aircraft-panel-visible");
        panelHawk200?.RemoveFromClassList("aircraft-panel-visible");
        panelMIG21?.RemoveFromClassList("aircraft-panel-visible");
        panelTornado?.RemoveFromClassList("aircraft-panel-visible");
        panelRafale?.RemoveFromClassList("aircraft-panel-visible");
        panelTyphoon?.RemoveFromClassList("aircraft-panel-visible");

        switch (tabIndex)
        {
            case 0:
                tabF15?.AddToClassList("aircraft-tab-active");
                panelF15?.AddToClassList("aircraft-panel-visible");
                break;
            case 1:
                tabSU27?.AddToClassList("aircraft-tab-active");
                panelSU27?.AddToClassList("aircraft-panel-visible");
                break;
            case 2:
                tabMIG29?.AddToClassList("aircraft-tab-active");
                panelMIG29?.AddToClassList("aircraft-panel-visible");
                break;
            case 3:
                tabFA18E?.AddToClassList("aircraft-tab-active");
                panelFA18E?.AddToClassList("aircraft-panel-visible");
                break;
            case 4:
                tabHawk200?.AddToClassList("aircraft-tab-active");
                panelHawk200?.AddToClassList("aircraft-panel-visible");
                break;
            case 5:
                tabMIG21?.AddToClassList("aircraft-tab-active");
                panelMIG21?.AddToClassList("aircraft-panel-visible");
                break;
            case 6:
                tabTornado?.AddToClassList("aircraft-tab-active");
                panelTornado?.AddToClassList("aircraft-panel-visible");
                break;
            case 7:
                tabRafale?.AddToClassList("aircraft-tab-active");
                panelRafale?.AddToClassList("aircraft-panel-visible");
                break;
            case 8:
                tabTyphoon?.AddToClassList("aircraft-tab-active");
                panelTyphoon?.AddToClassList("aircraft-panel-visible");
                break;
        }
    }

    public void SwitchMainSection(int sectionIndex)
    {
        mainTabStats?.RemoveFromClassList("main-section-tab-active");
        mainTabBadges?.RemoveFromClassList("main-section-tab-active");

        if (sectionStats != null)
            sectionStats.style.display = DisplayStyle.None;
        if (sectionBadges != null)
            sectionBadges.style.display = DisplayStyle.None;

        switch (sectionIndex)
        {
            case 0:
                mainTabStats?.AddToClassList("main-section-tab-active");
                if (sectionStats != null)
                    sectionStats.style.display = DisplayStyle.Flex;
                break;
            case 1:
                mainTabBadges?.AddToClassList("main-section-tab-active");
                if (sectionBadges != null)
                    sectionBadges.style.display = DisplayStyle.Flex;
                break;
        }
    }

    public void Show()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuOpen();
        }

        SetupUI();

        if (root == null && uiDocument != null && uiDocument.rootVisualElement != null)
        {
            root = uiDocument.rootVisualElement.Q<VisualElement>("root");
            if (root == null)
            {
                root = uiDocument.rootVisualElement;
            }
        }

        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
            RefreshStats();
            Debug.Log("[PlayerStatsController] Profile shown");
        }
        else
        {
            Debug.LogError("[PlayerStatsController] Root element not found!");
        }
    }

    public void Hide()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuClose();
        }

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
    }

    public void RefreshStats()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager not found. Stats will show default values.");
            return;
        }

        var gm = GameManager.Instance;

        if (callsignLabel != null)
            callsignLabel.text = gm.PlayerCallsign;

        if (rankLabel != null)
            rankLabel.text = gm.GetRankName(gm.PlayerRank);

        int currentRankXP = (gm.PlayerRank - 1) * 1000;
        int nextRankXP = gm.PlayerRank * 1000;
        int xpInRank = gm.PlayerXP - currentRankXP;
        int xpNeeded = 1000;
        float xpPercent = Mathf.Clamp01((float)xpInRank / xpNeeded) * 100f;

        if (xpText != null)
            xpText.text = $"{xpInRank:N0} / {xpNeeded:N0} XP";

        if (xpBar != null)
            xpBar.style.width = new Length(xpPercent, LengthUnit.Percent);

        if (totalKillsLabel != null)
            totalKillsLabel.text = gm.TotalKills.ToString("N0");

        if (totalWinsLabel != null)
            totalWinsLabel.text = gm.TotalWins.ToString("N0");

        if (flightTimeLabel != null)
            flightTimeLabel.text = gm.FormatFlightTime(gm.TotalFlightTime);

        if (missilesFiredLabel != null)
            missilesFiredLabel.text = gm.TotalMissilesFired.ToString("N0");

        if (accuracyLabel != null)
            accuracyLabel.text = $"{gm.GetAccuracy():F1}%";

        if (kdRatioLabel != null)
            kdRatioLabel.text = gm.GetKDRatio().ToString("F1");

        if (f15KillsLabel != null)
            f15KillsLabel.text = gm.F15Kills.ToString("N0");

        if (f15TimeLabel != null)
            f15TimeLabel.text = gm.FormatFlightTime(gm.F15FlightTime);

        if (f15SortiesLabel != null)
            f15SortiesLabel.text = gm.F15Sorties.ToString("N0");

        if (su27KillsLabel != null)
            su27KillsLabel.text = gm.SU27Kills.ToString("N0");

        if (su27TimeLabel != null)
            su27TimeLabel.text = gm.FormatFlightTime(gm.SU27FlightTime);

        if (su27SortiesLabel != null)
            su27SortiesLabel.text = gm.SU27Sorties.ToString("N0");

        if (mig29KillsLabel != null)
            mig29KillsLabel.text = gm.MIG29Kills.ToString("N0");

        if (mig29TimeLabel != null)
            mig29TimeLabel.text = gm.FormatFlightTime(gm.MIG29FlightTime);

        if (mig29SortiesLabel != null)
            mig29SortiesLabel.text = gm.MIG29Sorties.ToString("N0");

        if (fa18eKillsLabel != null)
            fa18eKillsLabel.text = gm.FA18EKills.ToString("N0");

        if (fa18eTimeLabel != null)
            fa18eTimeLabel.text = gm.FormatFlightTime(gm.FA18EFlightTime);

        if (fa18eSortiesLabel != null)
            fa18eSortiesLabel.text = gm.FA18ESorties.ToString("N0");

        if (hawk200KillsLabel != null)
            hawk200KillsLabel.text = gm.Hawk200Kills.ToString("N0");

        if (hawk200TimeLabel != null)
            hawk200TimeLabel.text = gm.FormatFlightTime(gm.Hawk200FlightTime);

        if (hawk200SortiesLabel != null)
            hawk200SortiesLabel.text = gm.Hawk200Sorties.ToString("N0");

        if (mig21KillsLabel != null)
            mig21KillsLabel.text = gm.MIG21Kills.ToString("N0");

        if (mig21TimeLabel != null)
            mig21TimeLabel.text = gm.FormatFlightTime(gm.MIG21FlightTime);

        if (mig21SortiesLabel != null)
            mig21SortiesLabel.text = gm.MIG21Sorties.ToString("N0");

        if (tornadoKillsLabel != null)
            tornadoKillsLabel.text = gm.TornadoKills.ToString("N0");

        if (tornadoTimeLabel != null)
            tornadoTimeLabel.text = gm.FormatFlightTime(gm.TornadoFlightTime);

        if (tornadoSortiesLabel != null)
            tornadoSortiesLabel.text = gm.TornadoSorties.ToString("N0");

        if (rafaleKillsLabel != null)
            rafaleKillsLabel.text = gm.RafaleKills.ToString("N0");

        if (rafaleTimeLabel != null)
            rafaleTimeLabel.text = gm.FormatFlightTime(gm.RafaleFlightTime);

        if (rafaleSortiesLabel != null)
            rafaleSortiesLabel.text = gm.RafaleSorties.ToString("N0");

        if (typhoonKillsLabel != null)
            typhoonKillsLabel.text = gm.TyphoonKills.ToString("N0");

        if (typhoonTimeLabel != null)
            typhoonTimeLabel.text = gm.FormatFlightTime(gm.TyphoonFlightTime);

        if (typhoonSortiesLabel != null)
            typhoonSortiesLabel.text = gm.TyphoonSorties.ToString("N0");

        UpdateAchievements(gm);
        UpdateBadges(gm);
    }

    void UpdateBadges(GameManager gm)
    {
        int totalBadges = 44;
        int unlockedBadges = 0;

        unlockedBadges += UpdateBadgeState("badge-first-kill", gm.TotalKills >= 1);
        unlockedBadges += UpdateBadgeState("badge-5-kills", gm.TotalKills >= 5);
        unlockedBadges += UpdateBadgeState("badge-10-kills", gm.TotalKills >= 10);
        unlockedBadges += UpdateBadgeState("badge-25-kills", gm.TotalKills >= 25);
        unlockedBadges += UpdateBadgeState("badge-50-kills", gm.TotalKills >= 50);
        unlockedBadges += UpdateBadgeState("badge-100-kills", gm.TotalKills >= 100);
        unlockedBadges += UpdateBadgeState("badge-250-kills", gm.TotalKills >= 250);
        unlockedBadges += UpdateBadgeState("badge-500-kills", gm.TotalKills >= 500);

        unlockedBadges += UpdateBadgeState("badge-first-win", gm.TotalWins >= 1);
        unlockedBadges += UpdateBadgeState("badge-5-wins", gm.TotalWins >= 5);
        unlockedBadges += UpdateBadgeState("badge-10-wins", gm.TotalWins >= 10);
        unlockedBadges += UpdateBadgeState("badge-25-wins", gm.TotalWins >= 25);
        unlockedBadges += UpdateBadgeState("badge-50-wins", gm.TotalWins >= 50);

        unlockedBadges += UpdateBadgeState("badge-1hr-flight", gm.TotalFlightTime >= 3600f);
        unlockedBadges += UpdateBadgeState("badge-5hr-flight", gm.TotalFlightTime >= 18000f);
        unlockedBadges += UpdateBadgeState("badge-10hr-flight", gm.TotalFlightTime >= 36000f);
        unlockedBadges += UpdateBadgeState("badge-24hr-flight", gm.TotalFlightTime >= 86400f);

        unlockedBadges += UpdateBadgeState("badge-10-sorties", gm.TotalSorties >= 10);
        unlockedBadges += UpdateBadgeState("badge-25-sorties", gm.TotalSorties >= 25);
        unlockedBadges += UpdateBadgeState("badge-50-sorties", gm.TotalSorties >= 50);
        unlockedBadges += UpdateBadgeState("badge-100-sorties", gm.TotalSorties >= 100);

        unlockedBadges += UpdateBadgeState("badge-flew-f15", gm.F15Sorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-su27", gm.SU27Sorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-mig29", gm.MIG29Sorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-fa18e", gm.FA18ESorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-hawk200", gm.Hawk200Sorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-mig21", gm.MIG21Sorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-tornado", gm.TornadoSorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-rafale", gm.RafaleSorties >= 1);
        unlockedBadges += UpdateBadgeState("badge-flew-typhoon", gm.TyphoonSorties >= 1);

        unlockedBadges += UpdateBadgeState("badge-f15-10kills", gm.F15Kills >= 10);
        unlockedBadges += UpdateBadgeState("badge-su27-10kills", gm.SU27Kills >= 10);
        unlockedBadges += UpdateBadgeState("badge-mig29-10kills", gm.MIG29Kills >= 10);
        unlockedBadges += UpdateBadgeState("badge-fa18e-10kills", gm.FA18EKills >= 10);
        unlockedBadges += UpdateBadgeState("badge-hawk200-10kills", gm.Hawk200Kills >= 10);
        unlockedBadges += UpdateBadgeState("badge-mig21-10kills", gm.MIG21Kills >= 10);
        unlockedBadges += UpdateBadgeState("badge-tornado-10kills", gm.TornadoKills >= 10);
        unlockedBadges += UpdateBadgeState("badge-rafale-10kills", gm.RafaleKills >= 10);
        unlockedBadges += UpdateBadgeState("badge-typhoon-10kills", gm.TyphoonKills >= 10);

        bool allAircraftFlown =
            gm.F15Sorties >= 1
            && gm.SU27Sorties >= 1
            && gm.MIG29Sorties >= 1
            && gm.FA18ESorties >= 1
            && gm.Hawk200Sorties >= 1
            && gm.MIG21Sorties >= 1
            && gm.TornadoSorties >= 1
            && gm.RafaleSorties >= 1
            && gm.TyphoonSorties >= 1;
        unlockedBadges += UpdateBadgeState("badge-all-aircraft", allAircraftFlown);
        unlockedBadges += UpdateBadgeState("badge-rank-5", gm.PlayerRank >= 5);
        unlockedBadges += UpdateBadgeState("badge-rank-10", gm.PlayerRank >= 10);
        unlockedBadges += UpdateBadgeState("badge-missiles-100", gm.TotalMissilesFired >= 100);
        unlockedBadges += UpdateBadgeState("badge-missiles-500", gm.TotalMissilesFired >= 500);

        if (badgeCountLabel != null)
            badgeCountLabel.text = $"{unlockedBadges} / {totalBadges} Badges Unlocked";
    }

    int UpdateBadgeState(string badgeName, bool unlocked)
    {
        var badge = root?.Q<VisualElement>(badgeName);
        if (badge == null)
            return 0;

        badge.RemoveFromClassList(unlocked ? "locked" : "unlocked");
        badge.AddToClassList(unlocked ? "unlocked" : "locked");

        return unlocked ? 1 : 0;
    }

    void UpdateAchievements(GameManager gm)
    {
        int unlockedCount = 0;

        if (achFirstBlood != null)
        {
            bool unlocked = gm.TotalKills >= 1;
            achFirstBlood.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achFirstBlood.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;
        }

        if (achAce != null)
        {
            bool unlocked = gm.TotalKills >= 5;
            achAce.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achAce.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;

            var bar = achAce.Q<VisualElement>("ach-ace-bar");
            var text = achAce.Q<Label>("ach-ace-text");
            if (bar != null)
                bar.style.width = new Length(
                    Mathf.Min(100, gm.TotalKills * 20),
                    LengthUnit.Percent
                );
            if (text != null)
                text.text = $"{Mathf.Min(5, gm.TotalKills)} / 5";
        }

        if (achDoubleAce != null)
        {
            bool unlocked = gm.TotalKills >= 10;
            achDoubleAce.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achDoubleAce.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;

            var bar = achDoubleAce.Q<VisualElement>("ach-double-ace-bar");
            var text = achDoubleAce.Q<Label>("ach-double-ace-text");
            if (bar != null)
                bar.style.width = new Length(
                    Mathf.Min(100, gm.TotalKills * 10),
                    LengthUnit.Percent
                );
            if (text != null)
                text.text = $"{Mathf.Min(10, gm.TotalKills)} / 10";
        }

        if (achVeteran != null)
        {
            bool unlocked = gm.TotalKills >= 50;
            achVeteran.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achVeteran.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;

            var bar = achVeteran.Q<VisualElement>("ach-veteran-bar");
            var text = achVeteran.Q<Label>("ach-veteran-text");
            if (bar != null)
                bar.style.width = new Length(Mathf.Min(100, gm.TotalKills * 2), LengthUnit.Percent);
            if (text != null)
                text.text = $"{Mathf.Min(50, gm.TotalKills)} / 50";
        }

        if (achVictor != null)
        {
            bool unlocked = gm.TotalWins >= 1;
            achVictor.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achVictor.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;
        }

        if (achTopgun != null)
        {
            bool unlocked = gm.TotalKills >= 100;
            achTopgun.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achTopgun.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;

            var bar = achTopgun.Q<VisualElement>("ach-topgun-bar");
            var text = achTopgun.Q<Label>("ach-topgun-text");
            if (bar != null)
                bar.style.width = new Length(Mathf.Min(100, gm.TotalKills), LengthUnit.Percent);
            if (text != null)
                text.text = $"{Mathf.Min(100, gm.TotalKills)} / 100";
        }

        if (achCenturion != null)
        {
            bool unlocked = gm.TotalWins >= 10;
            achCenturion.RemoveFromClassList(unlocked ? "locked" : "unlocked");
            achCenturion.AddToClassList(unlocked ? "unlocked" : "locked");
            if (unlocked)
                unlockedCount++;

            var bar = achCenturion.Q<VisualElement>("ach-centurion-bar");
            var text = achCenturion.Q<Label>("ach-centurion-text");
            if (bar != null)
                bar.style.width = new Length(Mathf.Min(100, gm.TotalWins * 10), LengthUnit.Percent);
            if (text != null)
                text.text = $"{Mathf.Min(10, gm.TotalWins)} / 10";
        }

        if (achievementCountLabel != null)
            achievementCountLabel.text = $"{unlockedCount} / 7 Achievements Unlocked";
    }

    void OnBackClicked()
    {
        Hide();

        var mainMenu = FindFirstObjectByType<MainMenuController>();
        if (mainMenu != null)
        {
            mainMenu.Show();
        }
    }

    void OnEditCallsignClicked()
    {
        Debug.Log("Edit callsign clicked - feature coming soon!");
    }

    void OnDestroy()
    {
        if (backButton != null)
            backButton.clicked -= OnBackClicked;
        if (editCallsignButton != null)
            editCallsignButton.clicked -= OnEditCallsignClicked;
    }
}
