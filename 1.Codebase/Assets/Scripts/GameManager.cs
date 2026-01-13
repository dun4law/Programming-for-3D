using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField]
    private bool isPaused = false;

    [SerializeField]
    private bool isGameOver = false;

    [SerializeField]
    private float gameTime = 0f;

    [Header("Session Stats")]
    [SerializeField]
    private int sessionKills = 0;

    [SerializeField]
    private int sessionDeaths = 0;

    [SerializeField]
    private int missionsCompleted = 0;

    private float autoSaveTimer = 0f;
    private const float AUTO_SAVE_INTERVAL = 300f;

    private const string KEY_TOTAL_KILLS = "PlayerStats_TotalKills";
    private const string KEY_TOTAL_DEATHS = "PlayerStats_TotalDeaths";
    private const string KEY_TOTAL_WINS = "PlayerStats_TotalWins";
    private const string KEY_TOTAL_FLIGHT_TIME = "PlayerStats_TotalFlightTime";
    private const string KEY_TOTAL_MISSILES_FIRED = "PlayerStats_TotalMissilesFired";
    private const string KEY_TOTAL_MISSILES_HIT = "PlayerStats_TotalMissilesHit";
    private const string KEY_F15_KILLS = "PlayerStats_F15Kills";
    private const string KEY_F15_FLIGHT_TIME = "PlayerStats_F15FlightTime";
    private const string KEY_F15_SORTIES = "PlayerStats_F15Sorties";
    private const string KEY_SU27_KILLS = "PlayerStats_SU27Kills";
    private const string KEY_SU27_FLIGHT_TIME = "PlayerStats_SU27FlightTime";
    private const string KEY_SU27_SORTIES = "PlayerStats_SU27Sorties";
    private const string KEY_MIG29_KILLS = "PlayerStats_MIG29Kills";
    private const string KEY_MIG29_FLIGHT_TIME = "PlayerStats_MIG29FlightTime";
    private const string KEY_MIG29_SORTIES = "PlayerStats_MIG29Sorties";
    private const string KEY_FA18E_KILLS = "PlayerStats_FA18EKills";
    private const string KEY_FA18E_FLIGHT_TIME = "PlayerStats_FA18EFlightTime";
    private const string KEY_FA18E_SORTIES = "PlayerStats_FA18ESorties";
    private const string KEY_HAWK200_KILLS = "PlayerStats_Hawk200Kills";
    private const string KEY_HAWK200_FLIGHT_TIME = "PlayerStats_Hawk200FlightTime";
    private const string KEY_HAWK200_SORTIES = "PlayerStats_Hawk200Sorties";
    private const string KEY_MIG21_KILLS = "PlayerStats_MIG21Kills";
    private const string KEY_MIG21_FLIGHT_TIME = "PlayerStats_MIG21FlightTime";
    private const string KEY_MIG21_SORTIES = "PlayerStats_MIG21Sorties";
    private const string KEY_TORNADO_KILLS = "PlayerStats_TornadoKills";
    private const string KEY_TORNADO_FLIGHT_TIME = "PlayerStats_TornadoFlightTime";
    private const string KEY_TORNADO_SORTIES = "PlayerStats_TornadoSorties";
    private const string KEY_RAFALE_KILLS = "PlayerStats_RafaleKills";
    private const string KEY_RAFALE_FLIGHT_TIME = "PlayerStats_RafaleFlightTime";
    private const string KEY_RAFALE_SORTIES = "PlayerStats_RafaleSorties";
    private const string KEY_TYPHOON_KILLS = "PlayerStats_TyphoonKills";
    private const string KEY_TYPHOON_FLIGHT_TIME = "PlayerStats_TyphoonFlightTime";
    private const string KEY_TYPHOON_SORTIES = "PlayerStats_TyphoonSorties";
    private const string KEY_PLAYER_CALLSIGN = "PlayerStats_Callsign";
    private const string KEY_PLAYER_RANK = "PlayerStats_Rank";
    private const string KEY_PLAYER_XP = "PlayerStats_XP";
    private const string KEY_TOTAL_SORTIES = "PlayerStats_TotalSorties";

    public System.Action OnGamePaused;
    public System.Action OnGameResumed;
    public System.Action OnGameOver;
    public System.Action<bool> OnMissionComplete;

    public int TotalKills { get; private set; }
    public int TotalDeaths { get; private set; }
    public int TotalWins { get; private set; }
    public float TotalFlightTime { get; private set; }
    public int TotalMissilesFired { get; private set; }
    public int TotalMissilesHit { get; private set; }
    public int F15Kills { get; private set; }
    public float F15FlightTime { get; private set; }
    public int F15Sorties { get; private set; }
    public int SU27Kills { get; private set; }
    public float SU27FlightTime { get; private set; }
    public int SU27Sorties { get; private set; }
    public int MIG29Kills { get; private set; }
    public float MIG29FlightTime { get; private set; }
    public int MIG29Sorties { get; private set; }
    public int FA18EKills { get; private set; }
    public float FA18EFlightTime { get; private set; }
    public int FA18ESorties { get; private set; }
    public int Hawk200Kills { get; private set; }
    public float Hawk200FlightTime { get; private set; }
    public int Hawk200Sorties { get; private set; }
    public int MIG21Kills { get; private set; }
    public float MIG21FlightTime { get; private set; }
    public int MIG21Sorties { get; private set; }
    public int TornadoKills { get; private set; }
    public float TornadoFlightTime { get; private set; }
    public int TornadoSorties { get; private set; }
    public int RafaleKills { get; private set; }
    public float RafaleFlightTime { get; private set; }
    public int RafaleSorties { get; private set; }
    public int TyphoonKills { get; private set; }
    public float TyphoonFlightTime { get; private set; }
    public int TyphoonSorties { get; private set; }
    public string PlayerCallsign { get; private set; }
    public int PlayerRank { get; private set; }
    public int PlayerXP { get; private set; }
    public int TotalSorties { get; private set; }
    public string SelectedAircraft { get; set; } = "F15";

    public bool IsPaused => isPaused;
    public bool IsGameOver => isGameOver;
    public float GameTime => gameTime;
    public int SessionKills => sessionKills;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPersistentStats();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        DifficultySettings.LogCurrentDifficulty();

        if (KillTracker.Instance != null)
        {
            KillTracker.Instance.OnKillCountChanged += OnKillCountChanged;
            KillTracker.Instance.OnDeathCountChanged += OnDeathCountChanged;
        }
    }

    void Update()
    {
        if (!isPaused && !isGameOver)
        {
            gameTime += Time.deltaTime;
            UpdateAutoSave(Time.deltaTime);
        }
    }

    private void UpdateAutoSave(float deltaTime)
    {
        bool autoSaveEnabled = PlayerPrefs.GetInt("AutoSave", 1) == 1;
        if (!autoSaveEnabled)
        {
            autoSaveTimer = 0f;
            return;
        }

        autoSaveTimer += deltaTime;
        if (autoSaveTimer >= AUTO_SAVE_INTERVAL)
        {
            autoSaveTimer = 0f;
            SavePersistentStats();
            Debug.Log("[GameManager] Auto-saved game progress");
        }
    }

    void OnDestroy()
    {
        if (KillTracker.Instance != null)
        {
            KillTracker.Instance.OnKillCountChanged -= OnKillCountChanged;
            KillTracker.Instance.OnDeathCountChanged -= OnDeathCountChanged;
        }
    }

    void OnKillCountChanged(int kills)
    {
        sessionKills = kills;
    }

    void OnDeathCountChanged(int deaths)
    {
        sessionDeaths = deaths;
    }

    public void LoadPersistentStats()
    {
        TotalKills = PlayerPrefs.GetInt(KEY_TOTAL_KILLS, 0);
        TotalDeaths = PlayerPrefs.GetInt(KEY_TOTAL_DEATHS, 0);
        TotalWins = PlayerPrefs.GetInt(KEY_TOTAL_WINS, 0);
        TotalFlightTime = PlayerPrefs.GetFloat(KEY_TOTAL_FLIGHT_TIME, 0f);
        TotalMissilesFired = PlayerPrefs.GetInt(KEY_TOTAL_MISSILES_FIRED, 0);
        TotalMissilesHit = PlayerPrefs.GetInt(KEY_TOTAL_MISSILES_HIT, 0);
        F15Kills = PlayerPrefs.GetInt(KEY_F15_KILLS, 0);
        F15FlightTime = PlayerPrefs.GetFloat(KEY_F15_FLIGHT_TIME, 0f);
        F15Sorties = PlayerPrefs.GetInt(KEY_F15_SORTIES, 0);
        SU27Kills = PlayerPrefs.GetInt(KEY_SU27_KILLS, 0);
        SU27FlightTime = PlayerPrefs.GetFloat(KEY_SU27_FLIGHT_TIME, 0f);
        SU27Sorties = PlayerPrefs.GetInt(KEY_SU27_SORTIES, 0);
        MIG29Kills = PlayerPrefs.GetInt(KEY_MIG29_KILLS, 0);
        MIG29FlightTime = PlayerPrefs.GetFloat(KEY_MIG29_FLIGHT_TIME, 0f);
        MIG29Sorties = PlayerPrefs.GetInt(KEY_MIG29_SORTIES, 0);
        FA18EKills = PlayerPrefs.GetInt(KEY_FA18E_KILLS, 0);
        FA18EFlightTime = PlayerPrefs.GetFloat(KEY_FA18E_FLIGHT_TIME, 0f);
        FA18ESorties = PlayerPrefs.GetInt(KEY_FA18E_SORTIES, 0);
        Hawk200Kills = PlayerPrefs.GetInt(KEY_HAWK200_KILLS, 0);
        Hawk200FlightTime = PlayerPrefs.GetFloat(KEY_HAWK200_FLIGHT_TIME, 0f);
        Hawk200Sorties = PlayerPrefs.GetInt(KEY_HAWK200_SORTIES, 0);
        MIG21Kills = PlayerPrefs.GetInt(KEY_MIG21_KILLS, 0);
        MIG21FlightTime = PlayerPrefs.GetFloat(KEY_MIG21_FLIGHT_TIME, 0f);
        MIG21Sorties = PlayerPrefs.GetInt(KEY_MIG21_SORTIES, 0);
        TornadoKills = PlayerPrefs.GetInt(KEY_TORNADO_KILLS, 0);
        TornadoFlightTime = PlayerPrefs.GetFloat(KEY_TORNADO_FLIGHT_TIME, 0f);
        TornadoSorties = PlayerPrefs.GetInt(KEY_TORNADO_SORTIES, 0);
        RafaleKills = PlayerPrefs.GetInt(KEY_RAFALE_KILLS, 0);
        RafaleFlightTime = PlayerPrefs.GetFloat(KEY_RAFALE_FLIGHT_TIME, 0f);
        RafaleSorties = PlayerPrefs.GetInt(KEY_RAFALE_SORTIES, 0);
        TyphoonKills = PlayerPrefs.GetInt(KEY_TYPHOON_KILLS, 0);
        TyphoonFlightTime = PlayerPrefs.GetFloat(KEY_TYPHOON_FLIGHT_TIME, 0f);
        TyphoonSorties = PlayerPrefs.GetInt(KEY_TYPHOON_SORTIES, 0);
        PlayerCallsign = PlayerPrefs.GetString(KEY_PLAYER_CALLSIGN, "PHOENIX LEAD");
        PlayerRank = PlayerPrefs.GetInt(KEY_PLAYER_RANK, 1);
        PlayerXP = PlayerPrefs.GetInt(KEY_PLAYER_XP, 0);
        TotalSorties = PlayerPrefs.GetInt(KEY_TOTAL_SORTIES, 0);

        SelectedAircraft = PlayerPrefs.GetString(
            AircraftSelectionApplier.SelectedAircraftKey,
            "F15"
        );

        Debug.Log(
            $" Loaded stats: Kills={TotalKills}, Wins={TotalWins}, FlightTime={TotalFlightTime:F1}s"
        );
    }

    public void SavePersistentStats()
    {
        PlayerPrefs.SetInt(KEY_TOTAL_KILLS, TotalKills);
        PlayerPrefs.SetInt(KEY_TOTAL_DEATHS, TotalDeaths);
        PlayerPrefs.SetInt(KEY_TOTAL_WINS, TotalWins);
        PlayerPrefs.SetFloat(KEY_TOTAL_FLIGHT_TIME, TotalFlightTime);
        PlayerPrefs.SetInt(KEY_TOTAL_MISSILES_FIRED, TotalMissilesFired);
        PlayerPrefs.SetInt(KEY_TOTAL_MISSILES_HIT, TotalMissilesHit);
        PlayerPrefs.SetInt(KEY_F15_KILLS, F15Kills);
        PlayerPrefs.SetFloat(KEY_F15_FLIGHT_TIME, F15FlightTime);
        PlayerPrefs.SetInt(KEY_F15_SORTIES, F15Sorties);
        PlayerPrefs.SetInt(KEY_SU27_KILLS, SU27Kills);
        PlayerPrefs.SetFloat(KEY_SU27_FLIGHT_TIME, SU27FlightTime);
        PlayerPrefs.SetInt(KEY_SU27_SORTIES, SU27Sorties);
        PlayerPrefs.SetInt(KEY_MIG29_KILLS, MIG29Kills);
        PlayerPrefs.SetFloat(KEY_MIG29_FLIGHT_TIME, MIG29FlightTime);
        PlayerPrefs.SetInt(KEY_MIG29_SORTIES, MIG29Sorties);
        PlayerPrefs.SetInt(KEY_FA18E_KILLS, FA18EKills);
        PlayerPrefs.SetFloat(KEY_FA18E_FLIGHT_TIME, FA18EFlightTime);
        PlayerPrefs.SetInt(KEY_FA18E_SORTIES, FA18ESorties);
        PlayerPrefs.SetInt(KEY_HAWK200_KILLS, Hawk200Kills);
        PlayerPrefs.SetFloat(KEY_HAWK200_FLIGHT_TIME, Hawk200FlightTime);
        PlayerPrefs.SetInt(KEY_HAWK200_SORTIES, Hawk200Sorties);
        PlayerPrefs.SetInt(KEY_MIG21_KILLS, MIG21Kills);
        PlayerPrefs.SetFloat(KEY_MIG21_FLIGHT_TIME, MIG21FlightTime);
        PlayerPrefs.SetInt(KEY_MIG21_SORTIES, MIG21Sorties);
        PlayerPrefs.SetInt(KEY_TORNADO_KILLS, TornadoKills);
        PlayerPrefs.SetFloat(KEY_TORNADO_FLIGHT_TIME, TornadoFlightTime);
        PlayerPrefs.SetInt(KEY_TORNADO_SORTIES, TornadoSorties);
        PlayerPrefs.SetInt(KEY_RAFALE_KILLS, RafaleKills);
        PlayerPrefs.SetFloat(KEY_RAFALE_FLIGHT_TIME, RafaleFlightTime);
        PlayerPrefs.SetInt(KEY_RAFALE_SORTIES, RafaleSorties);
        PlayerPrefs.SetInt(KEY_TYPHOON_KILLS, TyphoonKills);
        PlayerPrefs.SetFloat(KEY_TYPHOON_FLIGHT_TIME, TyphoonFlightTime);
        PlayerPrefs.SetInt(KEY_TYPHOON_SORTIES, TyphoonSorties);
        PlayerPrefs.SetString(KEY_PLAYER_CALLSIGN, PlayerCallsign);
        PlayerPrefs.SetInt(KEY_PLAYER_RANK, PlayerRank);
        PlayerPrefs.SetInt(KEY_PLAYER_XP, PlayerXP);
        PlayerPrefs.SetInt(KEY_TOTAL_SORTIES, TotalSorties);

        PlayerPrefs.SetString(AircraftSelectionApplier.SelectedAircraftKey, SelectedAircraft);
        PlayerPrefs.Save();

        Debug.Log(" Stats saved!");
    }

    public void ScheduleGameOver(string deathReason, float delay)
    {
        if (isGameOver)
            return;

        Debug.Log($" ScheduleGameOver: Scheduling in {delay}s, reason={deathReason}");
        StartCoroutine(GameOverDelayedCoroutine(deathReason, delay));
    }

    private System.Collections.IEnumerator GameOverDelayedCoroutine(string deathReason, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        Debug.Log($" GameOverDelayedCoroutine: Delay complete, calling CompleteMission");
        CompleteMission(false, deathReason);
    }

    public void CompleteMission(bool victory, string deathReason = null)
    {
        isGameOver = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGameplaySounds();
            if (victory)
            {
                AudioManager.Instance.PlayMissionAccomplished();
            }
            else
            {
                AudioManager.Instance.PlayGameOver();
            }
        }

        if (victory)
        {
            TotalWins++;
            missionsCompleted++;
        }

        TotalSorties++;

        int kills = 0;
        float accuracy = 0f;

        if (KillTracker.Instance != null)
        {
            kills = KillTracker.Instance.Kills;
            accuracy = KillTracker.Instance.GetAccuracy();

            TotalKills += KillTracker.Instance.Kills;
            TotalDeaths += KillTracker.Instance.Deaths;
            TotalMissilesFired += KillTracker.Instance.MissilesFired;
            TotalMissilesHit += KillTracker.Instance.MissilesHit;

            switch (SelectedAircraft)
            {
                case "Su27":
                    SU27Kills += KillTracker.Instance.Kills;
                    break;
                case "Mig29":
                    MIG29Kills += KillTracker.Instance.Kills;
                    break;
                case "fa18e":
                    FA18EKills += KillTracker.Instance.Kills;
                    break;
                case "Hawk_200":
                    Hawk200Kills += KillTracker.Instance.Kills;
                    break;
                case "mig21":
                    MIG21Kills += KillTracker.Instance.Kills;
                    break;
                case "panavia-tornado":
                    TornadoKills += KillTracker.Instance.Kills;
                    break;
                case "rafalemf3":
                    RafaleKills += KillTracker.Instance.Kills;
                    break;
                case "Typhoon":
                    TyphoonKills += KillTracker.Instance.Kills;
                    break;
                case "F15":
                default:
                    F15Kills += KillTracker.Instance.Kills;
                    break;
            }
        }

        TotalFlightTime += gameTime;

        switch (SelectedAircraft)
        {
            case "Su27":
                SU27FlightTime += gameTime;
                SU27Sorties++;
                break;
            case "Mig29":
                MIG29FlightTime += gameTime;
                MIG29Sorties++;
                break;
            case "fa18e":
                FA18EFlightTime += gameTime;
                FA18ESorties++;
                break;
            case "Hawk_200":
                Hawk200FlightTime += gameTime;
                Hawk200Sorties++;
                break;
            case "mig21":
                MIG21FlightTime += gameTime;
                MIG21Sorties++;
                break;
            case "panavia-tornado":
                TornadoFlightTime += gameTime;
                TornadoSorties++;
                break;
            case "rafalemf3":
                RafaleFlightTime += gameTime;
                RafaleSorties++;
                break;
            case "Typhoon":
                TyphoonFlightTime += gameTime;
                TyphoonSorties++;
                break;
            case "F15":
            default:
                F15FlightTime += gameTime;
                F15Sorties++;
                break;
        }

        int xpEarned = CalculateXPEarned(victory);
        AddXP(xpEarned);

        SavePersistentStats();

        Debug.Log("[GameManager] ====== CHECKING BADGES AFTER MISSION COMPLETE ======");
        BadgeTracker.CheckForNewBadges($"GameManager.CompleteMission(victory={victory})");

        OnMissionComplete?.Invoke(victory);

        Debug.Log(
            $" Mission complete! Victory={victory}, Kills={sessionKills}, Time={gameTime:F1}s"
        );

        if (string.IsNullOrEmpty(deathReason))
        {
            deathReason = victory ? null : "Killed in action";
        }
        string deathLocation = victory ? null : "Over the battlefield";

        int mMissilesFired = KillTracker.Instance != null ? KillTracker.Instance.MissilesFired : 0;
        int mMissilesHit = KillTracker.Instance != null ? KillTracker.Instance.MissilesHit : 0;
        int mBulletsFired = KillTracker.Instance != null ? KillTracker.Instance.BulletsFired : 0;
        int mBulletsHit = KillTracker.Instance != null ? KillTracker.Instance.BulletsHit : 0;

        MissionResultController.SetMissionResult(
            victory: victory,
            kills: kills,
            accuracy: accuracy,
            gameTime: gameTime,
            deathReason: deathReason,
            deathLocation: deathLocation,
            missionName: "Hong Kong Defense",
            xpEarned: xpEarned,
            missilesFired: mMissilesFired,
            missilesHit: mMissilesHit,
            bulletsFired: mBulletsFired,
            bulletsHit: mBulletsHit
        );

        MainMenuController.ShouldStartImmediately = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu and story");
    }

    [System.Obsolete("Use MissionResultController instead")]
    public void ShowGameOverScreen(string reason = null, string location = null)
    {
        Debug.Log(
            "[GameManager] ShowGameOverScreen is deprecated, use CompleteMission(false) instead"
        );
    }

    [System.Obsolete("Use MissionResultController instead")]
    public void ShowVictoryScreen(string missionName = null)
    {
        Debug.Log(
            "[GameManager] ShowVictoryScreen is deprecated, use CompleteMission(true) instead"
        );
    }

    int CalculateXPEarned(bool victory)
    {
        int xp = 0;

        if (victory)
            xp += 100;

        xp += sessionKills * 50;

        xp += Mathf.FloorToInt(gameTime / 60f) * 10;

        return xp;
    }

    public void AddXP(int amount)
    {
        PlayerXP += amount;

        int newRank = CalculateRank(PlayerXP);
        if (newRank > PlayerRank)
        {
            PlayerRank = newRank;
            Debug.Log($" Rank up! New rank: {GetRankName(PlayerRank)}");
        }

        SavePersistentStats();
    }

    int CalculateRank(int xp)
    {
        return Mathf.Min(20, 1 + (xp / 1000));
    }

    public string GetRankName(int rank)
    {
        return rank switch
        {
            1 => "Second Lieutenant",
            2 => "First Lieutenant",
            3 => "Captain",
            4 => "Major",
            5 => "Lieutenant Colonel",
            6 => "Colonel",
            7 => "Brigadier General",
            8 => "Major General",
            9 => "Lieutenant General",
            10 => "General",
            _ => rank > 10 ? "General of the Air Force" : "Cadet",
        };
    }

    public void PauseGame()
    {
        if (isGameOver)
            return;

        isPaused = true;
        Time.timeScale = 0f;
        OnGamePaused?.Invoke();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        OnGameResumed?.Invoke();
    }

    public void SetCallsign(string callsign)
    {
        if (!string.IsNullOrWhiteSpace(callsign))
        {
            PlayerCallsign = callsign.ToUpper();
            SavePersistentStats();
        }
    }

    public void ResetSessionStats()
    {
        isGameOver = false;
        isPaused = false;
        gameTime = 0f;
        sessionKills = 0;
        sessionDeaths = 0;

        SelectedAircraft = PlayerPrefs.GetString(
            AircraftSelectionApplier.SelectedAircraftKey,
            "F15"
        );
        Debug.Log($"[GameManager] ResetSessionStats: SelectedAircraft = {SelectedAircraft}");

        if (KillTracker.Instance != null)
        {
            KillTracker.Instance.ResetStats();
        }
    }

    public void RestartMission()
    {
        ResetSessionStats();
        Time.timeScale = 1f;
        isGameOver = false;
        isPaused = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGameplaySounds();
            AudioManager.Instance.PlayGameStart();
        }

        Debug.Log("[GameManager] Restarting mission - loading Main scene");
        SceneManager.LoadScene("Main");
    }

    public void ReturnToMainMenu()
    {
        isGameOver = false;
        isPaused = false;
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopGameplaySounds();
        }

        MainMenuController.ShouldStartImmediately = false;

        if (gameTime > 0)
        {
            TotalFlightTime += gameTime;

            switch (SelectedAircraft)
            {
                case "Su27":
                    SU27FlightTime += gameTime;
                    break;
                case "Mig29":
                    MIG29FlightTime += gameTime;
                    break;
                case "fa18e":
                    FA18EFlightTime += gameTime;
                    break;
                case "Hawk_200":
                    Hawk200FlightTime += gameTime;
                    break;
                case "mig21":
                    MIG21FlightTime += gameTime;
                    break;
                case "panavia-tornado":
                    TornadoFlightTime += gameTime;
                    break;
                case "rafalemf3":
                    RafaleFlightTime += gameTime;
                    break;
                case "Typhoon":
                    TyphoonFlightTime += gameTime;
                    break;
                case "F15":
                default:
                    F15FlightTime += gameTime;
                    break;
            }

            SavePersistentStats();
        }

        SceneManager.LoadScene("Menu and story");
    }

    public float GetAccuracy()
    {
        if (TotalMissilesFired == 0)
            return 0f;
        return (float)TotalMissilesHit / TotalMissilesFired * 100f;
    }

    public float GetKDRatio()
    {
        if (TotalDeaths == 0)
            return TotalKills;
        return (float)TotalKills / TotalDeaths;
    }

    public string FormatFlightTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600f);
        int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);

        if (hours > 0)
        {
            return $"{hours}h {minutes:D2}m";
        }
        return $"{minutes}m";
    }

    [ContextMenu("Reset All Stats")]
    public void ResetAllStats()
    {
        TotalKills = 0;
        TotalDeaths = 0;
        TotalWins = 0;
        TotalFlightTime = 0f;
        TotalMissilesFired = 0;
        TotalMissilesHit = 0;
        F15Kills = 0;
        F15FlightTime = 0f;
        F15Sorties = 0;
        SU27Kills = 0;
        SU27FlightTime = 0f;
        SU27Sorties = 0;
        MIG29Kills = 0;
        MIG29FlightTime = 0f;
        MIG29Sorties = 0;
        FA18EKills = 0;
        FA18EFlightTime = 0f;
        FA18ESorties = 0;
        Hawk200Kills = 0;
        Hawk200FlightTime = 0f;
        Hawk200Sorties = 0;
        MIG21Kills = 0;
        MIG21FlightTime = 0f;
        MIG21Sorties = 0;
        TornadoKills = 0;
        TornadoFlightTime = 0f;
        TornadoSorties = 0;
        RafaleKills = 0;
        RafaleFlightTime = 0f;
        RafaleSorties = 0;
        TyphoonKills = 0;
        TyphoonFlightTime = 0f;
        TyphoonSorties = 0;
        PlayerCallsign = "PHOENIX LEAD";
        PlayerRank = 1;
        PlayerXP = 0;
        TotalSorties = 0;
        SelectedAircraft = "F15";

        SavePersistentStats();
        Debug.Log(" All stats reset!");
    }

    #region Save Data Export/Import

    [System.Serializable]
    public class SaveData
    {
        public string version = "1.3";
        public string exportDate;
        public string playerCallsign;

        public int totalKills;
        public int totalDeaths;
        public int totalWins;
        public float totalFlightTime;
        public int totalMissilesFired;
        public int totalMissilesHit;
        public int f15Kills;
        public float f15FlightTime;
        public int f15Sorties;
        public int su27Kills;
        public float su27FlightTime;
        public int su27Sorties;
        public int mig29Kills;
        public float mig29FlightTime;
        public int mig29Sorties;
        public int fa18eKills;
        public float fa18eFlightTime;
        public int fa18eSorties;
        public int hawk200Kills;
        public float hawk200FlightTime;
        public int hawk200Sorties;
        public int mig21Kills;
        public float mig21FlightTime;
        public int mig21Sorties;
        public int tornadoKills;
        public float tornadoFlightTime;
        public int tornadoSorties;
        public int rafaleKills;
        public float rafaleFlightTime;
        public int rafaleSorties;
        public int typhoonKills;
        public float typhoonFlightTime;
        public int typhoonSorties;
        public int playerRank;
        public int playerXP;
        public int totalSorties;
        public string selectedAircraft;
        public string defaultAircraft;

        public bool storyShown;

        public float mouseSensitivity = 1f;
        public float joystickDeadzone = 0.1f;
        public bool invertY = false;
        public bool invertX = false;
        public bool flightAssist = true;
        public bool vibration = true;

        public KeyBindingSave[] keyBindings;
    }

    [System.Serializable]
    public class KeyBindingSave
    {
        public string actionKey;
        public string boundKey;
    }

    public static string GetDefaultSavePath()
    {
        return System.IO.Path.Combine(
            Application.persistentDataPath,
            "AirSuperiority_SaveData.json"
        );
    }

    public string ExportToJson()
    {
        var saveData = new SaveData
        {
            version = "1.3",
            exportDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            playerCallsign = PlayerCallsign,
            totalKills = TotalKills,
            totalDeaths = TotalDeaths,
            totalWins = TotalWins,
            totalFlightTime = TotalFlightTime,
            totalMissilesFired = TotalMissilesFired,
            totalMissilesHit = TotalMissilesHit,
            f15Kills = F15Kills,
            f15FlightTime = F15FlightTime,
            f15Sorties = F15Sorties,
            su27Kills = SU27Kills,
            su27FlightTime = SU27FlightTime,
            su27Sorties = SU27Sorties,
            mig29Kills = MIG29Kills,
            mig29FlightTime = MIG29FlightTime,
            mig29Sorties = MIG29Sorties,
            fa18eKills = FA18EKills,
            fa18eFlightTime = FA18EFlightTime,
            fa18eSorties = FA18ESorties,
            hawk200Kills = Hawk200Kills,
            hawk200FlightTime = Hawk200FlightTime,
            hawk200Sorties = Hawk200Sorties,
            mig21Kills = MIG21Kills,
            mig21FlightTime = MIG21FlightTime,
            mig21Sorties = MIG21Sorties,
            tornadoKills = TornadoKills,
            tornadoFlightTime = TornadoFlightTime,
            tornadoSorties = TornadoSorties,
            rafaleKills = RafaleKills,
            rafaleFlightTime = RafaleFlightTime,
            rafaleSorties = RafaleSorties,
            typhoonKills = TyphoonKills,
            typhoonFlightTime = TyphoonFlightTime,
            typhoonSorties = TyphoonSorties,
            playerRank = PlayerRank,
            playerXP = PlayerXP,
            totalSorties = TotalSorties,
            selectedAircraft = SelectedAircraft,
            defaultAircraft = PlayerPrefs.GetString(HangarController.DefaultAircraftKey, "F15"),
            storyShown = PlayerPrefs.GetInt("StoryIntroShown", 0) == 1,

            mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f),
            joystickDeadzone = PlayerPrefs.GetFloat("JoystickDeadzone", 0.1f),
            invertY = PlayerPrefs.GetInt("InvertY", 0) == 1,
            invertX = PlayerPrefs.GetInt("InvertX", 0) == 1,
            flightAssist = PlayerPrefs.GetInt("AutoStabilize", 1) == 1,
            vibration = PlayerPrefs.GetInt("Vibration", 1) == 1,
        };

        var keyBindingsList = new System.Collections.Generic.List<KeyBindingSave>();
        string[] bindingKeys = new string[]
        {
            "throttle-up",
            "throttle-down",
            "pitch-up",
            "pitch-down",
            "roll-left",
            "roll-right",
            "yaw-left",
            "yaw-right",
            "flaps",
            "fire-missile",
            "fire-cannon",
            "toggle-ai",
        };

        foreach (string key in bindingKeys)
        {
            string boundKey = PlayerPrefs.GetString("KeyBinding_" + key, "");
            if (!string.IsNullOrEmpty(boundKey))
            {
                keyBindingsList.Add(new KeyBindingSave { actionKey = key, boundKey = boundKey });
            }
        }
        saveData.keyBindings = keyBindingsList.ToArray();

        return JsonUtility.ToJson(saveData, true);
    }

    [ContextMenu("Export Save Data")]
    public bool ExportToFile(string filePath = null)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetDefaultSavePath();
            }

            string json = ExportToJson();
            System.IO.File.WriteAllText(filePath, json);

            Debug.Log($" Save data exported to: {filePath}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Failed to export save data: {e.Message}");
            return false;
        }
    }

    public bool ImportFromJson(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError(" Import failed: Empty JSON data");
                return false;
            }

            var saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData == null)
            {
                Debug.LogError(" Import failed: Invalid JSON format");
                return false;
            }

            PlayerCallsign = string.IsNullOrWhiteSpace(saveData.playerCallsign)
                ? "PHOENIX LEAD"
                : saveData.playerCallsign;
            TotalKills = Mathf.Max(0, saveData.totalKills);
            TotalDeaths = Mathf.Max(0, saveData.totalDeaths);
            TotalWins = Mathf.Max(0, saveData.totalWins);
            TotalFlightTime = Mathf.Max(0f, saveData.totalFlightTime);
            TotalMissilesFired = Mathf.Max(0, saveData.totalMissilesFired);
            TotalMissilesHit = Mathf.Max(0, saveData.totalMissilesHit);
            F15Kills = Mathf.Max(0, saveData.f15Kills);
            F15FlightTime = Mathf.Max(0f, saveData.f15FlightTime);
            F15Sorties = Mathf.Max(0, saveData.f15Sorties);
            SU27Kills = Mathf.Max(0, saveData.su27Kills);
            SU27FlightTime = Mathf.Max(0f, saveData.su27FlightTime);
            SU27Sorties = Mathf.Max(0, saveData.su27Sorties);
            MIG29Kills = Mathf.Max(0, saveData.mig29Kills);
            MIG29FlightTime = Mathf.Max(0f, saveData.mig29FlightTime);
            MIG29Sorties = Mathf.Max(0, saveData.mig29Sorties);
            FA18EKills = Mathf.Max(0, saveData.fa18eKills);
            FA18EFlightTime = Mathf.Max(0f, saveData.fa18eFlightTime);
            FA18ESorties = Mathf.Max(0, saveData.fa18eSorties);
            Hawk200Kills = Mathf.Max(0, saveData.hawk200Kills);
            Hawk200FlightTime = Mathf.Max(0f, saveData.hawk200FlightTime);
            Hawk200Sorties = Mathf.Max(0, saveData.hawk200Sorties);
            MIG21Kills = Mathf.Max(0, saveData.mig21Kills);
            MIG21FlightTime = Mathf.Max(0f, saveData.mig21FlightTime);
            MIG21Sorties = Mathf.Max(0, saveData.mig21Sorties);
            TornadoKills = Mathf.Max(0, saveData.tornadoKills);
            TornadoFlightTime = Mathf.Max(0f, saveData.tornadoFlightTime);
            TornadoSorties = Mathf.Max(0, saveData.tornadoSorties);
            RafaleKills = Mathf.Max(0, saveData.rafaleKills);
            RafaleFlightTime = Mathf.Max(0f, saveData.rafaleFlightTime);
            RafaleSorties = Mathf.Max(0, saveData.rafaleSorties);
            TyphoonKills = Mathf.Max(0, saveData.typhoonKills);
            TyphoonFlightTime = Mathf.Max(0f, saveData.typhoonFlightTime);
            TyphoonSorties = Mathf.Max(0, saveData.typhoonSorties);
            PlayerRank = Mathf.Clamp(saveData.playerRank, 1, 20);
            PlayerXP = Mathf.Max(0, saveData.playerXP);
            TotalSorties = Mathf.Max(0, saveData.totalSorties);
            SelectedAircraft = string.IsNullOrWhiteSpace(saveData.selectedAircraft)
                ? "F15"
                : saveData.selectedAircraft;

            string defaultAircraft = string.IsNullOrWhiteSpace(saveData.defaultAircraft)
                ? "F15"
                : saveData.defaultAircraft;
            PlayerPrefs.SetString(HangarController.DefaultAircraftKey, defaultAircraft);

            PlayerPrefs.SetInt("StoryIntroShown", saveData.storyShown ? 1 : 0);

            PlayerPrefs.SetFloat("MouseSensitivity", saveData.mouseSensitivity);
            PlayerPrefs.SetFloat("JoystickDeadzone", saveData.joystickDeadzone);
            PlayerPrefs.SetInt("InvertY", saveData.invertY ? 1 : 0);
            PlayerPrefs.SetInt("InvertX", saveData.invertX ? 1 : 0);
            PlayerPrefs.SetInt("AutoStabilize", saveData.flightAssist ? 1 : 0);
            PlayerPrefs.SetInt("Vibration", saveData.vibration ? 1 : 0);

            if (saveData.keyBindings != null)
            {
                foreach (var binding in saveData.keyBindings)
                {
                    if (
                        !string.IsNullOrEmpty(binding.actionKey)
                        && !string.IsNullOrEmpty(binding.boundKey)
                    )
                    {
                        PlayerPrefs.SetString("KeyBinding_" + binding.actionKey, binding.boundKey);
                    }
                }
            }

            SavePersistentStats();
            PlayerPrefs.Save();

            Debug.Log(
                $" Save data imported successfully! Callsign: {PlayerCallsign}, Kills: {TotalKills}, Rank: {PlayerRank}, Default Aircraft: {defaultAircraft}, All 9 aircraft stats restored"
            );
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Import failed: {e.Message}");
            return false;
        }
    }

    [ContextMenu("Import Save Data")]
    public bool ImportFromFile(string filePath = null)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = GetDefaultSavePath();
            }

            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError($" Import failed: File not found at {filePath}");
                return false;
            }

            string json = System.IO.File.ReadAllText(filePath);
            return ImportFromJson(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Failed to import save data: {e.Message}");
            return false;
        }
    }

    public static bool SaveFileExists()
    {
        return System.IO.File.Exists(GetDefaultSavePath());
    }

    public static bool DeleteSaveFile()
    {
        try
        {
            string path = GetDefaultSavePath();
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
                Debug.Log(" Save file deleted");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($" Failed to delete save file: {e.Message}");
            return false;
        }
    }

    #endregion
}
