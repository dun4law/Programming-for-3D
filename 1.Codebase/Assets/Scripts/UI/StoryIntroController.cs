using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StoryIntroController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField]
    private UIDocument uiDocument;

    [Header("Story Images (loaded from Resources/Story/)")]
    private List<string> storyImageNames = new List<string>
    {
        "story_city_dusk",
        "story_teacher_chan_broadcast",
        "story_void_history",
        "story_ark_command",
        "story_dimensional_rift",
        "story_civilians_evacuate",
        "story_gloria_command",
        "story_classified_folder",
        "story_radar_screen",
        "story_donkey_defense",
        "story_one_hangar",
        "story_f15_hangar",
        "story_weapons_check",
        "story_gloria_hologram",
        "story_mission_objectives",
        "story_pilot_memories",
        "story_pilot_suit",
        "story_walk_to_plane",
        "story_cockpit_startup",
        "story_radio_check",
        "story_tower_clearance",
        "story_afterburner",
        "story_runway_accel",
        "story_takeoff",
        "story_formation_flight",
        "story_night_sky_approach",
        "story_rift_distant",
        "story_enemy_contact",
        "story_gloria_final",
        "story_battle_begins",
    };

    private List<Texture2D> loadedImages = new List<Texture2D>();

    [Header("Settings")]
    [SerializeField]
    private string gameplaySceneName = "Main";

    [SerializeField]
    private string loadingSceneName = "Loading";

    private VisualElement root;
    private VisualElement storyBackground;
    private VisualElement progressFill;
    private Label pageNumberLabel;
    private Label storyTitle;
    private Label storyText;
    private Button prevButton;
    private Button skipButton;
    private Button nextButton;
    private VisualElement continuePrompt;
    private Label nextButtonText;
    private Toggle neverShowAgainToggle;

    private List<StoryPage> storyPages;
    private int currentPageIndex = 0;

    public System.Action OnStoryComplete;

    private const string PREFS_STORY_SHOWN = "StoryIntroShown";

    [System.Serializable]
    public class StoryPage
    {
        public string title;
        public string text;
        public int imageIndex;
    }

    void Awake()
    {
        LoadStoryImages();
        InitializeStoryContent();
    }

    void LoadStoryImages()
    {
        loadedImages.Clear();
        foreach (var imageName in storyImageNames)
        {
            var texture = Resources.Load<Texture2D>($"Story/{imageName}");
            if (texture != null)
            {
                loadedImages.Add(texture);
                Debug.Log($"[StoryIntro] Loaded image: {imageName}");
            }
            else
            {
                Debug.LogWarning($"[StoryIntro] Failed to load image: Story/{imageName}");
                loadedImages.Add(null);
            }
        }
        Debug.Log($"[StoryIntro] Loaded {loadedImages.Count} images");
    }

    void OnEnable()
    {
        if (uiDocument == null)
            return;

        root = uiDocument.rootVisualElement?.Q<VisualElement>("root");
        if (root == null)
            return;

        root.style.display = DisplayStyle.None;

        SetupUI();
    }

    void InitializeStoryContent()
    {
        storyPages = new List<StoryPage>
        {
            new StoryPage
            {
                title = "HONG KONG, 2097",
                text =
                    "The neon-lit metropolis of Hong Kong stands as humanity's last beacon of hope "
                    + "in an increasingly unstable world. Victoria Harbour glitters with the lights "
                    + "of a million souls, unaware of the darkness gathering beyond the horizon.\n\n"
                    + "For decades, scientists warned of dimensional instability. For decades, they were silenced.",
                imageIndex = 0,
            },
            new StoryPage
            {
                title = "EMERGENCY BROADCAST",
                text =
                    "\"This is Teacher Chan with an emergency bulletin from the Global Defense Network.\"\n\n"
                    + "The famous idol's voice is impossibly calm, almost hypnotic. "
                    + "\"Citizens are advised to remain calm. There is no cause for alarm. "
                    + "Whatever you see in the northern sky is simply... atmospheric phenomena.\n\n"
                    + "\"Remember: Let go of your fears. Embrace positivity. Everything will be fine.\""
                    + "\n\n...Something about her smile doesn't reach her eyes.",
                imageIndex = 1,
            },
            new StoryPage
            {
                title = "ANCIENT WARNINGS",
                text =
                    "This is not the first time the Void has tried to break through.\n\n"
                    + "Ancient cave paintings warned of 'Rift Walkers'—beings that emerge when "
                    + "humanity's collective delusion reaches critical mass. The ancients understood "
                    + "that forced optimism, manufactured hope, accelerates the decay.\n\n"
                    + "The Global Defense Network dismissed these findings as 'too negative.'",
                imageIndex = 2,
            },
            new StoryPage
            {
                title = "STRATEGIC COMMAND",
                text =
                    "Deep within PHOENIX Base, Director ARK works alone at his terminal. "
                    + "He hasn't shared his analysis with anyone—he never does.\n\n"
                    + "\"The Void Entropy readings are off the charts,\" he mutters to himself, "
                    + "fingers dancing across displays. \"But they wouldn't understand. They never do.\"\n\n"
                    + "He issues commands to systems he alone controls, through channels only he monitors.",
                imageIndex = 3,
            },
            new StoryPage
            {
                title = "THE BREACH",
                text =
                    "At 1847 hours, the sky tears open.\n\n"
                    + "A swirling vortex of purple and crimson energy rips through the atmosphere. "
                    + "From within its depths, shadows of hostile aircraft emerge.\n\n"
                    + "High Command's official statement: \"This is a wonderful opportunity to "
                    + "demonstrate our unity and positive spirit.\"",
                imageIndex = 4,
            },
            new StoryPage
            {
                title = "EVACUATION",
                text =
                    "On the streets below, sirens wail as emergency protocols activate.\n\n"
                    + "Civilians pour from buildings, but the public address systems play "
                    + "Teacher Chan's soothing music instead of evacuation instructions. "
                    + "\"Stay positive,\" the speakers croon. \"Fear only attracts more negativity.\"\n\n"
                    + "Some people stand frozen, smiling vacantly at the rift. Others run.",
                imageIndex = 5,
            },
            new StoryPage
            {
                title = "HIGH COMMAND ASSEMBLY",
                text =
                    "Fleet Admiral Gloria stands before the assembled commanders, her white robes "
                    + "pristine, her smile serene. The golden staff in her hand gleams.\n\n"
                    + "\"I sense some... negativity in this room,\" she says softly. "
                    + "\"Your doubt wounds me deeply. Have I not given everything for this cause?\"\n\n"
                    + "Commanders shift uncomfortably. Questioning her feels like kicking a puppy. "
                    + "\"Operation IRONCLAD is authorized. Any objections would be... hurtful.\"",
                imageIndex = 6,
            },
            new StoryPage
            {
                title = "CLASSIFIED BRIEFING",
                text =
                    "You receive the mission folder. Inside, satellite imagery shows the rift "
                    + "and estimated enemy numbers—crossed out and replaced with 'POSITIVE OUTCOME EXPECTED.'\n\n"
                    + "The actual tactical data is buried in appendices. The cover sheet reads: "
                    + "\"Remember, your attitude determines your altitude! - Admiral Gloria\"\n\n"
                    + "You read between the lines. Failure means death for millions.",
                imageIndex = 7,
            },
            new StoryPage
            {
                title = "HOSTILE DETECTION",
                text =
                    "The tactical display erupts with contacts—dozens of hostile signatures.\n\n"
                    + "Director ARK's voice crackles over command frequency. He's already made "
                    + "decisions without consulting anyone: \"Execute Pattern Omega-Seven. "
                    + "I've already uploaded the parameters. Just follow the protocol.\"\n\n"
                    + "There's no explanation of what Pattern Omega-Seven actually entails. There never is.",
                imageIndex = 8,
            },
            new StoryPage
            {
                title = "GROUND DEFENSE",
                text =
                    "Colonel Donkey strikes a heroic pose before the cameras, cape billowing "
                    + "in the artificial wind of a nearby fan.\n\n"
                    + "\"Fear not, citizens! I, the great Donkey, shall vanquish these dragons— "
                    + "er, I mean, enemy aircraft! Like the heroes of legend!\" He unsheathes a "
                    + "ceremonial sword that won't help against jets.\n\n"
                    + "Behind him, actual SAM operators exchange weary glances and do the real work.",
                imageIndex = 9,
            },
            new StoryPage
            {
                title = "WINGMAN",
                text =
                    "In the hangar, Lieutenant One waits beside his F-15. The quiet pilot "
                    + "gives you a knowing look—the only person here who seems to see reality.\n\n"
                    + "\"Ready when you are, lead,\" he says simply. There's so much more behind his eyes, "
                    + "but he won't say it. He never does. Gloria's tolerance doesn't extend to him.\n\n"
                    + "He's the only competent one here. And he knows better than to speak up.",
                imageIndex = 10,
            },
            new StoryPage
            {
                title = "YOUR AIRCRAFT",
                text =
                    "The F-15 Eagle stands ready—a masterpiece of warfare perfected over generations.\n\n"
                    + "Ground crew complete their checks. Unlike command, they speak in realities: "
                    + "fuel levels, weapon status, engine temps. No inspirational quotes.\n\n"
                    + "Armed with missiles and cannon, this bird has an undefeated record. "
                    + "You'll need every advantage. Command's 'positive thinking' won't stop missiles.",
                imageIndex = 11,
            },
            new StoryPage
            {
                title = "WEAPONS LOADOUT",
                text =
                    "A weapons technician gives the final inspection. She speaks quietly:\n\n"
                    + "\"Four Sidewinders, two AMRAAMs, full cannon. She's ready to hunt, sir.\" "
                    + "A pause. \"...Word of advice? Ignore the command chatter. Just fly.\"\n\n"
                    + "She's seen too many pilots distracted by Gloria's sermons and ARK's "
                    + "impenetrable protocols. The ones who survived learned to think for themselves.",
                imageIndex = 12,
            },
            new StoryPage
            {
                title = "ADMIRAL'S ADDRESS",
                text =
                    "Admiral Gloria's hologram materializes, her smile radiant and unsettling.\n\n"
                    + "\"My dear pilots,\" she coos. \"I know some of you harbor doubts. "
                    + "Those doubts hurt me personally. When you doubt, you send negative energy "
                    + "into the universe...\"\n\n"
                    + "She sighs as if bearing tremendous burden. \"But I forgive you. "
                    + "Fly with positive hearts, and the universe will protect you.\"\n\n"
                    + "Lieutenant One's jaw tightens almost imperceptibly.",
                imageIndex = 13,
            },
            new StoryPage
            {
                title = "MISSION PARAMETERS",
                text =
                    "The tactical display shows objectives, filtered through command's lens:\n\n"
                    + "PRIMARY: \"Embrace the opportunity to defend Sector 7 with joy!\"\n"
                    + "SECONDARY: \"Transform hostile energy into peaceful resolution!\"\n"
                    + "TERTIARY: \"Protect infrastructure through collective positive intention!\"\n\n"
                    + "Translated: Kill the enemy before they kill civilians. The real mission, "
                    + "buried beneath layers of toxic optimism.",
                imageIndex = 14,
            },
            new StoryPage
            {
                title = "PERSONAL STAKES",
                text =
                    "In a quiet moment, you pause at your locker.\n\n"
                    + "A photograph is taped inside—faces of those you're fighting for. "
                    + "Real people. Not the abstract 'positive energy' Gloria preaches about.\n\n"
                    + "This is why you fly. Not for medals, not for speeches. "
                    + "For them. Despite command, not because of it.",
                imageIndex = 15,
            },
            new StoryPage
            {
                title = "PREPARATION",
                text =
                    "In the locker room, you don your flight suit. The G-suit's weight is familiar.\n\n"
                    + "Overhead speakers play Teacher Chan's latest single: "
                    + "\"Let go of your worries, let go of your pain, everything happens for a reason...\"\n\n"
                    + "The music is meant to be calming. Instead, it feels like a countdown. "
                    + "You tune it out and focus on the mission.",
                imageIndex = 16,
            },
            new StoryPage
            {
                title = "THE WALK",
                text =
                    "The tarmac stretches before you. Your F-15 waits at the end.\n\n"
                    + "A propaganda screen shows Gloria's face: \"Your attitude determines your altitude!\" "
                    + "Next to it, Colonel Donkey poses with his ceremonial sword, captioned: "
                    + "\"A TRUE HERO DEFENDS THE REALM!\"\n\n"
                    + "You walk past. There's a real war to fight.",
                imageIndex = 17,
            },
            new StoryPage
            {
                title = "COCKPIT INITIALIZATION",
                text =
                    "You settle into the ejection seat. Canopy descends.\n\n"
                    + "Displays flicker to life—engine status, weapons, navigation. "
                    + "A notification pops up: \"MANDATORY MORALE MESSAGE FROM ADM. GLORIA\" "
                    + "You dismiss it.\n\n"
                    + "\"Engine start... Systems check... All green.\"\n\n"
                    + "At least the aircraft doesn't lecture you about positivity.",
                imageIndex = 18,
            },
            new StoryPage
            {
                title = "COMM CHECK",
                text =
                    "\"Phoenix Lead to Phoenix Two, radio check.\"\n\n"
                    + "One's voice comes back: \"Loud and clear.\" A pause, then quieter: "
                    + "\"...Ignore the command chatter. I've got your six. For real.\"\n\n"
                    + "It's the closest he'll come to acknowledging the dysfunction. "
                    + "You exchange thumbs up. The bond between you isn't built on speeches— "
                    + "it's built on shared survival.",
                imageIndex = 19,
            },
            new StoryPage
            {
                title = "TOWER AUTHORIZATION",
                text =
                    "ATC cuts through the radio:\n\n"
                    + "\"Phoenix Flight, cleared for departure. Godspeed—\" Static interrupts, "
                    + "then ARK's voice: \"Disregard tower. Follow my uploaded flight plan exactly. "
                    + "Deviation will not be tolerated.\"\n\n"
                    + "The tower controller sighs off-mic. ARK's plans are inflexible and unexplained. "
                    + "But you know better than to argue. Just adapt when you're airborne.",
                imageIndex = 20,
            },
            new StoryPage
            {
                title = "IGNITION",
                text =
                    "Throttles forward. The twin engines roar, afterburners igniting.\n\n"
                    + "Gloria's voice intrudes on the radio: \"Remember, pilots—negative thoughts "
                    + "create negative outcomes! fly with love in your hearts!\"\n\n"
                    + "You release brakes. let the roar of the engines drown out everything else.\n\n"
                    + "The world explodes into motion. Finally, blessed silence.",
                imageIndex = 21,
            },
            new StoryPage
            {
                title = "ACCELERATION",
                text =
                    "G-forces press you into the seat. Runway lights blur.\n\n"
                    + "Speed builds—150, 180, 210—the nose lifts, gear breaks contact, "
                    + "and suddenly you're climbing free. Up here, it's simpler. "
                    + "No speeches, no slogans, no guilt trips.\n\n"
                    + "\"Phoenix Lead, airborne.\" \"Phoenix Two, on your six.\"",
                imageIndex = 22,
            },
            new StoryPage
            {
                title = "ASCENT",
                text =
                    "The city falls away—a galaxy of lights against the dark earth. "
                    + "Millions of people down there, trusting their leaders.\n\n"
                    + "What they don't know: Gloria's 'positive energy' won't stop bullets. "
                    + "Donkey's heroic poses won't intercept missiles. ARK's plans exist in a vacuum.\n\n"
                    + "You and One. That's what stands between them and annihilation.",
                imageIndex = 23,
            },
            new StoryPage
            {
                title = "FORMATION",
                text =
                    "One slides into position on your wing, his F-15 a dark silhouette.\n\n"
                    + "\"Phoenix Two, in formation.\" A pause. \"Lead... if things go sideways up there, "
                    + "trust your instincts. Not the protocols.\"\n\n"
                    + "It's the most he's ever said. You understand. When command's plans fail— "
                    + "and they will—you'll have to improvise. Together.",
                imageIndex = 24,
            },
            new StoryPage
            {
                title = "APPROACH",
                text =
                    "The dimensional rift grows larger, its purple glow painting the clouds.\n\n"
                    + "ARK's voice interrupts: \"Maintain Pattern Omega-Seven. No deviations.\" "
                    + "Gloria's voice follows: \"Remember, your fear feeds the enemy! Smile!\"\n\n"
                    + "Your instruments fluctuate. The laws of physics seem uncertain here.\n\n"
                    + "You mute the command channel. Focus on the rift. Focus on surviving.",
                imageIndex = 25,
            },
            new StoryPage
            {
                title = "THE VOID AWAITS",
                text =
                    "The dimensional rift dominates the sky—a wound in reality, "
                    + "pulsing with energy that feels somehow... artificial.\n\n"
                    + "Scientists once theorized that collective human delusion could tear holes "
                    + "in dimensions. That manufactured hope, forced positivity, accelerates decay.\n\n"
                    + "Looking at the rift, you wonder if humanity's desperate optimism "
                    + "didn't just attract the Void—but created it.",
                imageIndex = 26,
            },
            new StoryPage
            {
                title = "CONTACT",
                text =
                    "\"BANDITS! Multiple contacts, bearing north-northeast!\"\n\n"
                    + "The radar scope lights up. Red diamonds appear on your HUD— "
                    + "each one an enemy that must be stopped.\n\n"
                    + "No more speeches. No more slogans. Just you, your wingman, and the kill.\n\n"
                    + "Sidewinder selected. The seeker head begins its deadly song.",
                imageIndex = 27,
            },
            new StoryPage
            {
                title = "FINAL WORDS",
                text =
                    "Gloria's face appears on your comm display, unbidden.\n\n"
                    + "\"Phoenix Flight, I believe in you! When you destroy those enemies, "
                    + "do it with love and forgiveness in your hearts! Transform their negative "
                    + "energy into positive light!\"\n\n"
                    + "You think of One's silence. The weapons tech's quiet advice. The ground crew's realism.\n\n"
                    + "The real heroes never needed speeches. You close the channel.",
                imageIndex = 28,
            },
            new StoryPage
            {
                title = "BATTLE STATIONS",
                text =
                    "Missiles launch. Contrails crisscross the sky. Explosions bloom.\n\n"
                    + "\"Phoenix Two, engaging! Fox Two!\"\n\n"
                    + "This is real. Not propaganda, not slogans, not ceremonial swords. "
                    + "Steel and fire and split-second decisions.\n\n"
                    + "You arm your weapons. The targeting reticle finds its first victim. "
                    + "Your finger tightens.\n\n"
                    + "Whatever happens next—it won't be decided by positive thinking.\n\n"
                    + "The battle for Earth begins NOW.",
                imageIndex = 29,
            },
        };
    }

    void SetupUI()
    {
        storyBackground = root.Q<VisualElement>("story-background");
        progressFill = root.Q<VisualElement>("progress-fill");
        pageNumberLabel = root.Q<Label>("page-number");
        storyTitle = root.Q<Label>("story-title");
        storyText = root.Q<Label>("story-text");
        prevButton = root.Q<Button>("prev-button");
        skipButton = root.Q<Button>("skip-button");
        nextButton = root.Q<Button>("next-button");
        continuePrompt = root.Q<VisualElement>("continue-prompt");

        if (nextButton != null)
        {
            nextButtonText = nextButton.Q<Label>(className: "nav-button-text");
        }

        if (prevButton != null)
            prevButton.clicked += OnPrevious;
        if (skipButton != null)
            skipButton.clicked += OnSkip;
        if (nextButton != null)
            nextButton.clicked += OnNext;

        if (continuePrompt != null)
        {
            continuePrompt.style.display = DisplayStyle.None;
        }

        neverShowAgainToggle = root.Q<Toggle>("never-show-again-toggle");
        if (neverShowAgainToggle != null)
        {
            neverShowAgainToggle.value = false;
        }
    }

    void Update()
    {
        if (root == null || root.style.display == DisplayStyle.None)
            return;

        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            if (
                UnityEngine.InputSystem.Keyboard.current.rightArrowKey.wasPressedThisFrame
                || UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame
                || UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame
            )
            {
                OnNext();
            }
            else if (UnityEngine.InputSystem.Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                OnPrevious();
            }
            else if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                OnSkip();
            }
        }
    }

    public static bool ShouldShowStory()
    {
        return PlayerPrefs.GetInt(PREFS_STORY_SHOWN, 0) == 0;
    }

    public static void MarkStoryAsShown()
    {
        PlayerPrefs.SetInt(PREFS_STORY_SHOWN, 1);
        PlayerPrefs.Save();
    }

    public static void ResetStoryFlag()
    {
        PlayerPrefs.DeleteKey(PREFS_STORY_SHOWN);
        PlayerPrefs.Save();
    }

    public void Show()
    {
        if (root == null)
            return;

        currentPageIndex = 0;
        root.style.display = DisplayStyle.Flex;
        UpdatePage();

        Time.timeScale = 1f;

        Debug.Log(
            $"[StoryIntro] Showing story intro - {storyPages.Count} pages, {loadedImages.Count} images loaded"
        );
    }

    public void Hide()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
    }

    void UpdatePage()
    {
        if (currentPageIndex < 0 || currentPageIndex >= storyPages.Count)
            return;

        var page = storyPages[currentPageIndex];

        if (storyTitle != null)
            storyTitle.text = page.title;
        if (storyText != null)
            storyText.text = page.text;
        if (pageNumberLabel != null)
            pageNumberLabel.text = $"{currentPageIndex + 1} / {storyPages.Count}";

        if (progressFill != null)
        {
            float progress = (float)(currentPageIndex + 1) / storyPages.Count * 100f;
            progressFill.style.width = new Length(progress, LengthUnit.Percent);
        }

        if (
            storyBackground != null
            && loadedImages != null
            && loadedImages.Count > 0
            && page.imageIndex >= 0
            && page.imageIndex < loadedImages.Count
        )
        {
            var texture = loadedImages[page.imageIndex];
            if (texture != null)
            {
                storyBackground.style.backgroundImage = new StyleBackground(texture);
            }
            else
            {
                storyBackground.style.backgroundImage = StyleKeyword.None;
                storyBackground.style.backgroundColor = new StyleColor(
                    new Color(0.1f, 0.15f, 0.25f, 1f)
                );
                Debug.LogWarning($"[StoryIntro] Texture null for index {page.imageIndex}");
            }
        }
        else
        {
            if (storyBackground != null)
            {
                storyBackground.style.backgroundImage = StyleKeyword.None;
                storyBackground.style.backgroundColor = new StyleColor(
                    new Color(0.05f, 0.1f, 0.15f, 1f)
                );
            }
        }

        if (prevButton != null)
        {
            prevButton.style.visibility =
                currentPageIndex > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        if (nextButton != null)
        {
            if (currentPageIndex >= storyPages.Count - 1)
            {
                if (nextButtonText != null)
                    nextButtonText.text = "START MISSION";
                nextButton.AddToClassList("start-mission-button");
            }
            else
            {
                if (nextButtonText != null)
                    nextButtonText.text = "NEXT";
                nextButton.RemoveFromClassList("start-mission-button");
            }
        }

        if (continuePrompt != null)
        {
            continuePrompt.style.display =
                currentPageIndex >= storyPages.Count - 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    void OnPrevious()
    {
        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            Debug.Log(
                $"[StoryIntro] Previous page -> {currentPageIndex + 1}/{storyPages.Count}: {storyPages[currentPageIndex].title}"
            );
            UpdatePage();
            PlayPageSound();
        }
    }

    void OnNext()
    {
        if (currentPageIndex < storyPages.Count - 1)
        {
            currentPageIndex++;
            Debug.Log(
                $"[StoryIntro] Next page -> {currentPageIndex + 1}/{storyPages.Count}: {storyPages[currentPageIndex].title}"
            );
            UpdatePage();
            PlayPageSound();
        }
        else
        {
            Debug.Log("[StoryIntro] Reached end of story, completing...");
            CompleteStory();
        }
    }

    void OnSkip()
    {
        Debug.Log("[StoryIntro] Skip button pressed, completing story...");
        CompleteStory();
    }

    void CompleteStory()
    {
        if (neverShowAgainToggle != null && neverShowAgainToggle.value)
        {
            MarkStoryAsShown();
            Debug.Log(
                "[StoryIntro] Player selected 'Never show again' - story disabled for future rounds"
            );
        }
        else
        {
            Debug.Log("[StoryIntro] Story will show again next round (toggle not checked)");
        }

        Hide();

        OnStoryComplete?.Invoke();

        StartGame();
    }

    void StartGame()
    {
        Debug.Log("[StoryIntro] Story complete, starting game...");

        MainMenuController.ShouldStartImmediately = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetSessionStats();
        }

        Time.timeScale = 1f;

        if (
            !string.IsNullOrWhiteSpace(loadingSceneName)
            && Application.CanStreamedLevelBeLoaded(loadingSceneName)
        )
        {
            LoadingSceneState.TargetSceneName = gameplaySceneName;
            SceneManager.LoadScene(loadingSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
    }

    void PlayPageSound()
    {
        if (AudioManager.Instance != null)
        {
            Debug.Log("[StoryIntro] Playing page turn sound");
            AudioManager.Instance.PlayButtonClick();
        }
        else
        {
            Debug.LogWarning("[StoryIntro] AudioManager not available for page sound");
        }
    }

    void OnDestroy()
    {
        if (prevButton != null)
            prevButton.clicked -= OnPrevious;
        if (skipButton != null)
            skipButton.clicked -= OnSkip;
        if (nextButton != null)
            nextButton.clicked -= OnNext;
    }
}
