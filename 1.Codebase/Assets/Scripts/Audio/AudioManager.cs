using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("=== AUDIO LIBRARY ===")]
    [Tooltip("Assign the AudioLibrary ScriptableObject here for centralized clip management")]
    [SerializeField]
    private AudioLibrary audioLibrary;

    [Header("=== AUDIO SOURCES ===")]
    [Tooltip("Music playback source")]
    [SerializeField]
    private AudioSource musicSource;

    [Tooltip("UI sounds source")]
    [SerializeField]
    private AudioSource uiSource;

    [Tooltip("General SFX source")]
    [SerializeField]
    private AudioSource sfxSource;

    [Tooltip("Engine sounds source (looping)")]
    [SerializeField]
    private AudioSource engineSource;

    [Tooltip("Warning sounds source (looping with fade)")]
    [SerializeField]
    private AudioSource warningSource;

    [Tooltip("Missile incoming warning source (looping with fade)")]
    [SerializeField]
    private AudioSource missileIncomingSource;

    [Tooltip("Voice/Radio source")]
    [SerializeField]
    private AudioSource voiceSource;

    [Tooltip("Environment/Ambient source")]
    [SerializeField]
    private AudioSource ambientSource;

    [Header("=== AUDIO MIXER ===")]
    [Tooltip("Optional AudioMixer for advanced mixing")]
    [SerializeField]
    private AudioMixer audioMixer;

    [Header("=== LEGACY CLIP REFERENCES ===")]
    [Tooltip("For backward compatibility - will be moved to AudioLibrary")]
    [SerializeField]
    private AudioClip menuMusic;

    [SerializeField]
    private AudioClip gameMusic;

    [SerializeField]
    private AudioClip victoryMusic;

    [SerializeField]
    private AudioClip defeatMusic;

    [SerializeField]
    private AudioClip buttonClick;

    [SerializeField]
    private AudioClip buttonHover;

    [SerializeField]
    private AudioClip menuOpen;

    [SerializeField]
    private AudioClip menuClose;

    [SerializeField]
    private AudioClip missilelock;

    [SerializeField]
    private AudioClip missileFire;

    [SerializeField]
    private AudioClip missileIncoming;

    [SerializeField]
    private AudioClip cannonFire;

    [SerializeField]
    private AudioClip explosion;

    [SerializeField]
    private AudioClip hit;

    [SerializeField]
    private AudioClip engineLoop;

    [SerializeField]
    private AudioClip afterburner;

    [SerializeField]
    private AudioClip stall;

    [SerializeField]
    private AudioClip pullUp;

    [SerializeField]
    private AudioClip boundaryWarning;

    [SerializeField]
    private AudioClip lowHealth;

    [SerializeField]
    private AudioClip rainLoop;

    [SerializeField]
    private AudioClip foxTwo;

    [SerializeField]
    private AudioClip splash;

    [SerializeField]
    private AudioClip enemyDown;

    [SerializeField]
    private AudioClip rtb;

    [Header("=== ADDITIONAL CLIPS ===")]
    [SerializeField]
    private AudioClip flareSound;

    [SerializeField]
    private AudioClip chaffSound;

    [SerializeField]
    private AudioClip breathingHeavy;

    [SerializeField]
    private AudioClip heartbeat;

    [SerializeField]
    private AudioClip glocSound;

    [SerializeField]
    private AudioClip fireLoop;

    [SerializeField]
    private AudioClip warningBeep;

    [SerializeField]
    private AudioClip killConfirm;

    [Header("=== KILL STREAK CALLOUTS ===")]
    [SerializeField]
    private AudioClip doubleKill;

    [SerializeField]
    private AudioClip tripleKill;

    [SerializeField]
    private AudioClip enemyEliminated;

    [Header("=== TACTICAL CALLOUTS ===")]
    [SerializeField]
    private AudioClip underAttack;

    [SerializeField]
    private AudioClip weHit;

    [SerializeField]
    private AudioClip mayday;

    [SerializeField]
    private AudioClip watchYourSix;

    [SerializeField]
    private AudioClip targetLocked;

    [SerializeField]
    private AudioClip airSupport;

    [SerializeField]
    private AudioClip goodWork;

    [Header("=== MISSION STATUS ===")]
    [SerializeField]
    private AudioClip gameStart;

    [SerializeField]
    private AudioClip gameOver;

    [SerializeField]
    private AudioClip missionAccomplished;

    [SerializeField]
    private AudioClip objectiveSecured;

    [Header("=== RADIO EFFECTS ===")]
    [SerializeField]
    private AudioClip radioCheck;

    [SerializeField]
    private AudioClip radioStatic;

    [Header("=== ALTERNATE SOUNDS ===")]
    [SerializeField]
    private AudioClip explosion2;

    [SerializeField]
    private AudioClip minigun;

    [SerializeField]
    private AudioClip loadingSound1;

    [SerializeField]
    private AudioClip loadingSound2;

    [Header("=== STARTUP LOGO SOUNDS ===")]
    [Tooltip("Sound effects for each startup logo (played in order)")]
    [SerializeField]
    private AudioClip[] startupLogoSounds;

    [Header("=== SETTINGS ===")]
    [SerializeField]
    private float fadeDuration = 1f;
#pragma warning disable 0414
    [SerializeField]
    private float defaultMusicVolume = 0.5f;
#pragma warning restore 0414
#pragma warning disable 0414
    [SerializeField]
    private float defaultSFXVolume = 1f;
