using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class SettingsController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField]
    private UIDocument uiDocument;

    [Header("Audio")]
    [SerializeField]
    private AudioMixer audioMixer;

    private VisualElement root;
    private VisualElement settingsOverlay;

    private Button audioTab;
    private Button graphicsTab;
    private Button controlsTab;
    private Button gameplayTab;
    private VisualElement audioSection;
    private VisualElement graphicsSection;
    private VisualElement controlsSection;
    private VisualElement gameplaySection;

    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private Slider engineVolumeSlider;
    private Slider voiceVolumeSlider;
    private Label masterVolumeValue;
    private Label musicVolumeValue;
    private Label sfxVolumeValue;
    private Label engineVolumeValue;
    private Label voiceVolumeValue;
    private Toggle muteToggle;
    private Toggle radioChatterToggle;

    private DropdownField resolutionDropdown;
    private DropdownField qualityDropdown;
    private DropdownField displayModeDropdown;
    private DropdownField frameRateDropdown;
    private DropdownField antiAliasingDropdown;
    private DropdownField shadowDropdown;
    private DropdownField textureDropdown;
    private Toggle vsyncToggle;
    private Toggle motionBlurToggle;
    private Toggle bloomToggle;
    private Toggle gforceEffectsToggle;
    private Slider renderDistanceSlider;
    private Slider fovSlider;
    private Label renderDistanceValue;
    private Label fovValue;

    private Slider sensitivitySlider;
    private Slider deadzoneSlider;
    private Label sensitivityValue;
    private Label deadzoneValue;
    private Toggle invertYToggle;
    private Toggle invertXToggle;
    private Toggle vibrationToggle;
    private Toggle autoStabilizeToggle;
    private DropdownField controlSchemeDropdown;

    private DropdownField difficultyDropdown;
    private Slider maxWavesSlider;
    private Label maxWavesValue;
    private DropdownField flightModelDropdown;
    private DropdownField speedUnitDropdown;
    private DropdownField altitudeUnitDropdown;
    private DropdownField languageDropdown;
    private Toggle hudToggle;
    private Toggle minimapToggle;
    private Toggle autoSaveToggle;
    private Toggle tutorialToggle;
    private Toggle unlimitedAmmoToggle;
    private Toggle autoAimToggle;
    private Slider hudOpacitySlider;
    private DropdownField cameraViewDropdown;
    private Toggle cameraShakeToggle;

    private Toggle radarShowEnemiesToggle;
    private Toggle radarShowMissilesToggle;
    private Slider radarMarkerSizeSlider;
    private Label radarMarkerSizeValue;
    private Slider radarRangeSlider;
    private Label radarRangeValue;

    private Button resetButton;
    private Button applyButton;
    private Button backButton;

    private Button exportSaveButton;
    private Button importSaveButton;
    private Button resetProgressButton;
    private Label savePathLabel;

    private float masterVolume = 0.9f;
    private float musicVolume = 0.9f;
    private float sfxVolume = 0.9f;
    private float engineVolume = 0.9f;
    private float voiceVolume = 0.9f;
    private bool isMuted = false;
    private bool radioChatter = true;

    private int resolutionIndex = 0;
    private int qualityIndex = 2;
    private int displayModeIndex = 0;
    private int frameRateLimit = 0;
    private int antiAliasing = 2;
    private int shadowQuality = 2;
    private int textureQuality = 0;
    private bool vsync = true;
    private bool motionBlur = false;
    private bool bloom = true;
    private bool gforceEffects = true;
    private float renderDistance = 5000f;
    private float fieldOfView = 75f;

    private float mouseSensitivity = 1f;
    private float joystickDeadzone = 0.1f;
    private bool invertY = false;
    private bool invertX = false;
    private bool vibration = true;
    private bool autoStabilize = true;
    private int controlScheme = 0;

    private int difficulty = 1;
    private int maxWaves = 0;
    private int flightModel = 0;
    private int speedUnit = 0;
    private int altitudeUnit = 0;
    private int language = 0;
    private bool showHUD = true;
    private bool showMinimap = true;
    private bool autoSave = true;
    private bool showTutorial = true;
    private bool unlimitedAmmo = false;
    private bool autoAimAssist = false;
    private float hudOpacity = 0.9f;
    private int cameraView = 0;
    private bool cameraShake = true;

    private bool radarShowEnemies = true;
    private bool radarShowMissiles = true;
    private float radarMarkerSize = 1f;
    private float radarRange = 4000f;

    private List<Resolution> availableResolutions;

    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string ENGINE_VOLUME_KEY = "EngineVolume";
    private const string VOICE_VOLUME_KEY = "VoiceVolume";
    private const string MUTE_KEY = "Mute";
    private const string RADIO_CHATTER_KEY = "RadioChatter";
    private const string RESOLUTION_KEY = "Resolution";
    private const string QUALITY_KEY = "QualityLevel";
    private const string DISPLAY_MODE_KEY = "DisplayMode";
    private const string FRAME_RATE_KEY = "FrameRate";
    private const string VSYNC_KEY = "VSync";
    private const string AA_KEY = "AntiAliasing";
    private const string SHADOW_KEY = "ShadowQuality";
    private const string TEXTURE_KEY = "TextureQuality";
    private const string MOTION_BLUR_KEY = "MotionBlur";
    private const string BLOOM_KEY = "Bloom";
    private const string RENDER_DISTANCE_KEY = "RenderDistance";
    private const string FOV_KEY = "FieldOfView";
    private const string SENSITIVITY_KEY = "MouseSensitivity";
    private const string DEADZONE_KEY = "JoystickDeadzone";
    private const string INVERT_Y_KEY = "InvertY";
    private const string INVERT_X_KEY = "InvertX";
    private const string VIBRATION_KEY = "Vibration";
    private const string AUTO_STABILIZE_KEY = "AutoStabilize";
    private const string CONTROL_SCHEME_KEY = "ControlScheme";
    private const string DIFFICULTY_KEY = "Difficulty";
    private const string MAX_WAVES_KEY = "MaxWaves";
    private const string FLIGHT_MODEL_KEY = "FlightModel";
    private const string SPEED_UNIT_KEY = "SpeedUnit";
    private const string ALTITUDE_UNIT_KEY = "AltitudeUnit";
    private const string LANGUAGE_KEY = "Language";
    private const string SHOW_HUD_KEY = "ShowHUD";
    private const string SHOW_MINIMAP_KEY = "ShowMinimap";
    private const string AUTO_SAVE_KEY = "AutoSave";
    private const string SHOW_TUTORIAL_KEY = "ShowTutorial";
    private const string UNLIMITED_AMMO_KEY = "UnlimitedAmmo";
    private const string AUTO_AIM_KEY = "AutoAimAssist";
    private const string HUD_OPACITY_KEY = "HUDOpacity";
    private const string RADAR_SHOW_ENEMIES_KEY = "RadarShowEnemies";
    private const string RADAR_SHOW_MISSILES_KEY = "RadarShowMissiles";
    private const string RADAR_MARKER_SIZE_KEY = "RadarMarkerSize";
    private const string RADAR_RANGE_KEY = "RadarRange";

    private MainMenuController mainMenuController;
    private PauseMenuController pauseMenuController;

    private static SettingsController activeInstance = null;

    private bool isSettingsOpen = false;

    public bool IsSettingsOpen => isSettingsOpen;

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

        bool needsSetup = uiDocument == null;
        if (needsSetup)
        {
            if (
                uiDocument != null
                && uiDocument.visualTreeAsset != null
                && uiDocument.visualTreeAsset.name != "SettingsMenu"
            )
            {
                GameObject childGO = new GameObject("SettingsUI");
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
            uiDocument.sortingOrder = 1000;

            if (
                uiDocument.visualTreeAsset == null
                || uiDocument.visualTreeAsset.name != "SettingsMenu"
            )
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

                var settingsUxml = Resources.Load<VisualTreeAsset>("UI/SettingsMenu");
                if (settingsUxml == null)
                {
#if UNITY_EDITOR
                    settingsUxml = UnityEditor.AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        "Assets/UI/UXML/SettingsMenu.uxml"
                    );
#endif
                }

                if (settingsUxml != null)
                {
                    uiDocument.visualTreeAsset = settingsUxml;
                    Debug.Log("[Settings] Auto-loaded SettingsMenu.uxml");
                }
                else
                {
                    Debug.LogError("[Settings] Could not load SettingsMenu.uxml!");
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

        BindUIElements();
        RegisterCallbacks();
    }

    private void Start()
    {
        if (!enabled)
            return;

        EnsureReferences();
        LoadSettings();
        InitializeDropdowns();
        InitializeUI();
        SetSettingsVisible(false);
        isSettingsOpen = false;

        Debug.Log("[Settings] Initialized and hidden");
    }

    private void Update()
    {
        if (isSettingsOpen && IsEscapePressedThisFrame())
        {
            Debug.Log("[Settings] ESC pressed while settings open, closing...");
            BackToPreviousMenu();
        }
    }

    private void BindUIElements()
    {
        settingsOverlay = root.Q<VisualElement>("overlay");

        audioTab = root.Q<Button>("tab-audio");
        graphicsTab = root.Q<Button>("tab-graphics");
        controlsTab = root.Q<Button>("tab-controls");
        gameplayTab = root.Q<Button>("tab-gameplay");

        audioSection = root.Q<VisualElement>("audio-content");
        graphicsSection = root.Q<VisualElement>("graphics-content");
        controlsSection = root.Q<VisualElement>("controls-content");
        gameplaySection = root.Q<VisualElement>("gameplay-content");

        masterVolumeSlider = root.Q<Slider>("master-volume");
        musicVolumeSlider = root.Q<Slider>("music-volume");
        sfxVolumeSlider = root.Q<Slider>("sfx-volume");
        engineVolumeSlider = root.Q<Slider>("engine-volume");
        voiceVolumeSlider = root.Q<Slider>("voice-volume");
        muteToggle = root.Q<Toggle>("mute-all");
        radioChatterToggle = root.Q<Toggle>("radio-chatter");

        resolutionDropdown = root.Q<DropdownField>("resolution");
        qualityDropdown = root.Q<DropdownField>("quality-preset");
        displayModeDropdown = root.Q<DropdownField>("display-mode");
        frameRateDropdown = root.Q<DropdownField>("target-fps");
        antiAliasingDropdown = root.Q<DropdownField>("anti-aliasing");
        shadowDropdown = root.Q<DropdownField>("shadow-quality");
        textureDropdown = root.Q<DropdownField>("texture-quality");
        vsyncToggle = root.Q<Toggle>("vsync");
        motionBlurToggle = root.Q<Toggle>("motion-blur");
        bloomToggle = root.Q<Toggle>("bloom");
        gforceEffectsToggle = root.Q<Toggle>("gforce-effects");
        renderDistanceSlider = root.Q<Slider>("view-distance");
        fovSlider = root.Q<Slider>("fov");

        sensitivitySlider = root.Q<Slider>("mouse-sensitivity");
        deadzoneSlider = root.Q<Slider>("stick-deadzone");
        invertYToggle = root.Q<Toggle>("invert-y");
        invertXToggle = root.Q<Toggle>("invert-x");
        vibrationToggle = root.Q<Toggle>("vibration");
        autoStabilizeToggle = root.Q<Toggle>("flight-assist");

        difficultyDropdown = root.Q<DropdownField>("difficulty");
        maxWavesSlider = root.Q<Slider>("max-waves");
        maxWavesValue = root.Q<Label>("max-waves-value");
        hudToggle = root.Q<Toggle>("show-hud");
        minimapToggle = root.Q<Toggle>("show-minimap");
        autoSaveToggle = root.Q<Toggle>("show-waypoints");
        tutorialToggle = root.Q<Toggle>("camera-shake");
        unlimitedAmmoToggle = root.Q<Toggle>("unlimited-ammo");
        autoAimToggle = root.Q<Toggle>("auto-aim");
        hudOpacitySlider = root.Q<Slider>("hud-opacity");
        cameraViewDropdown = root.Q<DropdownField>("camera-view");
        cameraShakeToggle = root.Q<Toggle>("camera-shake");

        radarShowEnemiesToggle = root.Q<Toggle>("radar-show-enemies");
        radarShowMissilesToggle = root.Q<Toggle>("radar-show-missiles");
        radarMarkerSizeSlider = root.Q<Slider>("radar-marker-size");
        radarMarkerSizeValue = root.Q<Label>("radar-marker-size-value");
        radarRangeSlider = root.Q<Slider>("radar-range");
        radarRangeValue = root.Q<Label>("radar-range-value");

        resetButton = root.Q<Button>("reset-button");
        applyButton = root.Q<Button>("apply-button");
        backButton = root.Q<Button>("close-button");

        exportSaveButton = root.Q<Button>("export-save-button");
        importSaveButton = root.Q<Button>("import-save-button");
        resetProgressButton = root.Q<Button>("reset-progress-button");
        savePathLabel = root.Q<Label>("save-path-label");

        if (savePathLabel != null)
        {
            savePathLabel.text = $"Save location: {GameManager.GetDefaultSavePath()}";
        }
    }

    private void RegisterCallbacks()
    {
        audioTab?.RegisterCallback<ClickEvent>(evt => SwitchTab(0));
        graphicsTab?.RegisterCallback<ClickEvent>(evt => SwitchTab(1));
        controlsTab?.RegisterCallback<ClickEvent>(evt => SwitchTab(2));
        gameplayTab?.RegisterCallback<ClickEvent>(evt => SwitchTab(3));

        masterVolumeSlider?.RegisterValueChangedCallback(evt =>
        {
            masterVolume = evt.newValue;
            UpdateVolumeTexts();
            ApplyAudio();
        });
        musicVolumeSlider?.RegisterValueChangedCallback(evt =>
        {
            musicVolume = evt.newValue;
            UpdateVolumeTexts();
            ApplyAudio();
        });
        sfxVolumeSlider?.RegisterValueChangedCallback(evt =>
        {
            sfxVolume = evt.newValue;
            UpdateVolumeTexts();
            ApplyAudio();
        });
        engineVolumeSlider?.RegisterValueChangedCallback(evt =>
        {
            engineVolume = evt.newValue;
            UpdateVolumeTexts();
            ApplyAudio();
        });
        voiceVolumeSlider?.RegisterValueChangedCallback(evt =>
        {
            voiceVolume = evt.newValue;
            UpdateVolumeTexts();
            ApplyAudio();
        });
        muteToggle?.RegisterValueChangedCallback(evt =>
        {
            isMuted = evt.newValue;
            ApplyAudio();
        });
        radioChatterToggle?.RegisterValueChangedCallback(evt =>
        {
            radioChatter = evt.newValue;
            ApplyAudio();
        });

        resolutionDropdown?.RegisterValueChangedCallback(evt =>
            resolutionIndex = resolutionDropdown.index
        );
        qualityDropdown?.RegisterValueChangedCallback(evt =>
        {
            qualityIndex = qualityDropdown.index;
            QualitySettings.SetQualityLevel(qualityIndex);
        });
        displayModeDropdown?.RegisterValueChangedCallback(evt =>
            displayModeIndex = displayModeDropdown.index
        );
        frameRateDropdown?.RegisterValueChangedCallback(evt =>
        {
            frameRateLimit = frameRateDropdown.index;
            ApplyFrameRate(frameRateDropdown.index);
        });
        antiAliasingDropdown?.RegisterValueChangedCallback(evt =>
            ApplyAntiAliasing(antiAliasingDropdown.index)
        );
        shadowDropdown?.RegisterValueChangedCallback(evt =>
            ApplyShadowQuality(shadowDropdown.index)
        );
        textureDropdown?.RegisterValueChangedCallback(evt =>
            ApplyTextureQuality(textureDropdown.index)
        );
        vsyncToggle?.RegisterValueChangedCallback(evt =>
        {
            vsync = evt.newValue;
            QualitySettings.vSyncCount = vsync ? 1 : 0;
        });
        motionBlurToggle?.RegisterValueChangedCallback(evt =>
        {
            motionBlur = evt.newValue;
            ApplyMotionBlur();
        });
        bloomToggle?.RegisterValueChangedCallback(evt =>
        {
            bloom = evt.newValue;
            ApplyBloom();
        });
        gforceEffectsToggle?.RegisterValueChangedCallback(evt =>
        {
            gforceEffects = evt.newValue;
        });
        renderDistanceSlider?.RegisterValueChangedCallback(evt =>
        {
            renderDistance = evt.newValue;
            UpdateRenderDistanceText();
            ApplyRenderDistance();
        });
        fovSlider?.RegisterValueChangedCallback(evt =>
        {
            fieldOfView = evt.newValue;
            UpdateFOVText();
            ApplyFOV();
        });

        sensitivitySlider?.RegisterValueChangedCallback(evt =>
        {
            mouseSensitivity = evt.newValue;
            UpdateSensitivityText();
            NotifyPlayerControllerOfChanges();
        });
        deadzoneSlider?.RegisterValueChangedCallback(evt =>
        {
            joystickDeadzone = evt.newValue;
            UpdateDeadzoneText();
            NotifyPlayerControllerOfChanges();
        });
        invertYToggle?.RegisterValueChangedCallback(evt =>
        {
            invertY = evt.newValue;
            NotifyPlayerControllerOfChanges();
        });
        invertXToggle?.RegisterValueChangedCallback(evt =>
        {
            invertX = evt.newValue;
            NotifyPlayerControllerOfChanges();
        });
        vibrationToggle?.RegisterValueChangedCallback(evt => vibration = evt.newValue);
        autoStabilizeToggle?.RegisterValueChangedCallback(evt =>
        {
            autoStabilize = evt.newValue;
            NotifyPlayerControllerOfChanges();
        });
        controlSchemeDropdown?.RegisterValueChangedCallback(evt =>
            controlScheme = controlSchemeDropdown.index
        );

        difficultyDropdown?.RegisterValueChangedCallback(evt =>
            difficulty = difficultyDropdown.index
        );
        maxWavesSlider?.RegisterValueChangedCallback(evt =>
        {
            maxWaves = (int)evt.newValue;
            UpdateMaxWavesText();
        });
        flightModelDropdown?.RegisterValueChangedCallback(evt =>
            flightModel = flightModelDropdown.index
        );
        speedUnitDropdown?.RegisterValueChangedCallback(evt => speedUnit = speedUnitDropdown.index);
        altitudeUnitDropdown?.RegisterValueChangedCallback(evt =>
            altitudeUnit = altitudeUnitDropdown.index
        );
        languageDropdown?.RegisterValueChangedCallback(evt => language = languageDropdown.index);
        hudToggle?.RegisterValueChangedCallback(evt => showHUD = evt.newValue);
        minimapToggle?.RegisterValueChangedCallback(evt => showMinimap = evt.newValue);
        autoSaveToggle?.RegisterValueChangedCallback(evt => autoSave = evt.newValue);
        tutorialToggle?.RegisterValueChangedCallback(evt => showTutorial = evt.newValue);
        unlimitedAmmoToggle?.RegisterValueChangedCallback(evt =>
        {
            unlimitedAmmo = evt.newValue;
            NotifyPlaneOfUnlimitedAmmo();
        });
        autoAimToggle?.RegisterValueChangedCallback(evt => autoAimAssist = evt.newValue);
        hudOpacitySlider?.RegisterValueChangedCallback(evt => hudOpacity = evt.newValue);
        cameraViewDropdown?.RegisterValueChangedCallback(evt =>
            cameraView = cameraViewDropdown.index
        );
        cameraShakeToggle?.RegisterValueChangedCallback(evt => cameraShake = evt.newValue);

        radarShowEnemiesToggle?.RegisterValueChangedCallback(evt =>
            radarShowEnemies = evt.newValue
        );
        radarShowMissilesToggle?.RegisterValueChangedCallback(evt =>
            radarShowMissiles = evt.newValue
        );
        radarMarkerSizeSlider?.RegisterValueChangedCallback(evt =>
        {
            radarMarkerSize = evt.newValue;
            UpdateRadarMarkerSizeText();
        });
        radarRangeSlider?.RegisterValueChangedCallback(evt =>
        {
            radarRange = evt.newValue;
            UpdateRadarRangeText();
        });

        resetButton?.RegisterCallback<ClickEvent>(evt => ResetToDefaults());
        applyButton?.RegisterCallback<ClickEvent>(evt => ApplyAndSave());
        backButton?.RegisterCallback<ClickEvent>(evt => BackToPreviousMenu());

        exportSaveButton?.RegisterCallback<ClickEvent>(evt => OnExportSave());
        importSaveButton?.RegisterCallback<ClickEvent>(evt => OnImportSave());
        resetProgressButton?.RegisterCallback<ClickEvent>(evt => OnResetProgress());
    }

    private void InitializeDropdowns()
    {
        availableResolutions = Screen
            .resolutions.Where(r => r.refreshRateRatio.value >= 59)
            .GroupBy(r => new { r.width, r.height })
            .Select(g => g.First())
            .OrderByDescending(r => r.width * r.height)
            .ToList();

        if (resolutionDropdown != null)
        {
            resolutionDropdown.choices = availableResolutions
                .Select(r => $"{r.width} x {r.height}")
                .ToList();

            if (resolutionIndex >= 0 && resolutionIndex < availableResolutions.Count)
            {
                resolutionDropdown.index = resolutionIndex;
            }
            else
            {
                var currentRes = availableResolutions.FindIndex(r =>
                    r.width == Screen.currentResolution.width
                    && r.height == Screen.currentResolution.height
                );
                resolutionDropdown.index = currentRes >= 0 ? currentRes : 0;
                resolutionIndex = resolutionDropdown.index;
            }
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.choices = new List<string>(QualitySettings.names);
            qualityDropdown.index = qualityIndex;
        }

        if (displayModeDropdown != null)
        {
            displayModeDropdown.choices = new List<string>
            {
                "Fullscreen",
                "Windowed",
                "Borderless Window",
            };
            displayModeDropdown.index = displayModeIndex;
        }

        if (frameRateDropdown != null)
        {
            frameRateDropdown.choices = new List<string>
            {
                "Unlimited",
                "30 FPS",
                "60 FPS",
                "120 FPS",
                "144 FPS",
            };
            frameRateDropdown.index = frameRateLimit;
        }

        if (antiAliasingDropdown != null)
        {
            antiAliasingDropdown.choices = new List<string>
            {
                "Off",
                "2x MSAA",
                "4x MSAA",
                "8x MSAA",
            };
            antiAliasingDropdown.index = antiAliasing;
        }

        if (shadowDropdown != null)
        {
            shadowDropdown.choices = new List<string> { "Off", "Low", "Medium", "High", "Ultra" };
            shadowDropdown.index = shadowQuality;
        }

        if (textureDropdown != null)
        {
            textureDropdown.choices = new List<string> { "Full", "Half", "Quarter" };
            textureDropdown.index = textureQuality;
        }

        if (controlSchemeDropdown != null)
        {
            controlSchemeDropdown.choices = new List<string>
            {
                "Standard",
                "Arcade",
                "Simulation",
                "Custom",
            };
            controlSchemeDropdown.index = 0;
        }

        if (difficultyDropdown != null)
        {
            difficultyDropdown.choices = new List<string> { "Easy", "Normal", "Hard", "Realistic" };
            difficultyDropdown.index = 1;
        }

        if (flightModelDropdown != null)
        {
            flightModelDropdown.choices = new List<string> { "Arcade", "Simplified", "Realistic" };
            flightModelDropdown.index = 0;
        }

        if (speedUnitDropdown != null)
        {
            speedUnitDropdown.choices = new List<string> { "Knots", "km/h", "mph", "m/s" };
            speedUnitDropdown.index = 0;
        }

        if (altitudeUnitDropdown != null)
        {
            altitudeUnitDropdown.choices = new List<string> { "Feet", "Meters" };
            altitudeUnitDropdown.index = 0;
        }

        if (languageDropdown != null)
        {
            languageDropdown.choices = new List<string> { "English", "Chinese", "Japanese", "" };
            languageDropdown.index = 0;
        }
    }

    private void InitializeUI()
    {
        masterVolumeSlider?.SetValueWithoutNotify(masterVolume);
        musicVolumeSlider?.SetValueWithoutNotify(musicVolume);
        sfxVolumeSlider?.SetValueWithoutNotify(sfxVolume);
        engineVolumeSlider?.SetValueWithoutNotify(engineVolume);
        voiceVolumeSlider?.SetValueWithoutNotify(voiceVolume);
        muteToggle?.SetValueWithoutNotify(isMuted);
        radioChatterToggle?.SetValueWithoutNotify(radioChatter);

        vsyncToggle?.SetValueWithoutNotify(vsync);
        motionBlurToggle?.SetValueWithoutNotify(motionBlur);
        bloomToggle?.SetValueWithoutNotify(bloom);
        gforceEffectsToggle?.SetValueWithoutNotify(gforceEffects);
        renderDistanceSlider?.SetValueWithoutNotify(renderDistance);
        fovSlider?.SetValueWithoutNotify(fieldOfView);

        sensitivitySlider?.SetValueWithoutNotify(mouseSensitivity);
        deadzoneSlider?.SetValueWithoutNotify(joystickDeadzone);
        invertYToggle?.SetValueWithoutNotify(invertY);
        invertXToggle?.SetValueWithoutNotify(invertX);
        vibrationToggle?.SetValueWithoutNotify(vibration);
        autoStabilizeToggle?.SetValueWithoutNotify(autoStabilize);

        maxWavesSlider?.SetValueWithoutNotify(maxWaves);
        hudToggle?.SetValueWithoutNotify(showHUD);
        minimapToggle?.SetValueWithoutNotify(showMinimap);
        autoSaveToggle?.SetValueWithoutNotify(autoSave);
        tutorialToggle?.SetValueWithoutNotify(showTutorial);
        unlimitedAmmoToggle?.SetValueWithoutNotify(unlimitedAmmo);
        autoAimToggle?.SetValueWithoutNotify(autoAimAssist);
        hudOpacitySlider?.SetValueWithoutNotify(hudOpacity);
        cameraViewDropdown?.SetValueWithoutNotify(cameraViewDropdown.choices[cameraView]);
        cameraShakeToggle?.SetValueWithoutNotify(cameraShake);

        radarShowEnemiesToggle?.SetValueWithoutNotify(radarShowEnemies);
        radarShowMissilesToggle?.SetValueWithoutNotify(radarShowMissiles);
        radarMarkerSizeSlider?.SetValueWithoutNotify(radarMarkerSize);
        radarRangeSlider?.SetValueWithoutNotify(radarRange);

        UpdateAllTexts();
        SwitchTab(0);
    }

    private void SwitchTab(int tabIndex)
    {
        audioSection?.RemoveFromClassList("tab-visible");
        graphicsSection?.RemoveFromClassList("tab-visible");
        controlsSection?.RemoveFromClassList("tab-visible");
        gameplaySection?.RemoveFromClassList("tab-visible");

        audioTab?.RemoveFromClassList("tab-active");
        graphicsTab?.RemoveFromClassList("tab-active");
        controlsTab?.RemoveFromClassList("tab-active");
        gameplayTab?.RemoveFromClassList("tab-active");

        switch (tabIndex)
        {
            case 0:
                audioSection?.AddToClassList("tab-visible");
                audioTab?.AddToClassList("tab-active");
                break;
            case 1:
                graphicsSection?.AddToClassList("tab-visible");
                graphicsTab?.AddToClassList("tab-active");
                break;
            case 2:
                controlsSection?.AddToClassList("tab-visible");
                controlsTab?.AddToClassList("tab-active");
                break;
            case 3:
                gameplaySection?.AddToClassList("tab-visible");
                gameplayTab?.AddToClassList("tab-active");
                break;
        }
    }

    private void LoadSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.9f);
        musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.9f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0.9f);
        engineVolume = PlayerPrefs.GetFloat(ENGINE_VOLUME_KEY, 0.9f);
        voiceVolume = PlayerPrefs.GetFloat(VOICE_VOLUME_KEY, 0.9f);
        isMuted = PlayerPrefs.GetInt(MUTE_KEY, 0) == 1;
        radioChatter = PlayerPrefs.GetInt(RADIO_CHATTER_KEY, 1) == 1;

        resolutionIndex = PlayerPrefs.GetInt(RESOLUTION_KEY, 0);
        qualityIndex = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        displayModeIndex = PlayerPrefs.GetInt(DISPLAY_MODE_KEY, 0);
        frameRateLimit = PlayerPrefs.GetInt(FRAME_RATE_KEY, 0);
        antiAliasing = PlayerPrefs.GetInt(AA_KEY, 2);
        shadowQuality = PlayerPrefs.GetInt(SHADOW_KEY, 2);
        textureQuality = PlayerPrefs.GetInt(TEXTURE_KEY, 0);
        vsync = PlayerPrefs.GetInt(VSYNC_KEY, 1) == 1;
        renderDistance = PlayerPrefs.GetFloat(RENDER_DISTANCE_KEY, 5000f);
        fieldOfView = PlayerPrefs.GetFloat(FOV_KEY, 75f);
        motionBlur = PlayerPrefs.GetInt(MOTION_BLUR_KEY, 0) == 1;
        bloom = PlayerPrefs.GetInt(BLOOM_KEY, 1) == 1;
        gforceEffects = PlayerPrefs.GetInt("GForceEffects", 1) == 1;

        mouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);
        joystickDeadzone = PlayerPrefs.GetFloat(DEADZONE_KEY, 0.1f);
        invertY = PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1;
        invertX = PlayerPrefs.GetInt(INVERT_X_KEY, 0) == 1;
        vibration = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
        autoStabilize = PlayerPrefs.GetInt(AUTO_STABILIZE_KEY, 1) == 1;
        controlScheme = PlayerPrefs.GetInt(CONTROL_SCHEME_KEY, 0);

        difficulty = PlayerPrefs.GetInt(DIFFICULTY_KEY, 1);
        maxWaves = PlayerPrefs.GetInt(MAX_WAVES_KEY, 0);
        flightModel = PlayerPrefs.GetInt(FLIGHT_MODEL_KEY, 0);
        speedUnit = PlayerPrefs.GetInt(SPEED_UNIT_KEY, 0);
        altitudeUnit = PlayerPrefs.GetInt(ALTITUDE_UNIT_KEY, 0);
        language = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);
        showHUD = PlayerPrefs.GetInt(SHOW_HUD_KEY, 1) == 1;
        showMinimap = PlayerPrefs.GetInt(SHOW_MINIMAP_KEY, 1) == 1;
        autoSave = PlayerPrefs.GetInt(AUTO_SAVE_KEY, 1) == 1;
        showTutorial = PlayerPrefs.GetInt(SHOW_TUTORIAL_KEY, 1) == 1;
        unlimitedAmmo = PlayerPrefs.GetInt(UNLIMITED_AMMO_KEY, 0) == 1;
        autoAimAssist = PlayerPrefs.GetInt(AUTO_AIM_KEY, 0) == 1;
        hudOpacity = PlayerPrefs.GetFloat(HUD_OPACITY_KEY, 0.9f);
        cameraView = PlayerPrefs.GetInt("CameraView", 0);
        cameraShake = PlayerPrefs.GetInt("CameraShake", 1) == 1;

        radarShowEnemies = PlayerPrefs.GetInt(RADAR_SHOW_ENEMIES_KEY, 1) == 1;
        radarShowMissiles = PlayerPrefs.GetInt(RADAR_SHOW_MISSILES_KEY, 1) == 1;
        radarMarkerSize = PlayerPrefs.GetFloat(RADAR_MARKER_SIZE_KEY, 1f);
        radarRange = PlayerPrefs.GetFloat(RADAR_RANGE_KEY, 4000f);

        ApplyAudio();
        ApplyGraphics();
    }

    private void ApplyGraphics()
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        QualitySettings.vSyncCount = vsync ? 1 : 0;

        ApplyFrameRate(frameRateLimit);

        ApplyAntiAliasing(antiAliasing);

        ApplyShadowQuality(shadowQuality);

        ApplyTextureQuality(textureQuality);

        ApplyRenderDistance();
        ApplyFOV();

        ApplyMotionBlur();
        ApplyBloom();

        Debug.Log(
            $"[Settings] Graphics applied: Quality={qualityIndex}, VSync={vsync}, FPS={frameRateLimit}, AA={antiAliasing}, Shadow={shadowQuality}, Texture={textureQuality}, MotionBlur={motionBlur}, Bloom={bloom}"
        );
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, masterVolume);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, musicVolume);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.SetFloat(ENGINE_VOLUME_KEY, engineVolume);
        PlayerPrefs.SetFloat(VOICE_VOLUME_KEY, voiceVolume);
        PlayerPrefs.SetInt(MUTE_KEY, isMuted ? 1 : 0);
        PlayerPrefs.SetInt(RADIO_CHATTER_KEY, radioChatter ? 1 : 0);

        PlayerPrefs.SetInt(RESOLUTION_KEY, resolutionIndex);
        PlayerPrefs.SetInt(QUALITY_KEY, qualityIndex);
        PlayerPrefs.SetInt(DISPLAY_MODE_KEY, displayModeIndex);
        PlayerPrefs.SetInt(FRAME_RATE_KEY, frameRateLimit);
        PlayerPrefs.SetInt(AA_KEY, antiAliasing);
        PlayerPrefs.SetInt(SHADOW_KEY, shadowQuality);
        PlayerPrefs.SetInt(TEXTURE_KEY, textureQuality);
        PlayerPrefs.SetInt(VSYNC_KEY, vsync ? 1 : 0);
        PlayerPrefs.SetFloat(RENDER_DISTANCE_KEY, renderDistance);
        PlayerPrefs.SetFloat(FOV_KEY, fieldOfView);
        PlayerPrefs.SetInt(MOTION_BLUR_KEY, motionBlur ? 1 : 0);
        PlayerPrefs.SetInt(BLOOM_KEY, bloom ? 1 : 0);
        PlayerPrefs.SetInt("GForceEffects", gforceEffects ? 1 : 0);

        PlayerPrefs.SetFloat(SENSITIVITY_KEY, mouseSensitivity);
        PlayerPrefs.SetFloat(DEADZONE_KEY, joystickDeadzone);
        PlayerPrefs.SetInt(INVERT_Y_KEY, invertY ? 1 : 0);
        PlayerPrefs.SetInt(INVERT_X_KEY, invertX ? 1 : 0);
        PlayerPrefs.SetInt(VIBRATION_KEY, vibration ? 1 : 0);
        PlayerPrefs.SetInt(AUTO_STABILIZE_KEY, autoStabilize ? 1 : 0);
        PlayerPrefs.SetInt(CONTROL_SCHEME_KEY, controlScheme);

        PlayerPrefs.SetInt(DIFFICULTY_KEY, difficulty);
        PlayerPrefs.SetInt(MAX_WAVES_KEY, maxWaves);
        PlayerPrefs.SetInt(FLIGHT_MODEL_KEY, flightModel);
        PlayerPrefs.SetInt(SPEED_UNIT_KEY, speedUnit);
        PlayerPrefs.SetInt(ALTITUDE_UNIT_KEY, altitudeUnit);
        PlayerPrefs.SetInt(LANGUAGE_KEY, language);
        PlayerPrefs.SetInt(SHOW_HUD_KEY, showHUD ? 1 : 0);
        PlayerPrefs.SetInt(SHOW_MINIMAP_KEY, showMinimap ? 1 : 0);
        PlayerPrefs.SetInt(AUTO_SAVE_KEY, autoSave ? 1 : 0);
        PlayerPrefs.SetInt(SHOW_TUTORIAL_KEY, showTutorial ? 1 : 0);
        PlayerPrefs.SetInt(UNLIMITED_AMMO_KEY, unlimitedAmmo ? 1 : 0);
        PlayerPrefs.SetInt(AUTO_AIM_KEY, autoAimAssist ? 1 : 0);
        PlayerPrefs.SetFloat(HUD_OPACITY_KEY, hudOpacity);
        PlayerPrefs.SetInt("CameraView", cameraView);
        PlayerPrefs.SetInt("CameraShake", cameraShake ? 1 : 0);

        PlayerPrefs.SetInt(RADAR_SHOW_ENEMIES_KEY, radarShowEnemies ? 1 : 0);
        PlayerPrefs.SetInt(RADAR_SHOW_MISSILES_KEY, radarShowMissiles ? 1 : 0);
        PlayerPrefs.SetFloat(RADAR_MARKER_SIZE_KEY, radarMarkerSize);
        PlayerPrefs.SetFloat(RADAR_RANGE_KEY, radarRange);

        PlayerPrefs.Save();
        Debug.Log("Settings saved!");
    }

    private void ApplyAudio()
    {
        AudioListener.volume = isMuted ? 0f : masterVolume;

        if (audioMixer != null)
        {
            TrySetMixerVolume("MasterVolume", isMuted ? 0f : masterVolume);
            TrySetMixerVolume("MusicVolume", musicVolume);
            TrySetMixerVolume("SFXVolume", sfxVolume);
            TrySetMixerVolume("EngineVolume", engineVolume);

            float actualVoiceVolume = radioChatter ? voiceVolume : 0f;
            TrySetMixerVolume("VoiceVolume", actualVoiceVolume);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.UpdateVolumes(
                masterVolume,
                musicVolume,
                sfxVolume,
                engineVolume,
                voiceVolume,
                radioChatter
            );
        }

        Debug.Log(
            $"[Settings] Audio applied: Master={masterVolume:F2}, Music={musicVolume:F2}, SFX={sfxVolume:F2}, Voice={voiceVolume:F2}, RadioChatter={radioChatter}, Muted={isMuted}"
        );
    }

    private void TrySetMixerVolume(string parameterName, float linear01)
    {
        if (audioMixer == null)
            return;
        var clamped = Mathf.Clamp01(linear01);
        var db = clamped <= 0.0001f ? -80f : Mathf.Log10(clamped) * 20f;
        audioMixer.SetFloat(parameterName, db);
    }

    private void ApplyFrameRate(int index)
    {
        int[] rates = { -1, 30, 60, 120, 144 };
        Application.targetFrameRate = rates[Mathf.Clamp(index, 0, rates.Length - 1)];
    }

    private void ApplyAntiAliasing(int index)
    {
        antiAliasing = index;
        int[] aaLevels = { 0, 2, 4, 8 };
        QualitySettings.antiAliasing = aaLevels[Mathf.Clamp(index, 0, aaLevels.Length - 1)];
    }

    private void ApplyShadowQuality(int index)
    {
        shadowQuality = index;
        ShadowQuality[] qualities =
        {
            ShadowQuality.Disable,
            ShadowQuality.HardOnly,
            ShadowQuality.All,
            ShadowQuality.All,
            ShadowQuality.All,
        };
        QualitySettings.shadows = qualities[Mathf.Clamp(index, 0, qualities.Length - 1)];
    }

    private void ApplyTextureQuality(int index)
    {
        textureQuality = index;
        QualitySettings.globalTextureMipmapLimit = Mathf.Clamp(index, 0, 2);
    }

    private void ApplyRenderDistance()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.farClipPlane = renderDistance;
        }
    }

    private void ApplyFOV()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.fieldOfView = fieldOfView;
        }
    }

    private void ApplyMotionBlur()
    {
        Debug.Log(
            $"[Settings] Motion Blur setting: {(motionBlur ? "enabled" : "disabled")} (requires URP for visual effect)"
        );
    }

    private void ApplyBloom()
    {
        Debug.Log(
            $"[Settings] Bloom setting: {(bloom ? "enabled" : "disabled")} (requires URP for visual effect)"
        );
    }

    private void ApplyResolution()
    {
        if (availableResolutions == null || resolutionIndex >= availableResolutions.Count)
            return;

        var res = availableResolutions[resolutionIndex];
        FullScreenMode mode = displayModeIndex switch
        {
            0 => FullScreenMode.ExclusiveFullScreen,
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.FullScreenWindow,
        };

        Screen.SetResolution(res.width, res.height, mode);
    }

    private void UpdateAllTexts()
    {
        UpdateVolumeTexts();
        UpdateSensitivityText();
        UpdateDeadzoneText();
        UpdateRenderDistanceText();
        UpdateFOVText();
        UpdateMaxWavesText();
        UpdateRadarMarkerSizeText();
        UpdateRadarRangeText();
    }

    private void UpdateVolumeTexts()
    {
        if (masterVolumeValue != null)
            masterVolumeValue.text = $"{(masterVolume * 100):F0}%";
        if (musicVolumeValue != null)
            musicVolumeValue.text = $"{(musicVolume * 100):F0}%";
        if (sfxVolumeValue != null)
            sfxVolumeValue.text = $"{(sfxVolume * 100):F0}%";
        if (engineVolumeValue != null)
            engineVolumeValue.text = $"{(engineVolume * 100):F0}%";
        if (voiceVolumeValue != null)
            voiceVolumeValue.text = $"{(voiceVolume * 100):F0}%";
    }

    private void UpdateSensitivityText()
    {
        if (sensitivityValue != null)
            sensitivityValue.text = $"{mouseSensitivity:F1}";
    }

    private void UpdateDeadzoneText()
    {
        if (deadzoneValue != null)
            deadzoneValue.text = $"{(joystickDeadzone * 100):F0}%";
    }

    private void UpdateRenderDistanceText()
    {
        if (renderDistanceValue != null)
            renderDistanceValue.text = $"{renderDistance:F0}m";
    }

    private void UpdateFOVText()
    {
        if (fovValue != null)
            fovValue.text = $"{fieldOfView:F0}";
    }

    private void UpdateMaxWavesText()
    {
        if (maxWavesValue != null)
        {
            maxWavesValue.text = maxWaves == 0 ? "All" : maxWaves.ToString();
        }
    }

    private void UpdateRadarMarkerSizeText()
    {
        if (radarMarkerSizeValue != null)
            radarMarkerSizeValue.text = $"{radarMarkerSize:F1}x";
    }

    private void UpdateRadarRangeText()
    {
        if (radarRangeValue != null)
            radarRangeValue.text = $"{radarRange:F0}m";
    }

    public void OpenSettings()
    {
        isSettingsOpen = true;
        SetSettingsVisible(true);
        SwitchTab(0);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuOpen();
        }
    }

    public void ApplyAndSave()
    {
        ApplyResolution();
        SaveSettings();
        Debug.Log("Settings applied and saved!");
    }

    public void BackToPreviousMenu()
    {
        SaveSettings();
        SetSettingsVisible(false);
        isSettingsOpen = false;

        if (mainMenuController != null && !mainMenuController.IsGameStarted())
        {
            mainMenuController.OnBackToMenu();
        }
        else if (pauseMenuController != null)
        {
            pauseMenuController.ShowPauseMenu();
        }
    }

    private static bool IsEscapePressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            return true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }

    public void ResetToDefaults()
    {
        masterVolume = 0.9f;
        musicVolume = 0.9f;
        sfxVolume = 0.9f;
        engineVolume = 0.9f;
        voiceVolume = 0.9f;
        isMuted = false;

        vsync = true;
        motionBlur = false;
        bloom = true;
        renderDistance = 5000f;
        fieldOfView = 75f;

        mouseSensitivity = 1f;
        joystickDeadzone = 0.1f;
        invertY = false;
        invertX = false;
        vibration = true;
        autoStabilize = true;

        maxWaves = 0;
        showHUD = true;
        showMinimap = true;
        autoSave = true;
        showTutorial = true;

        radarShowEnemies = true;
        radarShowMissiles = true;
        radarMarkerSize = 1f;
        radarRange = 4000f;

        InitializeUI();
        ApplyAudio();
        Debug.Log("Settings reset to defaults!");
    }

    private void SetSettingsVisible(bool visible)
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            if (root != uiDocument.rootVisualElement)
            {
                Debug.Log("[Settings] Detected root element change, re-binding...");
                root = uiDocument.rootVisualElement;
                BindUIElements();
                RegisterCallbacks();
            }
        }

        var displayStyle = visible ? DisplayStyle.Flex : DisplayStyle.None;

        if (root != null)
        {
            root.style.display = displayStyle;

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

        var settingsRoot = root?.Q<VisualElement>("root");
        if (settingsRoot != null)
        {
            settingsRoot.style.display = displayStyle;
        }
    }

    private void EnsureReferences()
    {
        if (mainMenuController == null)
            mainMenuController = FindFirstObjectByType<MainMenuController>();
        if (pauseMenuController == null)
            pauseMenuController = FindFirstObjectByType<PauseMenuController>();
    }

    public float MouseSensitivity => mouseSensitivity;
    public bool InvertY => invertY;
    public bool InvertX => invertX;
    public float JoystickDeadzone => joystickDeadzone;
    public bool AutoStabilize => autoStabilize;
    public int Difficulty => difficulty;
    public int MaxWaves => maxWaves;
    public int FlightModel => flightModel;
    public int SpeedUnit => speedUnit;
    public int AltitudeUnit => altitudeUnit;
    public bool ShowHUD => showHUD;
    public bool ShowMinimap => showMinimap;
    public float FieldOfView => fieldOfView;
    public float RenderDistance => renderDistance;

    public bool RadarShowEnemies => radarShowEnemies;
    public bool RadarShowMissiles => radarShowMissiles;
    public float RadarMarkerSize => radarMarkerSize;
    public float RadarRange => radarRange;

    #region Save Data Handlers

    private void OnExportSave()
    {
        if (GameManager.Instance != null)
        {
            bool success = GameManager.Instance.ExportToFile();
            if (success && savePathLabel != null)
            {
                savePathLabel.text = $" Exported to: {GameManager.GetDefaultSavePath()}";
            }
        }
        else
        {
            Debug.LogWarning("GameManager not found, cannot export save data");
        }
    }

    private void OnImportSave()
    {
        if (GameManager.Instance != null)
        {
            bool success = GameManager.Instance.ImportFromFile();
            if (success && savePathLabel != null)
            {
                savePathLabel.text = " Save data imported successfully!";
            }
            else if (savePathLabel != null)
            {
                savePathLabel.text = " Failed to import save data. Check the file path.";
            }
        }
        else
        {
            Debug.LogWarning("GameManager not found, cannot import save data");
        }
    }

    private void OnResetProgress()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetAllStats();

            StoryIntroController.ResetStoryFlag();

            if (savePathLabel != null)
            {
                savePathLabel.text = " All progress has been reset!";
            }

            Debug.Log("All player progress has been reset");
        }
    }

    private void NotifyPlayerControllerOfChanges()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            playerController.UpdateControlSettings();
        }
    }

    private void NotifyPlaneOfUnlimitedAmmo()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
        {
            var plane = playerController.GetComponent<Plane>();
            if (plane == null)
            {
                plane = playerController.GetComponentInChildren<Plane>();
            }

            if (plane != null)
            {
                plane.SetUnlimitedAmmo(unlimitedAmmo);
            }
        }
    }

    #endregion
}