#pragma warning restore 0414

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;
    private float engineVolume = 0.8f;
    private float voiceVolume = 1f;
    private bool radioChatterEnabled = true;
    private bool isMuted = false;

    private Dictionary<string, AudioClip> soundLibrary = new Dictionary<string, AudioClip>();
    private Coroutine warningFadeCoroutine;
    private Coroutine missileIncomingFadeCoroutine;
    private Coroutine musicFadeCoroutine;
    private AudioClip currentWarningClip;
    private AudioClip currentMissileIncomingClip;

    private Dictionary<string, AudioSource> loopingSources = new Dictionary<string, AudioSource>();

    public enum WarningPriority
    {
        None = 0,
        GForce = 1,
        LowHealth = 2,
        Stall = 3,
        MissileIncoming = 4,
    }

    private WarningPriority currentWarningPriority = WarningPriority.None;
    private Dictionary<WarningPriority, bool> activeWarnings = new Dictionary<WarningPriority, bool>
    {
        { WarningPriority.GForce, false },
        { WarningPriority.LowHealth, false },
        { WarningPriority.Stall, false },
        { WarningPriority.MissileIncoming, false },
    };

    private readonly HashSet<string> warnedMissingVoiceClips = new HashSet<string>();

    private float lastUnderAttackTime = -999f;
    private float lastTargetLockedTime = -999f;
    private const float VOICE_COOLDOWN = 2.0f;

    private bool isGameAudioPaused = false;
    private bool wasEnginePlaying = false;
    private bool wasCannonPlaying = false;
    private bool wasMissileLockPlaying = false;
    private bool wasWarningPlaying = false;
    private bool wasMissileIncomingPlaying = false;
    private bool wasAmbientPlaying = false;
    private bool wasGLocPlaying = false;

    #region Unity Lifecycle

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundLibrary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        CreateAudioSourcesIfNeeded();
        LoadVolumeSettings();
        ApplyVolumes();

        Debug.Log(
            $"[AudioManager] Started - AudioLibrary={(audioLibrary != null ? "Assigned" : "NULL")}, "
                + $"SoundLibrary={soundLibrary.Count} clips, "
                + $"Master={masterVolume:F2}, Music={musicVolume:F2}, SFX={sfxVolume:F2}, Voice={voiceVolume:F2}, "
                + $"RadioChatter={radioChatterEnabled}, Muted={isMuted}"
        );
        Debug.Log(
            $"[AudioManager] AudioSources - Music={musicSource != null}, UI={uiSource != null}, "
                + $"SFX={sfxSource != null}, Engine={engineSource != null}, Voice={voiceSource != null}"
        );
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Initialization

    private void CreateAudioSourcesIfNeeded()
    {
        if (musicSource == null)
        {
            musicSource = CreateAudioSource("Music", true, 0f);
            musicSource.ignoreListenerPause = true;
        }
        if (uiSource == null)
        {
            uiSource = CreateAudioSource("UI", false, 0f);
            uiSource.ignoreListenerPause = true;
        }
        if (sfxSource == null)
            sfxSource = CreateAudioSource("SFX", false, 0f);
        if (engineSource == null)
            engineSource = CreateAudioSource("Engine", true, 0.5f);
        if (warningSource == null)
        {
            warningSource = CreateAudioSource("Warning", true, 0f);
            warningSource.ignoreListenerPause = true;
        }
        if (missileIncomingSource == null)
        {
            missileIncomingSource = CreateAudioSource("MissileIncoming", true, 0f);
            missileIncomingSource.ignoreListenerPause = true;
        }
        if (voiceSource == null)
        {
            voiceSource = CreateAudioSource("Voice", false, 0f);
            voiceSource.ignoreListenerPause = true;
        }
        if (ambientSource == null)
            ambientSource = CreateAudioSource("Ambient", true, 0f);
    }

    private AudioSource CreateAudioSource(string name, bool loop, float spatialBlend)
    {
        GameObject go = new GameObject($"AudioSource_{name}");
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.loop = loop;
        source.playOnAwake = false;
        source.spatialBlend = spatialBlend;
        return source;
    }

    private void InitializeSoundLibrary()
    {
        soundLibrary.Clear();

        if (audioLibrary != null)
        {
            Debug.Log(
                $"[AudioManager] InitializeSoundLibrary - AudioLibrary assigned, loading clips..."
            );
            AddToLibrary("menu_music", audioLibrary.menuMusic);
            AddToLibrary("game_music", audioLibrary.gameMusic);
            AddToLibrary("victory_music", audioLibrary.victoryMusic);
            AddToLibrary("defeat_music", audioLibrary.defeatMusic);
            AddToLibrary("button_click", audioLibrary.buttonClick);
            AddToLibrary("button_hover", audioLibrary.buttonHover);
            AddToLibrary("menu_open", audioLibrary.menuOpen);
            AddToLibrary("menu_close", audioLibrary.menuClose);
            AddToLibrary("missile_lock", audioLibrary.missileLock);
            AddToLibrary("missile_fire", audioLibrary.missileFire);
            AddToLibrary("missile_incoming", audioLibrary.missileIncoming);
            AddToLibrary("cannon_fire", audioLibrary.cannonFire);
            AddToLibrary("explosion", audioLibrary.explosion);
            AddToLibrary("hit", audioLibrary.hit);
            AddToLibrary("engine_loop", audioLibrary.engineLoop);
            AddToLibrary("afterburner", audioLibrary.afterburner);
            AddToLibrary("stall", audioLibrary.stallWarning);
            AddToLibrary("pull_up", audioLibrary.pullUpWarning);
            AddToLibrary("boundary_warning", audioLibrary.boundaryWarning);
            AddToLibrary("low_health", audioLibrary.lowHealthWarning);
            AddToLibrary("rain_loop", audioLibrary.rainLoop);
            AddToLibrary("fox_two", audioLibrary.foxTwo);
            AddToLibrary("splash", audioLibrary.splash);
            AddToLibrary("enemy_down", audioLibrary.enemyDown);
            AddToLibrary("rtb", audioLibrary.rtb);
            AddToLibrary("flare", audioLibrary.flareSound);
            AddToLibrary("chaff", audioLibrary.chaffSound);
            AddToLibrary("breathing_heavy", audioLibrary.breathingHeavy);
            AddToLibrary("heartbeat", audioLibrary.heartbeat);
            AddToLibrary("gloc", audioLibrary.glocSound);
            AddToLibrary("fire_loop", audioLibrary.fireLoop);
            AddToLibrary("warning_beep", audioLibrary.warningBeep);
            AddToLibrary("kill_confirm", audioLibrary.killConfirm);
            AddToLibrary("double_kill", audioLibrary.doubleKill);
            AddToLibrary("triple_kill", audioLibrary.tripleKill);
            AddToLibrary("enemy_eliminated", audioLibrary.enemyEliminated);
            AddToLibrary("under_attack", audioLibrary.underAttack);
            AddToLibrary("we_hit", audioLibrary.weHit);
            AddToLibrary("mayday", audioLibrary.mayday);
            AddToLibrary("watch_your_six", audioLibrary.watchYourSix);
            AddToLibrary("target_locked", audioLibrary.targetLocked);
            AddToLibrary("air_support", audioLibrary.airSupport);
            AddToLibrary("good_work", audioLibrary.goodWork);
            AddToLibrary("game_start", audioLibrary.gameStart);
            AddToLibrary("game_over", audioLibrary.gameOver);
            AddToLibrary("mission_accomplished", audioLibrary.missionAccomplished);
            AddToLibrary("objective_secured", audioLibrary.objectiveSecured);
            AddToLibrary("radio_check", audioLibrary.radioCheck);
            AddToLibrary("radio_static", audioLibrary.radioStatic);
            AddToLibrary("explosion2", audioLibrary.explosion2);
            AddToLibrary("minigun", audioLibrary.minigun);
            AddToLibrary("loading_sound1", audioLibrary.loadingSound1);
            AddToLibrary("loading_sound2", audioLibrary.loadingSound2);
            Debug.Log($"[AudioManager] Loaded {soundLibrary.Count} clips from AudioLibrary");
        }
        else
        {
            Debug.LogWarning("[AudioManager] InitializeSoundLibrary - AudioLibrary is NULL!");
        }

        AddToLibrary("menu_music", menuMusic);
        AddToLibrary("game_music", gameMusic);
        AddToLibrary("victory_music", victoryMusic);
        AddToLibrary("defeat_music", defeatMusic);
        AddToLibrary("button_click", buttonClick);
        AddToLibrary("button_hover", buttonHover);
        AddToLibrary("menu_open", menuOpen);
        AddToLibrary("menu_close", menuClose);
        AddToLibrary("missile_lock", missilelock);
        AddToLibrary("missile_fire", missileFire);
        AddToLibrary("missile_incoming", missileIncoming);
        AddToLibrary("cannon_fire", cannonFire);
        AddToLibrary("explosion", explosion);
        AddToLibrary("hit", hit);
        AddToLibrary("engine_loop", engineLoop);
        AddToLibrary("afterburner", afterburner);
        AddToLibrary("stall", stall);
        AddToLibrary("pull_up", pullUp);
        AddToLibrary("boundary_warning", boundaryWarning);
        AddToLibrary("low_health", lowHealth);
        AddToLibrary("rain_loop", rainLoop);
        AddToLibrary("fox_two", foxTwo);
        AddToLibrary("splash", splash);
        AddToLibrary("enemy_down", enemyDown);
        AddToLibrary("rtb", rtb);
        AddToLibrary("flare", flareSound);
        AddToLibrary("chaff", chaffSound);
        AddToLibrary("breathing_heavy", breathingHeavy);
        AddToLibrary("heartbeat", heartbeat);
        AddToLibrary("gloc", glocSound);
        AddToLibrary("fire_loop", fireLoop);
        AddToLibrary("warning_beep", warningBeep);
        AddToLibrary("kill_confirm", killConfirm);
        AddToLibrary("double_kill", doubleKill);
        AddToLibrary("triple_kill", tripleKill);
        AddToLibrary("enemy_eliminated", enemyEliminated);
        AddToLibrary("under_attack", underAttack);
        AddToLibrary("we_hit", weHit);
        AddToLibrary("mayday", mayday);
        AddToLibrary("watch_your_six", watchYourSix);
        AddToLibrary("target_locked", targetLocked);
        AddToLibrary("air_support", airSupport);
        AddToLibrary("good_work", goodWork);
        AddToLibrary("game_start", gameStart);
        AddToLibrary("game_over", gameOver);
        AddToLibrary("mission_accomplished", missionAccomplished);
        AddToLibrary("objective_secured", objectiveSecured);
        AddToLibrary("radio_check", radioCheck);
        AddToLibrary("radio_static", radioStatic);
        AddToLibrary("explosion2", explosion2);
        AddToLibrary("minigun", minigun);
        AddToLibrary("loading_sound1", loadingSound1);
        AddToLibrary("loading_sound2", loadingSound2);
    }

    private void AddToLibrary(string key, AudioClip clip)
    {
        if (clip != null && !soundLibrary.ContainsKey(key))
        {
            soundLibrary[key] = clip;
        }
    }

    #endregion

    #region Volume Control

    private void LoadVolumeSettings()
    {
        masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVolume", 1f));
        musicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume", 1f));
        sfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("SFXVolume", 1f));
        engineVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("EngineVolume", 0.8f));
        voiceVolume = Mathf.Clamp01(PlayerPrefs.GetFloat("VoiceVolume", 1f));
        radioChatterEnabled = PlayerPrefs.GetInt("RadioChatter", 1) == 1;
        isMuted = PlayerPrefs.GetInt("Mute", 0) == 1;

        float rawMaster = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (rawMaster > 1f)
        {
            Debug.LogWarning(
                $"[AudioManager] Detected old PlayerPrefs format (value={rawMaster}). Resetting to 0.9f"
            );
            masterVolume = 0.9f;
            musicVolume = 0.9f;
            sfxVolume = 0.9f;
            engineVolume = 0.8f;
            voiceVolume = 0.9f;

            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("EngineVolume", engineVolume);
            PlayerPrefs.SetFloat("VoiceVolume", voiceVolume);
            PlayerPrefs.Save();
        }
    }

    public void UpdateVolumes(
        float master,
        float music,
        float sfx,
        float engine,
        float voice,
        bool radioChatter
    )
    {
        masterVolume = Mathf.Clamp01(master);
        musicVolume = Mathf.Clamp01(music);
        sfxVolume = Mathf.Clamp01(sfx);
        engineVolume = Mathf.Clamp01(engine);
        voiceVolume = Mathf.Clamp01(voice);
        radioChatterEnabled = radioChatter;

        ApplyVolumes();

        Debug.Log(
            $"[AudioManager] Volumes updated - Master={masterVolume:F2}, Music={musicVolume:F2}, SFX={sfxVolume:F2}, Engine={engineVolume:F2}, Voice={voiceVolume:F2}, RadioChatter={radioChatterEnabled}"
        );
    }

    public void SetMuted(bool muted)
    {
        isMuted = muted;
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        float master = isMuted ? 0f : masterVolume;

        if (musicSource != null)
            musicSource.volume = master * musicVolume;
        if (uiSource != null)
            uiSource.volume = master * sfxVolume;
        if (sfxSource != null)
            sfxSource.volume = master * sfxVolume;
        if (engineSource != null)
            engineSource.volume = master * engineVolume;
        if (warningSource != null)
            warningSource.volume = master * sfxVolume;
        if (missileIncomingSource != null)
            missileIncomingSource.volume = master * sfxVolume;
        if (voiceSource != null)
            voiceSource.volume = master * voiceVolume * (radioChatterEnabled ? 1f : 0f);
        if (ambientSource != null)
            ambientSource.volume = master * sfxVolume;

        if (audioMixer != null)
        {
            SetMixerVolume("MasterVolume", master);
            SetMixerVolume("MusicVolume", musicVolume);
            SetMixerVolume("SFXVolume", sfxVolume);
            SetMixerVolume("EngineVolume", engineVolume);
            SetMixerVolume("VoiceVolume", radioChatterEnabled ? voiceVolume : 0f);
        }
    }

    private void SetMixerVolume(string parameterName, float linear01)
    {
        if (audioMixer == null)
            return;
        var clamped = Mathf.Clamp01(linear01);
        var db = clamped <= 0.0001f ? -80f : Mathf.Log10(clamped) * 20f;
        audioMixer.SetFloat(parameterName, db);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = (isMuted ? 0f : masterVolume) * musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        ApplyVolumes();
    }

    public void SetUIVolume(float volume)
    {
        if (uiSource != null)
            uiSource.volume = (isMuted ? 0f : masterVolume) * Mathf.Clamp01(volume);
    }

    public void SetEngineVolume(float volume)
    {
        engineVolume = Mathf.Clamp01(volume);
        if (engineSource != null)
            engineSource.volume = (isMuted ? 0f : masterVolume) * engineVolume;
    }

    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        if (voiceSource != null)
            voiceSource.volume =
                (isMuted ? 0f : masterVolume) * voiceVolume * (radioChatterEnabled ? 1f : 0f);
    }

    #endregion

    #region Music

    public void PlayMenuMusic()
    {
        AudioClip clip = GetClip("menu_music") ?? menuMusic;
        if (clip != null)
        {
            PlayMusic(clip);
            Debug.Log($"[AudioManager] Playing menu music: {clip.name}");
        }
        else
        {
            Debug.LogWarning("[AudioManager] Cannot play menu music: clip is NULL");
        }
    }

    public void PlayGameMusic()
    {
        PlayMusic(GetClip("game_music") ?? gameMusic);
    }

    public void PlayVictoryMusic()
    {
        AudioClip clip = GetClip("victory_music") ?? victoryMusic;
        if (clip != null && musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = false;
            musicSource.volume = masterVolume * musicVolume;
            musicSource.Play();
            Debug.Log($"[AudioManager] Playing victory music: {clip.name}");
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] Cannot play victory music: clip={(clip != null ? clip.name : "NULL")}, musicSource={(musicSource != null ? "OK" : "NULL")}"
            );
        }
    }

    public void PlayDefeatMusic()
    {
        AudioClip clip = GetClip("defeat_music") ?? defeatMusic;
        if (clip != null && musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = false;
            musicSource.volume = masterVolume * musicVolume;
            musicSource.Play();
            Debug.Log($"[AudioManager] Playing defeat music: {clip.name}");
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] Cannot play defeat music: clip={(clip != null ? clip.name : "NULL")}, musicSource={(musicSource != null ? "OK" : "NULL")}"
            );
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();
    }

    public void FadeToMusic(AudioClip newClip, float duration = 1f)
    {
        if (musicFadeCoroutine != null)
            StopCoroutine(musicFadeCoroutine);
        musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(newClip, duration));
    }

    private IEnumerator FadeMusicCoroutine(AudioClip newClip, float duration)
    {
        if (musicSource == null)
            yield break;

        float startVolume = musicSource.volume;
        float halfDuration = duration / 2f;

        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / halfDuration);
            yield return null;
        }

        musicSource.clip = newClip;
        if (newClip != null)
        {
            musicSource.Play();

            t = 0f;
            float targetVolume = (isMuted ? 0f : masterVolume) * musicVolume;
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, t / halfDuration);
                yield return null;
            }
            musicSource.volume = targetVolume;
        }
    }

    #endregion

    #region UI Sounds

    public void PlayButtonClick()
    {
        Debug.Log("[AudioManager] PlayButtonClick called");
        PlayUI(GetClip("button_click") ?? buttonClick);
    }

    public void PlayButtonHover()
    {
        Debug.Log("[AudioManager] PlayButtonHover called");
        PlayUI(GetClip("button_hover") ?? buttonHover);
    }

    public void PlayMenuOpen()
    {
        Debug.Log("[AudioManager] PlayMenuOpen called");
        PlayUI(GetClip("menu_open") ?? menuOpen);
    }

    public void PlayMenuClose()
    {
        Debug.Log("[AudioManager] PlayMenuClose called");
        PlayUI(GetClip("menu_close") ?? menuClose);
    }

    public void PlayUI(AudioClip clip)
    {
        if (clip != null && uiSource != null)
        {
            float effectiveVolume = (isMuted ? 0f : masterVolume) * sfxVolume;
            Debug.Log(
                $"[AudioManager] PlayUI: Playing '{clip.name}' at volume {effectiveVolume:F2}"
            );
            uiSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] PlayUI: clip={(clip != null ? clip.name : "NULL")}, uiSource={(uiSource != null ? "OK" : "NULL")}"
            );
        }
    }

    #endregion

    #region SFX (Weapons, Explosions, etc.)

    private AudioSource missileLockSource;
    private bool isMissileLockPlaying = false;

    public void PlayMissileLock()
    {
        if (isMissileLockPlaying)
            return;

        AudioClip clip = GetClip("missile_lock") ?? missilelock;
        if (clip == null)
        {
            Debug.LogWarning(
                "[AudioManager] PlayMissileLock: No clip found! Check AudioLibrary.missileLock or AudioManager.missilelock assignment."
            );
            return;
        }

        if (missileLockSource == null)
        {
            missileLockSource = CreateAudioSource("MissileLock", true, 0f);
        }

        missileLockSource.clip = clip;
        missileLockSource.loop = true;
        missileLockSource.volume = (isMuted ? 0f : masterVolume) * sfxVolume;
        missileLockSource.Play();
        isMissileLockPlaying = true;

        Debug.Log($"[AudioManager] PlayMissileLock: Started loop '{clip.name}'");
    }

    public void StopMissileLock()
    {
        if (!isMissileLockPlaying)
            return;

        if (missileLockSource != null && missileLockSource.isPlaying)
        {
            missileLockSource.Stop();
        }
        isMissileLockPlaying = false;

        Debug.Log("[AudioManager] StopMissileLock: Lock warning stopped");
    }

    public void PlayMissileFire()
    {
        PlaySFX(GetClip("missile_fire") ?? missileFire);
        if (radioChatterEnabled)
        {
            PlayVoice(GetClip("fox_two") ?? foxTwo);
        }
    }

    public void PlayMissileIncoming()
    {
        AudioClip clip = GetClip("missile_incoming") ?? missileIncoming;
        Debug.Log(
            $"[AudioManager] PlayMissileIncoming called - clip={(clip != null ? clip.name : "NULL")}, missileIncomingSource={(missileIncomingSource != null ? "OK" : "NULL")}"
        );
        Debug.Log(
            $"[AudioManager] PlayMissileIncoming state - muted={isMuted}, master={masterVolume:F2}, sfx={sfxVolume:F2}"
        );
        if (clip == null)
        {
            Debug.LogWarning(
                "[AudioManager] PlayMissileIncoming: No clip found! Check AudioLibrary.missileIncoming or AudioManager.missileIncoming assignment."
            );
            return;
        }

        StartMissileIncomingInternal(clip);
    }

    public void StopMissileIncoming()
    {
        StopMissileIncomingInternal();
    }

    private void StartMissileIncomingInternal(AudioClip clip)
    {
        if (missileIncomingSource == null)
        {
            missileIncomingSource = CreateAudioSource("MissileIncoming", true, 0f);
            missileIncomingSource.ignoreListenerPause = true;
        }

        if (currentMissileIncomingClip == clip && missileIncomingSource.isPlaying)
        {
            Debug.Log(
                $"[AudioManager] StartMissileIncomingInternal: '{clip.name}' already playing, skipping"
            );
            return;
        }

        if (missileIncomingFadeCoroutine != null)
        {
            StopCoroutine(missileIncomingFadeCoroutine);
            missileIncomingFadeCoroutine = null;
        }

        currentMissileIncomingClip = clip;
        missileIncomingSource.clip = clip;
        missileIncomingSource.loop = true;

        float targetVolume = (isMuted ? 0f : masterVolume) * sfxVolume;
        if (targetVolume <= 0.001f)
        {
            Debug.LogWarning(
                $"[AudioManager] StartMissileIncomingInternal: target volume is near zero (muted={isMuted}, master={masterVolume:F2}, sfx={sfxVolume:F2})"
            );
        }
        missileIncomingSource.volume = targetVolume * 0.5f;
        missileIncomingSource.Play();

        Debug.Log(
            $"[AudioManager] StartMissileIncomingInternal: Playing '{clip.name}' at volume {missileIncomingSource.volume}, target={targetVolume}"
        );

        missileIncomingFadeCoroutine = StartCoroutine(
            FadeAudio(missileIncomingSource, targetVolume, fadeDuration)
        );
    }

    private void StopMissileIncomingInternal()
    {
        if (missileIncomingSource == null)
            return;

        Debug.Log(
            $"[AudioManager] StopMissileIncomingInternal: playing={missileIncomingSource.isPlaying}, fadeDuration={fadeDuration:F2}"
        );
        currentMissileIncomingClip = null;

        if (missileIncomingFadeCoroutine != null)
        {
            StopCoroutine(missileIncomingFadeCoroutine);
            missileIncomingFadeCoroutine = null;
        }

        if (missileIncomingSource.isPlaying && fadeDuration > 0f)
        {
            missileIncomingFadeCoroutine = StartCoroutine(
                FadeOutAndStop(missileIncomingSource, fadeDuration)
            );
        }
        else
        {
            missileIncomingSource.Stop();
            missileIncomingSource.volume = 1f;
        }
    }

    private AudioSource cannonSource;
    private bool isCannonFiring = false;

    public void PlayCannonFire()
    {
        PlaySFX(GetClip("cannon_fire") ?? cannonFire);
    }

    public void StartCannonLoop()
    {
        if (isCannonFiring)
            return;

        AudioClip clip = GetClip("cannon_fire") ?? cannonFire;
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] StartCannonLoop: No cannon clip found!");
            return;
        }

        if (cannonSource == null)
        {
            cannonSource = CreateAudioSource("Cannon", true, 0f);
        }

        cannonSource.clip = clip;
        cannonSource.loop = true;
        cannonSource.volume = (isMuted ? 0f : masterVolume) * sfxVolume;
        cannonSource.Play();
        isCannonFiring = true;

        Debug.Log("[AudioManager] StartCannonLoop: Cannon fire loop started");
    }

    public void StopCannonLoop()
    {
        if (!isCannonFiring)
            return;

        if (cannonSource != null && cannonSource.isPlaying)
        {
            cannonSource.Stop();
        }
        isCannonFiring = false;

        Debug.Log("[AudioManager] StopCannonLoop: Cannon fire loop stopped");
    }

    public void PlayExplosion()
    {
        PlaySFX(GetClip("explosion") ?? explosion);
    }

    public void PlayExplosionAtPosition(Vector3 position)
    {
        AudioClip clip;
        if (Random.value > 0.7f && (GetClip("explosion2") ?? explosion2) != null)
        {
            clip = GetClip("explosion2") ?? explosion2;
        }
        else
        {
            clip = GetClip("explosion") ?? explosion;
        }

        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, (isMuted ? 0f : masterVolume) * sfxVolume);
        }
    }

    public void PlayHit()
    {
        PlaySFX(GetClip("hit") ?? hit);
    }

    public void PlayFlare()
    {
        PlaySFX(GetClip("flare") ?? flareSound);
    }

    public void PlayChaff()
    {
        PlaySFX(GetClip("chaff") ?? chaffSound);
    }

    public void PlayKillConfirm()
    {
        PlaySFX(GetClip("kill_confirm") ?? killConfirm);
    }

    public void PlayDoubleKill()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("double_kill") ?? doubleKill);
    }

    public void PlayTripleKill()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("triple_kill") ?? tripleKill);
    }

    public void PlayEnemyEliminated()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("enemy_eliminated") ?? enemyEliminated);
    }

    public void PlayExplosion2()
    {
        PlaySFX(GetClip("explosion2") ?? explosion2);
    }

    public void PlayMinigun()
    {
        PlaySFX(GetClip("minigun") ?? minigun);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX called with null clip");
            return;
        }
        if (sfxSource == null)
        {
            Debug.LogWarning("[AudioManager] PlaySFX: sfxSource is null");
            return;
        }
        float effectiveVolume = (isMuted ? 0f : masterVolume) * sfxVolume;
        if (effectiveVolume <= 0.001f)
        {
            Debug.LogWarning(
                $"[AudioManager] PlaySFX '{clip.name}': Volume is effectively zero (master={masterVolume}, sfx={sfxVolume}, muted={isMuted})"
            );
        }
        Debug.Log($"[AudioManager] PlaySFX: Playing '{clip.name}' at volume {effectiveVolume:F2}");
        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            float effectiveVolume = (isMuted ? 0f : masterVolume) * sfxVolume;
            Debug.Log(
                $"[AudioManager] PlaySFXAtPosition: Playing '{clip.name}' at {position}, volume {effectiveVolume:F2}"
            );
            AudioSource.PlayClipAtPoint(clip, position, effectiveVolume);
        }
    }

    #endregion

    #region Engine Sounds

    public void PlayEngineLoop()
    {
        AudioClip clip = GetClip("engine_loop") ?? engineLoop;
        if (clip != null && engineSource != null)
        {
            engineSource.clip = clip;
            engineSource.loop = true;
            engineSource.Play();
        }
    }

    public void StopEngineLoop()
    {
        if (engineSource != null)
            engineSource.Stop();
    }

    public void PlayAfterburner()
    {
        AudioClip clip = GetClip("afterburner") ?? afterburner;
        if (clip != null && engineSource != null)
        {
            engineSource.clip = clip;
            engineSource.loop = true;
            if (!engineSource.isPlaying)
                engineSource.Play();
        }
    }

    public void SetEnginePitch(float pitch)
    {
        if (engineSource != null)
            engineSource.pitch = Mathf.Clamp(pitch, 0.5f, 2f);
    }

    public void UpdateAfterburnerIntensity(float intensity)
    {
        if (engineSource != null)
        {
            float targetVolume = (isMuted ? 0f : masterVolume) * engineVolume * intensity;
            engineSource.volume = targetVolume;

            if (intensity <= 0.01f && engineSource.isPlaying)
            {
                engineSource.Stop();
            }
        }
    }

    public void StartAfterburnerLoop(AudioClip clip = null)
    {
        AudioClip afterburnerClip = clip ?? GetClip("afterburner") ?? afterburner;
        if (afterburnerClip != null && engineSource != null)
        {
            if (engineSource.clip != afterburnerClip)
            {
                engineSource.clip = afterburnerClip;
            }
            engineSource.loop = true;
            if (!engineSource.isPlaying)
                engineSource.Play();
        }
    }

    public void StopAfterburnerLoop() { }

    #endregion

    #region Warning Sounds

    public void PlayStallWarning()
    {
        AudioClip clip = GetClip("stall") ?? stall;
        Debug.Log(
            $"[AudioManager] PlayStallWarning called - clip={(clip != null ? clip.name : "NULL")}, warningSource={(warningSource != null ? "OK" : "NULL")}"
        );
        if (clip == null)
        {
            Debug.LogWarning(
                "[AudioManager] PlayStallWarning: No clip found! Check AudioLibrary.stallWarning or AudioManager.stall assignment."
            );
        }
        StartWarningWithPriority(clip, WarningPriority.Stall);
    }

    public void StopStallWarning()
    {
        StopWarningWithPriority(WarningPriority.Stall);
    }

    public void PlayPullUpWarning()
    {
        StartWarningWithPriority(GetClip("pull_up") ?? pullUp, WarningPriority.Stall);
    }

    public void PlayBoundaryWarning()
    {
        StartWarningWithPriority(
            GetClip("boundary_warning") ?? boundaryWarning,
            WarningPriority.Stall
        );
    }

    public void StopBoundaryWarning()
    {
        AudioClip clip = GetClip("boundary_warning") ?? boundaryWarning;
        if (clip == null)
            return;

        if (currentWarningClip == clip)
        {
            StopWarningWithPriority(WarningPriority.Stall);
        }
    }

    public void PlayLowHealthWarning()
    {
        StartWarningWithPriority(GetClip("low_health") ?? lowHealth, WarningPriority.LowHealth);
    }

    public void StopLowHealthWarning()
    {
        StopWarningWithPriority(WarningPriority.LowHealth);
    }

    public void PlayFireAlarm()
    {
        AudioClip clip = GetClip("fire_loop") ?? fireLoop;
        StartWarningWithPriority(clip, WarningPriority.LowHealth);
    }

    public void PlayGForceWarning()
    {
        AudioClip clip = GetClip("breathing_heavy") ?? breathingHeavy;
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayGForceWarning: No breathing_heavy clip found!");
            return;
        }

        StartGForceBreathingInternal(clip);
    }

    public void StopGForceWarning()
    {
        StopGForceBreathingInternal();
    }

    private bool isGForceBreathingActive = false;

    private void StartGForceBreathingInternal(AudioClip clip)
    {
        if (glocSource == null)
        {
            glocSource = CreateAudioSource("GLoc", true, 0f);
            glocSource.ignoreListenerPause = true;
        }

        if (isGForceBreathingActive && glocSource.isPlaying && glocSource.clip == clip)
        {
            return;
        }

        glocSource.clip = clip;
        glocSource.loop = true;
        glocSource.volume = (isMuted ? 0f : masterVolume) * sfxVolume;
        glocSource.Play();
        isGForceBreathingActive = true;

        Debug.Log(
            $"[AudioManager] StartGForceBreathingInternal: Playing '{clip.name}' on dedicated channel"
        );
    }

    private void StopGForceBreathingInternal()
    {
        if (!isGForceBreathingActive)
            return;

        if (glocSource != null && glocSource.isPlaying && !isGLocActive)
        {
            glocSource.Stop();
        }
        isGForceBreathingActive = false;

        Debug.Log("[AudioManager] StopGForceBreathingInternal: G-Force breathing stopped");
    }

    private void StartWarningWithPriority(AudioClip clip, WarningPriority priority)
    {
        if (clip == null)
        {
            Debug.LogError(
                $"[AudioManager] StartWarningWithPriority: clip is NULL for priority {priority}!"
            );
            return;
        }

        activeWarnings[priority] = true;

        if (priority < currentWarningPriority)
        {
            Debug.Log(
                $"[AudioManager] Warning '{clip.name}' (priority={priority}) blocked by higher priority {currentWarningPriority}"
            );
            return;
        }

        Debug.Log(
            $"[AudioManager] StartWarningWithPriority: Playing '{clip.name}' with priority {priority} (current={currentWarningPriority})"
        );

        StartWarningInternal(clip, priority);
    }

    private void StopWarningWithPriority(WarningPriority priority)
    {
        bool wasActive = activeWarnings.TryGetValue(priority, out bool active) && active;

        if (!wasActive && currentWarningPriority != priority)
        {
            activeWarnings[priority] = false;
            return;
        }

        activeWarnings[priority] = false;

        if (priority == currentWarningPriority)
        {
            Debug.Log(
                $"[AudioManager] StopWarningWithPriority: Stopping priority {priority}, current={currentWarningPriority}"
            );
        }
        else if (wasActive)
        {
            Debug.Log(
                $"[AudioManager] StopWarningWithPriority: Deactivated priority {priority}, current={currentWarningPriority}"
            );
        }

        if (priority == currentWarningPriority)
        {
            WarningPriority nextHighest = GetHighestActiveWarningPriority();
            if (nextHighest != WarningPriority.None)
            {
                AudioClip nextClip = GetClipForPriority(nextHighest);
                if (nextClip != null)
                {
                    Debug.Log(
                        $"[AudioManager] Switching to next highest warning: {nextHighest} with clip '{nextClip.name}'"
                    );
                    StartWarningInternal(nextClip, nextHighest);
                    return;
                }
            }

            StopWarningInternal();
        }
    }

    private WarningPriority GetHighestActiveWarningPriority()
    {
        if (activeWarnings[WarningPriority.MissileIncoming])
            return WarningPriority.MissileIncoming;
        if (activeWarnings[WarningPriority.Stall])
            return WarningPriority.Stall;
        if (activeWarnings[WarningPriority.LowHealth])
            return WarningPriority.LowHealth;
        return WarningPriority.None;
    }

    private AudioClip GetClipForPriority(WarningPriority priority)
    {
        return priority switch
        {
            WarningPriority.MissileIncoming => GetClip("missile_incoming") ?? missileIncoming,
            WarningPriority.Stall => GetClip("stall") ?? stall,
            WarningPriority.LowHealth => GetClip("low_health") ?? lowHealth,

            _ => null,
        };
    }

    private void StartWarningInternal(AudioClip clip, WarningPriority priority)
    {
        if (warningSource == null)
        {
            warningSource = CreateAudioSource("Warning", true, 0f);
            warningSource.ignoreListenerPause = true;
        }

        if (warningFadeCoroutine != null)
        {
            StopCoroutine(warningFadeCoroutine);
            warningFadeCoroutine = null;
        }

        currentWarningPriority = priority;
        currentWarningClip = clip;
        warningSource.clip = clip;
        warningSource.loop = true;

        float targetVolume = (isMuted ? 0f : masterVolume) * sfxVolume;
        warningSource.volume = targetVolume * 0.5f;
        warningSource.Play();

        Debug.Log(
            $"[AudioManager] StartWarningInternal: Playing '{clip.name}' at volume {warningSource.volume}, target={targetVolume}, priority={priority}"
        );

        warningFadeCoroutine = StartCoroutine(FadeAudio(warningSource, targetVolume, fadeDuration));
    }

    private void StopWarningInternal()
    {
        if (warningSource != null)
        {
            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
                warningFadeCoroutine = null;
            }

            if (warningSource.isPlaying && fadeDuration > 0f)
            {
                warningFadeCoroutine = StartCoroutine(FadeOutAndStop(warningSource, fadeDuration));
            }
            else
            {
                warningSource.Stop();
                warningSource.volume = 1f;
            }
        }
        currentWarningClip = null;
        currentWarningPriority = WarningPriority.None;
    }

    private void StartWarning(AudioClip clip)
    {
        StartWarningWithPriority(clip, WarningPriority.Stall);
    }

    public void StopWarning()
    {
        foreach (var key in new List<WarningPriority>(activeWarnings.Keys))
        {
            activeWarnings[key] = false;
        }
        StopWarningInternal();
        StopMissileIncomingInternal();
    }

    public WarningPriority CurrentWarningPriority => currentWarningPriority;

    public void StopAllGameSounds()
    {
        Debug.Log("[AudioManager] Stopping all game sounds");

        if (sfxSource != null)
            sfxSource.Stop();

        if (engineSource != null)
            engineSource.Stop();

        if (warningSource != null)
        {
            if (warningFadeCoroutine != null)
            {
                StopCoroutine(warningFadeCoroutine);
                warningFadeCoroutine = null;
            }
            warningSource.Stop();
        }
        currentWarningClip = null;
        currentMissileIncomingClip = null;

        if (missileIncomingSource != null)
        {
            if (missileIncomingFadeCoroutine != null)
            {
                StopCoroutine(missileIncomingFadeCoroutine);
                missileIncomingFadeCoroutine = null;
            }
            missileIncomingSource.Stop();
        }

        if (glocSource != null)
        {
            glocSource.Stop();
        }
        isGForceBreathingActive = false;
        isGLocActive = false;

        if (voiceSource != null)
            voiceSource.Stop();

        if (ambientSource != null)
            ambientSource.Stop();

        foreach (var kvp in loopingSources)
        {
            if (kvp.Value != null)
            {
                kvp.Value.Stop();
                Destroy(kvp.Value.gameObject);
            }
        }
        loopingSources.Clear();
    }

    #endregion

    #region Voice / Radio

    public void PlayEnemyDown()
    {
        if (radioChatterEnabled)
        {
            PlayVoice(GetClip("splash") ?? splash);
            PlayVoice(GetClip("enemy_down") ?? enemyDown);
        }
        PlaySFX(GetClip("kill_confirm") ?? killConfirm);
    }

    public void PlayFoxTwo()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("fox_two") ?? foxTwo);
    }

    public void PlayRTB()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("rtb") ?? rtb);
    }

    public void PlayUnderAttack()
    {
        if (Time.unscaledTime - lastUnderAttackTime < VOICE_COOLDOWN)
        {
            return;
        }
        lastUnderAttackTime = Time.unscaledTime;

        if (radioChatterEnabled)
        {
            Debug.Log("[AudioManager] PlayUnderAttack: Playing voice");
            PlayVoice(GetClip("under_attack") ?? underAttack);
        }
    }

    public void PlayWeHit()
    {
        if (Time.unscaledTime - lastUnderAttackTime < VOICE_COOLDOWN)
        {
            return;
        }
        lastUnderAttackTime = Time.unscaledTime;

        if (radioChatterEnabled)
        {
            AudioClip clip = GetClip("we_hit") ?? weHit;
            if (clip == null)
            {
                if (warnedMissingVoiceClips.Add("we_hit"))
                {
                    Debug.LogWarning(
                        "[AudioManager] PlayWeHit: No clip found for 'we_hit'. "
                            + "Assign AudioManager.weHit or add a 'we_hit' entry in the AudioLibrary/SoundLibrary."
                    );
                }
                return;
            }

            Debug.Log("[AudioManager] PlayWeHit: Playing voice");
            PlayVoice(clip);
        }
    }

    public void PlayMayday()
    {
        if (radioChatterEnabled)
        {
            Debug.Log("[AudioManager] PlayMayday: Playing voice");
            PlayVoice(GetClip("mayday") ?? mayday);
        }
    }

    public void PlayWatchYourSix()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("watch_your_six") ?? watchYourSix);
    }

    public void PlayTargetLocked()
    {
        if (Time.unscaledTime - lastTargetLockedTime < VOICE_COOLDOWN)
        {
            return;
        }
        lastTargetLockedTime = Time.unscaledTime;

        if (radioChatterEnabled)
        {
            AudioClip clip = GetClip("target_locked") ?? targetLocked;
            if (clip == null)
            {
                Debug.LogWarning(
                    "[AudioManager] PlayTargetLocked: No clip found! "
                        + "Assign 'Assets/Audio/Voice/TargetLocked.wav' to AudioManager.targetLocked or AudioLibrary.targetLocked in Inspector."
                );
            }
            else
            {
                Debug.Log($"[AudioManager] PlayTargetLocked: Playing '{clip.name}'");
            }
            PlayVoice(clip);
        }
    }

    public void PlayAirSupport()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("air_support") ?? airSupport);
    }

    public void PlayGoodWork()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("good_work") ?? goodWork);
    }

    public void PlayGameStart()
    {
        PlaySFX(GetClip("game_start") ?? gameStart);
    }

    public void PlayGameOver()
    {
        AudioClip clip = GetClip("game_over") ?? gameOver;
        if (clip == null)
        {
            Debug.LogWarning(
                "[AudioManager] PlayGameOver: No clip found! Assign a game over sound to AudioManager.gameOver or AudioLibrary.gameOver in Inspector."
            );
            return;
        }
        Debug.Log($"[AudioManager] PlayGameOver: Playing '{clip.name}'");
        PlaySFX(clip);
    }

    public void PlayMissionAccomplished()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("mission_accomplished") ?? missionAccomplished);
    }

    public void PlayObjectiveSecured()
    {
        if (radioChatterEnabled)
            PlayVoice(GetClip("objective_secured") ?? objectiveSecured);
    }

    public void PlayRadioCheck()
    {
        PlayVoice(GetClip("radio_check") ?? radioCheck);
    }

    public void PlayRadioStatic()
    {
        PlaySFX(GetClip("radio_static") ?? radioStatic);
    }

    public void PlayLoadingSound(int index)
    {
        AudioClip clip =
            index == 1
                ? (GetClip("loading_sound1") ?? loadingSound1)
                : (GetClip("loading_sound2") ?? loadingSound2);
        if (clip != null)
            PlaySFX(clip);
    }

    public void PlayVoice(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayVoice called with null clip");
            return;
        }
        if (voiceSource == null)
        {
            Debug.LogWarning("[AudioManager] PlayVoice: voiceSource is null");
            return;
        }
        if (!radioChatterEnabled)
        {
            Debug.Log(
                $"[AudioManager] PlayVoice '{clip.name}': RadioChatter is disabled, skipping"
            );
            return;
        }
        float effectiveVolume = (isMuted ? 0f : masterVolume) * voiceVolume;
        if (effectiveVolume <= 0.001f)
        {
            Debug.LogWarning(
                $"[AudioManager] PlayVoice '{clip.name}': Volume is effectively zero (master={masterVolume}, voice={voiceVolume}, muted={isMuted})"
            );
        }
        Debug.Log(
            $"[AudioManager] PlayVoice: Playing '{clip.name}' at volume {effectiveVolume:F2}"
        );
        voiceSource.PlayOneShot(clip);
    }

    #endregion

    #region G-Force Sounds

    public void PlayBreathingHeavy()
    {
        AudioClip clip = GetClip("breathing_heavy") ?? breathingHeavy;
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayHeartbeat()
    {
        AudioClip clip = GetClip("heartbeat") ?? heartbeat;
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void StartGForceLoop(float intensity)
    {
        if (intensity > 0.20f)
        {
            AudioClip clip = GetClip("breathing_heavy") ?? breathingHeavy;
            if (clip != null)
            {
                if (!isGForceBreathingActive)
                {
                    StartGForceBreathingInternal(clip);
                }

                if (glocSource != null && isGForceBreathingActive)
                {
                    float targetVolume = Mathf.Lerp(
                        0f,
                        (isMuted ? 0f : masterVolume) * sfxVolume,
                        Mathf.Clamp01((intensity - 0.20f) / 0.80f)
                    );
                    glocSource.volume = targetVolume;
                }
            }
        }
        else if (intensity <= 0.10f)
        {
            StopGForceBreathingInternal();
        }
    }

    public void StopGForceLoop()
    {
        StopGForceWarning();
    }

    private AudioSource glocSource;
    private bool isGLocActive = false;

    public void PlayGLocEffect()
    {
        if (isGLocActive)
            return;

        AudioClip clip = GetClip("gloc") ?? glocSound;
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayGLocEffect: No G-LOC clip found!");
            return;
        }

        if (glocSource == null)
        {
            glocSource = CreateAudioSource("GLoc", true, 0f);
            glocSource.ignoreListenerPause = true;
        }

        isGForceBreathingActive = false;

        glocSource.clip = clip;
        glocSource.loop = true;
        glocSource.volume = (isMuted ? 0f : masterVolume) * sfxVolume;
        glocSource.Play();
        isGLocActive = true;

        Debug.Log($"[AudioManager] PlayGLocEffect: Starting G-LOC effect '{clip.name}'");
    }

    public void StopGLocEffect()
    {
        if (!isGLocActive)
            return;

        if (glocSource != null && glocSource.isPlaying)
        {
            glocSource.Stop();
            glocSource.clip = null;
        }
        isGLocActive = false;

        Debug.Log(
            "[AudioManager] StopGLocEffect: G-LOC effect stopped, pilot regained consciousness"
        );
    }

    #endregion

    #region Environment / Ambient

    public void PlayRainLoop()
    {
        AudioClip clip = GetClip("rain_loop") ?? rainLoop;
        if (clip != null && ambientSource != null)
        {
            ambientSource.clip = clip;
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    public void StopRainLoop()
    {
        if (ambientSource != null)
            ambientSource.Stop();
    }

    public void SetAmbientVolume(float volume)
    {
        if (ambientSource != null)
        {
            ambientSource.volume =
                (isMuted ? 0f : masterVolume) * sfxVolume * Mathf.Clamp01(volume);
        }
    }

    public AudioClip GetRainLoopClip()
    {
        return GetClip("rain_loop") ?? rainLoop;
    }

    #endregion

    #region Startup Logo Sounds

    private const string StartupLogoAudioPath = "StartupLogos/Audio";

    public AudioClip[] GetStartupLogoSounds()
    {
        if (startupLogoSounds != null && startupLogoSounds.Length > 0)
        {
            return startupLogoSounds;
        }

        var loadedClips = Resources.LoadAll<AudioClip>(StartupLogoAudioPath);
        if (loadedClips != null && loadedClips.Length > 0)
        {
            startupLogoSounds = loadedClips
                .OrderBy(c => c.name, System.StringComparer.OrdinalIgnoreCase)
                .ToArray();
            Debug.Log(
                $"[AudioManager] Auto-loaded {startupLogoSounds.Length} startup logo sounds from Resources/{StartupLogoAudioPath}"
            );
        }
        else
        {
            Debug.LogWarning(
                $"[AudioManager] No startup logo sounds found. Assign clips in Inspector or place them in Resources/{StartupLogoAudioPath}/"
            );
        }

        return startupLogoSounds;
    }

    public AudioClip GetStartupLogoSound(int index)
    {
        if (startupLogoSounds == null || startupLogoSounds.Length == 0)
            return null;
        if (index < 0 || index >= startupLogoSounds.Length)
            return null;
        return startupLogoSounds[index];
    }

    public void PlayStartupLogoSound(int index)
    {
        var clip = GetStartupLogoSound(index);
        if (clip != null)
        {
            PlaySFX(clip);
            Debug.Log($"[AudioManager] Playing startup logo sound {index}: {clip.name}");
        }
    }

    #endregion

    #region Generic Sound Playback

    public void PlaySound(string soundName)
    {
        if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Sound not found: '{soundName}'");
        }
    }

    public void PlaySoundAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, position, (isMuted ? 0f : masterVolume) * sfxVolume);
        }
    }

    public void PlaySoundAtPosition(string soundName, Vector3 position)
    {
        if (soundLibrary.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySoundAtPosition(clip, position);
        }
    }

    public AudioClip GetClip(string key)
    {
        soundLibrary.TryGetValue(key, out AudioClip clip);
        return clip;
    }

    public void RegisterClip(string key, AudioClip clip)
    {
        if (clip != null)
        {
            soundLibrary[key] = clip;
        }
    }

    #endregion

    #region Audio Helpers

    public void PauseGameAudio()
    {
        if (isGameAudioPaused)
            return;

        isGameAudioPaused = true;
        Debug.Log("[AudioManager] Pausing game audio");

        if (engineSource != null)
        {
            wasEnginePlaying = engineSource.isPlaying;
            if (wasEnginePlaying)
                engineSource.Pause();
        }

        if (cannonSource != null)
        {
            wasCannonPlaying = cannonSource.isPlaying;
            if (wasCannonPlaying)
                cannonSource.Pause();
        }

        if (missileLockSource != null)
        {
            wasMissileLockPlaying = missileLockSource.isPlaying;
            if (wasMissileLockPlaying)
                missileLockSource.Pause();
        }

        if (warningSource != null)
        {
            wasWarningPlaying = warningSource.isPlaying;
            if (wasWarningPlaying)
                warningSource.Pause();
        }

        if (missileIncomingSource != null)
        {
            wasMissileIncomingPlaying = missileIncomingSource.isPlaying;
            if (wasMissileIncomingPlaying)
                missileIncomingSource.Pause();
        }

        if (ambientSource != null)
        {
            wasAmbientPlaying = ambientSource.isPlaying;
            if (wasAmbientPlaying)
                ambientSource.Pause();
        }

        if (glocSource != null)
        {
            wasGLocPlaying = glocSource.isPlaying;
            if (wasGLocPlaying)
                glocSource.Pause();
        }

        if (sfxSource != null && sfxSource.isPlaying)
        {
            sfxSource.Pause();
        }

        if (voiceSource != null && voiceSource.isPlaying)
        {
            voiceSource.Pause();
        }
    }

    public void ResumeGameAudio()
    {
        if (!isGameAudioPaused)
            return;

        isGameAudioPaused = false;
        Debug.Log("[AudioManager] Resuming game audio");

        if (engineSource != null && wasEnginePlaying)
        {
            engineSource.UnPause();
        }

        if (cannonSource != null && wasCannonPlaying)
        {
            cannonSource.UnPause();
        }

        if (missileLockSource != null && wasMissileLockPlaying)
        {
            missileLockSource.UnPause();
        }

        if (warningSource != null && wasWarningPlaying)
        {
            warningSource.UnPause();
        }

        if (missileIncomingSource != null && wasMissileIncomingPlaying)
        {
            missileIncomingSource.UnPause();
        }

        if (ambientSource != null && wasAmbientPlaying)
        {
            ambientSource.UnPause();
        }

        if (glocSource != null && wasGLocPlaying)
        {
            glocSource.UnPause();
        }

        if (sfxSource != null)
        {
            sfxSource.UnPause();
        }

        if (voiceSource != null)
        {
            voiceSource.UnPause();
        }
    }

    public bool IsGameAudioPaused => isGameAudioPaused;

    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        if (source == null)
            yield break;

        if (duration <= 0f)
        {
            source.volume = targetVolume;
            yield break;
        }

        float startVolume = source.volume;
        float t = 0f;
        while (t < duration && source != null)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            source.volume = Mathf.Lerp(startVolume, targetVolume, a);
            yield return null;
        }

        if (source != null)
        {
            source.volume = targetVolume;
        }
    }

    private IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null)
            yield break;

        yield return FadeAudio(source, 0f, duration);
        if (source != null)
        {
            source.Stop();
            source.volume = 1f;
        }
    }

    public void StopAllSounds()
    {
        Debug.Log("[AudioManager] Stopping all sounds");

        if (musicSource != null)
        {
            musicSource.Stop();
        }
        if (uiSource != null)
        {
            uiSource.Stop();
        }
        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
        if (engineSource != null)
        {
            engineSource.Stop();
            engineSource.loop = false;
        }
        if (warningSource != null)
        {
            warningSource.Stop();
            warningSource.loop = false;
        }
        if (missileIncomingSource != null)
        {
            missileIncomingSource.Stop();
            missileIncomingSource.loop = false;
        }
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.loop = false;
        }

        StopAllCoroutines();
    }

    public void StopGameplaySounds()
    {
        Debug.Log("[AudioManager] Stopping gameplay sounds");

        if (sfxSource != null)
        {
            sfxSource.Stop();
        }
        if (engineSource != null)
        {
            engineSource.Stop();
            engineSource.loop = false;
        }
        if (warningSource != null)
        {
            warningSource.Stop();
            warningSource.loop = false;
        }
        if (missileIncomingSource != null)
        {
            missileIncomingSource.Stop();
            missileIncomingSource.loop = false;
        }
        if (voiceSource != null)
        {
            voiceSource.Stop();
        }
        if (ambientSource != null)
        {
            ambientSource.Stop();
            ambientSource.loop = false;
        }

        StopAllGameplayAudioSources();
    }

    private void StopAllGameplayAudioSources()
    {
        var stallWarning = FindAnyObjectByType<StallWarning>();
        if (stallWarning != null)
        {
            var audioSources = stallWarning.GetComponentsInChildren<AudioSource>(true);
            foreach (var source in audioSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    source.loop = false;
                }
            }
        }

        var afterburners = FindObjectsByType<AfterburnerEffect>(FindObjectsSortMode.None);
        foreach (var afterburner in afterburners)
        {
            if (afterburner != null)
            {
                var audioSources = afterburner.GetComponentsInChildren<AudioSource>(true);
                foreach (var source in audioSources)
                {
                    if (source != null && source.isPlaying)
                    {
                        source.Stop();
                        source.loop = false;
                    }
                }
            }
        }

        var threatWarning = FindAnyObjectByType<ThreatWarningSystem>();
        if (threatWarning != null)
        {
            var audioSources = threatWarning.GetComponentsInChildren<AudioSource>(true);
            foreach (var source in audioSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                    source.loop = false;
                }
            }
        }

        var planes = FindObjectsByType<Plane>(FindObjectsSortMode.None);
        foreach (var plane in planes)
        {
            if (plane != null)
            {
                var audioSources = plane.GetComponentsInChildren<AudioSource>(true);
                foreach (var source in audioSources)
                {
                    if (source != null && source.isPlaying)
                    {
                        source.Stop();
                        source.loop = false;
                    }
                }
            }
        }

        Debug.Log("[AudioManager] Stopped all gameplay AudioSources");
    }

    #endregion

    #region Public Properties

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float SFXVolume => sfxVolume;
    public float EngineVolume => engineVolume;
    public float VoiceVolume => voiceVolume;
    public float UIVolume => sfxVolume;
    public bool IsMuted => isMuted;
    public bool RadioChatterEnabled => radioChatterEnabled;
    public AudioLibrary Library => audioLibrary;

    #endregion
}
