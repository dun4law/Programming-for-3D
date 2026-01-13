using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class HowToPlayController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField]
    private UIDocument uiDocument;

    [Header("Input Actions")]
    [SerializeField]
    private InputActionAsset inputActions;

    [Header("References")]
    [SerializeField]
    private MainMenuController mainMenuController;

    private VisualElement root;
    private Button backButton;
    private Button resetButton;
    private VisualElement rebindOverlay;
    private Label rebindActionName;
    private Button cancelRebindButton;

    private Button btnControls;
    private Button btnController;
    private Button btnTips;
    private Button btnMission;
    private Button btnCharacters;
    private Button btnHud;
    private Button btnWarnings;
    private Button btnCombat;
    private Button btnScoring;
    private Button btnFeatures;
    private Button btnAircraft;
    private Button btnTactics;
    private Button btnFuture;

    private VisualElement sectionControls;
    private VisualElement sectionController;
    private VisualElement sectionTips;
    private VisualElement sectionMission;
    private VisualElement sectionCharacters;
    private VisualElement sectionHud;
    private VisualElement sectionWarnings;
    private VisualElement sectionCombat;
    private VisualElement sectionScoring;
    private VisualElement sectionFeatures;
    private VisualElement sectionAircraft;
    private VisualElement sectionTactics;
    private VisualElement sectionFuture;

    private Dictionary<string, Button> keyButtons = new Dictionary<string, Button>();
    private Dictionary<string, KeyBinding> keyBindings = new Dictionary<string, KeyBinding>();

    private InputActionRebindingExtensions.RebindingOperation currentRebindOperation;
    private string currentRebindKey;

    private const string PREFS_PREFIX = "KeyBinding_";

    [System.Serializable]
    public class KeyBinding
    {
        public string actionName;
        public string compositePart;
        public string defaultKey;
        public string currentKey;
    }

    void Awake()
    {
        if (mainMenuController == null)
        {
            mainMenuController = FindAnyObjectByType<MainMenuController>();
        }
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
        LoadKeyBindings();
    }

    void SetupUI()
    {
        backButton = root.Q<Button>("back-button");
        resetButton = root.Q<Button>("reset-button");

        if (backButton != null)
            backButton.clicked += OnBackClicked;
        if (resetButton != null)
            resetButton.clicked += OnResetClicked;

        rebindOverlay = root.Q<VisualElement>("rebind-overlay");
        rebindActionName = root.Q<Label>("rebind-action-name");
        cancelRebindButton = root.Q<Button>("cancel-rebind");

        if (cancelRebindButton != null)
            cancelRebindButton.clicked += OnCancelRebind;

        btnControls = root.Q<Button>("btn-controls");
        btnController = root.Q<Button>("btn-controller");
        btnTips = root.Q<Button>("btn-tips");
        btnMission = root.Q<Button>("btn-mission");
        btnCharacters = root.Q<Button>("btn-characters");
        btnHud = root.Q<Button>("btn-hud");
        btnWarnings = root.Q<Button>("btn-warnings");
        btnCombat = root.Q<Button>("btn-combat");
        btnScoring = root.Q<Button>("btn-scoring");
        btnFeatures = root.Q<Button>("btn-features");
        btnAircraft = root.Q<Button>("btn-aircraft");
        btnTactics = root.Q<Button>("btn-tactics");
        btnFuture = root.Q<Button>("btn-future");

        sectionControls = root.Q<VisualElement>("section-controls");
        sectionController = root.Q<VisualElement>("section-controller");
        sectionTips = root.Q<VisualElement>("section-tips");
        sectionMission = root.Q<VisualElement>("section-mission");
        sectionCharacters = root.Q<VisualElement>("section-characters");
        sectionHud = root.Q<VisualElement>("section-hud");
        sectionWarnings = root.Q<VisualElement>("section-warnings");
        sectionCombat = root.Q<VisualElement>("section-combat");
        sectionScoring = root.Q<VisualElement>("section-scoring");
        sectionFeatures = root.Q<VisualElement>("section-features");
        sectionAircraft = root.Q<VisualElement>("section-aircraft");
        sectionTactics = root.Q<VisualElement>("section-tactics");
        sectionFuture = root.Q<VisualElement>("section-future");

        if (btnControls != null)
            btnControls.clicked += () => SwitchSection("controls");
        if (btnController != null)
            btnController.clicked += () => SwitchSection("controller");
        if (btnTips != null)
            btnTips.clicked += () => SwitchSection("tips");
        if (btnMission != null)
            btnMission.clicked += () => SwitchSection("mission");
        if (btnCharacters != null)
            btnCharacters.clicked += () => SwitchSection("characters");
        if (btnHud != null)
            btnHud.clicked += () => SwitchSection("hud");
        if (btnWarnings != null)
            btnWarnings.clicked += () => SwitchSection("warnings");
        if (btnCombat != null)
            btnCombat.clicked += () => SwitchSection("combat");
        if (btnScoring != null)
            btnScoring.clicked += () => SwitchSection("scoring");
        if (btnFeatures != null)
            btnFeatures.clicked += () => SwitchSection("features");
        if (btnAircraft != null)
            btnAircraft.clicked += () => SwitchSection("aircraft");
        if (btnTactics != null)
            btnTactics.clicked += () => SwitchSection("tactics");
        if (btnFuture != null)
            btnFuture.clicked += () => SwitchSection("future");

        SwitchSection("controls");

        InitializeKeyBindings();

        SetupKeyButtons();
    }

    void InitializeKeyBindings()
    {
        keyBindings["throttle-up"] = new KeyBinding
        {
            actionName = "Throttle",
            compositePart = "positive",
            defaultKey = "Shift",
        };
        keyBindings["throttle-down"] = new KeyBinding
        {
            actionName = "Throttle",
            compositePart = "negative",
            defaultKey = "Ctrl",
        };
        keyBindings["pitch-up"] = new KeyBinding
        {
            actionName = "RollPitch",
            compositePart = "up",
            defaultKey = "W",
        };
        keyBindings["pitch-down"] = new KeyBinding
        {
            actionName = "RollPitch",
            compositePart = "down",
            defaultKey = "S",
        };
        keyBindings["roll-left"] = new KeyBinding
        {
            actionName = "RollPitch",
            compositePart = "left",
            defaultKey = "A",
        };
        keyBindings["roll-right"] = new KeyBinding
        {
            actionName = "RollPitch",
            compositePart = "right",
            defaultKey = "D",
        };
        keyBindings["yaw-left"] = new KeyBinding
        {
            actionName = "Yaw",
            compositePart = "negative",
            defaultKey = "Q",
        };
        keyBindings["yaw-right"] = new KeyBinding
        {
            actionName = "Yaw",
            compositePart = "positive",
            defaultKey = "E",
        };
        keyBindings["flaps"] = new KeyBinding
        {
            actionName = "ToggleFlaps",
            compositePart = null,
            defaultKey = "F",
        };

        keyBindings["fire-missile"] = new KeyBinding
        {
            actionName = "FireMissile",
            compositePart = null,
            defaultKey = "Space",
        };
        keyBindings["fire-cannon"] = new KeyBinding
        {
            actionName = "FireCannon",
            compositePart = null,
            defaultKey = "Left Mouse",
        };
        keyBindings["deploy-flares"] = new KeyBinding
        {
            actionName = "DeployFlares",
            compositePart = null,
            defaultKey = "X",
        };
        keyBindings["toggle-ai"] = new KeyBinding
        {
            actionName = "ToggleAI",
            compositePart = null,
            defaultKey = "Tab",
        };

        foreach (var kvp in keyBindings)
        {
            kvp.Value.currentKey = kvp.Value.defaultKey;
        }
    }

    void SetupKeyButtons()
    {
        var buttonMappings = new Dictionary<string, string>
        {
            { "key-throttle-up", "throttle-up" },
            { "key-throttle-down", "throttle-down" },
            { "key-pitch-up", "pitch-up" },
            { "key-pitch-down", "pitch-down" },
            { "key-roll-left", "roll-left" },
            { "key-roll-right", "roll-right" },
            { "key-yaw-left", "yaw-left" },
            { "key-yaw-right", "yaw-right" },
            { "key-flaps", "flaps" },
            { "key-fire-missile", "fire-missile" },
            { "key-fire-cannon", "fire-cannon" },
            { "key-deploy-flares", "deploy-flares" },
            { "key-toggle-ai", "toggle-ai" },
        };

        foreach (var mapping in buttonMappings)
        {
            var button = root.Q<Button>(mapping.Key);
            if (button != null)
            {
                keyButtons[mapping.Value] = button;
                string bindingKey = mapping.Value;
                button.clicked += () => StartRebind(bindingKey);
            }
        }
    }

    void LoadKeyBindings()
    {
        foreach (var kvp in keyBindings)
        {
            string savedKey = PlayerPrefs.GetString(PREFS_PREFIX + kvp.Key, kvp.Value.defaultKey);
            kvp.Value.currentKey = savedKey;

            if (keyButtons.TryGetValue(kvp.Key, out var button))
            {
                button.text = savedKey;
            }
        }
    }

    void SaveKeyBinding(string bindingKey, string keyName)
    {
        PlayerPrefs.SetString(PREFS_PREFIX + bindingKey, keyName);
        PlayerPrefs.Save();
    }

    void StartRebind(string bindingKey)
    {
        if (!keyBindings.TryGetValue(bindingKey, out var binding))
            return;

        currentRebindKey = bindingKey;

        if (rebindOverlay != null)
        {
            rebindOverlay.style.display = DisplayStyle.Flex;
        }

        if (rebindActionName != null)
        {
            rebindActionName.text = $"Rebinding: {GetReadableActionName(bindingKey)}";
        }

        if (keyButtons.TryGetValue(bindingKey, out var button))
        {
            button.AddToClassList("listening");
            button.text = "...";
        }

        if (inputActions != null)
        {
            var action = inputActions.FindAction(binding.actionName);
            if (action != null)
            {
                int bindingIndex = FindBindingIndex(action, binding.compositePart);

                if (bindingIndex >= 0)
                {
                    currentRebindOperation = action
                        .PerformInteractiveRebinding(bindingIndex)
                        .WithControlsExcluding("Mouse")
                        .WithCancelingThrough("<Keyboard>/escape")
                        .OnMatchWaitForAnother(0.1f)
                        .OnComplete(operation => CompleteRebind(bindingKey, operation))
                        .OnCancel(operation => CancelRebind())
                        .Start();
                }
                else
                {
                    currentRebindOperation = action
                        .PerformInteractiveRebinding()
                        .WithControlsExcluding("<Mouse>/position")
                        .WithControlsExcluding("<Mouse>/delta")
                        .WithCancelingThrough("<Keyboard>/escape")
                        .OnMatchWaitForAnother(0.1f)
                        .OnComplete(operation => CompleteRebind(bindingKey, operation))
                        .OnCancel(operation => CancelRebind())
                        .Start();
                }
            }
            else
            {
                ListenForAnyKey(bindingKey);
            }
        }
        else
        {
            ListenForAnyKey(bindingKey);
        }
    }

    int FindBindingIndex(InputAction action, string compositePart)
    {
        if (string.IsNullOrEmpty(compositePart))
            return -1;

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];
            if (binding.isPartOfComposite && binding.name == compositePart)
            {
                if (binding.path.Contains("<Keyboard>"))
                {
                    return i;
                }
            }
        }

        return -1;
    }

    void ListenForAnyKey(string bindingKey)
    {
        StartCoroutine(ListenForKeyCoroutine(bindingKey));
    }

    System.Collections.IEnumerator ListenForKeyCoroutine(string bindingKey)
    {
        yield return null;

        while (currentRebindKey == bindingKey)
        {
            if (Keyboard.current != null)
            {
                foreach (var key in Keyboard.current.allKeys)
                {
                    if (key.wasPressedThisFrame && key.keyCode != Key.Escape)
                    {
                        string keyName = GetReadableKeyName(key.keyCode);
                        ApplyRebind(bindingKey, keyName);
                        yield break;
                    }
                }

                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    CancelRebind();
                    yield break;
                }
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ApplyRebind(bindingKey, "Left Mouse");
                    yield break;
                }
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    ApplyRebind(bindingKey, "Right Mouse");
                    yield break;
                }
                if (Mouse.current.middleButton.wasPressedThisFrame)
                {
                    ApplyRebind(bindingKey, "Middle Mouse");
                    yield break;
                }
            }

            yield return null;
        }
    }

    void CompleteRebind(
        string bindingKey,
        InputActionRebindingExtensions.RebindingOperation operation
    )
    {
        string newPath = operation.selectedControl?.path ?? "";
        string keyName = GetReadableKeyNameFromPath(newPath);

        operation.Dispose();
        currentRebindOperation = null;

        ApplyRebind(bindingKey, keyName);
    }

    void ApplyRebind(string bindingKey, string keyName)
    {
        if (keyBindings.TryGetValue(bindingKey, out var binding))
        {
            binding.currentKey = keyName;
            SaveKeyBinding(bindingKey, keyName);
        }

        if (keyButtons.TryGetValue(bindingKey, out var button))
        {
            button.RemoveFromClassList("listening");
            button.text = keyName;
        }

        if (rebindOverlay != null)
        {
            rebindOverlay.style.display = DisplayStyle.None;
        }

        currentRebindKey = null;

        Debug.Log($"[HowToPlay] Rebound {bindingKey} to {keyName}");
    }

    void CancelRebind()
    {
        if (currentRebindOperation != null)
        {
            currentRebindOperation.Dispose();
            currentRebindOperation = null;
        }

        if (
            !string.IsNullOrEmpty(currentRebindKey)
            && keyBindings.TryGetValue(currentRebindKey, out var binding)
        )
        {
            if (keyButtons.TryGetValue(currentRebindKey, out var button))
            {
                button.RemoveFromClassList("listening");
                button.text = binding.currentKey;
            }
        }

        if (rebindOverlay != null)
        {
            rebindOverlay.style.display = DisplayStyle.None;
        }

        currentRebindKey = null;
    }

    void OnCancelRebind()
    {
        CancelRebind();
    }

    void OnResetClicked()
    {
        foreach (var kvp in keyBindings)
        {
            kvp.Value.currentKey = kvp.Value.defaultKey;
            SaveKeyBinding(kvp.Key, kvp.Value.defaultKey);

            if (keyButtons.TryGetValue(kvp.Key, out var button))
            {
                button.text = kvp.Value.defaultKey;
            }
        }

        Debug.Log("[HowToPlay] Reset all key bindings to defaults");
    }

    void SwitchSection(string sectionName)
    {
        if (resetButton != null)
        {
            resetButton.style.display =
                (sectionName == "controls") ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (sectionControls != null)
            sectionControls.style.display =
                (sectionName == "controls") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionController != null)
            sectionController.style.display =
                (sectionName == "controller") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionTips != null)
            sectionTips.style.display =
                (sectionName == "tips") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionMission != null)
            sectionMission.style.display =
                (sectionName == "mission") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionCharacters != null)
            sectionCharacters.style.display =
                (sectionName == "characters") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionHud != null)
            sectionHud.style.display =
                (sectionName == "hud") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionWarnings != null)
            sectionWarnings.style.display =
                (sectionName == "warnings") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionCombat != null)
            sectionCombat.style.display =
                (sectionName == "combat") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionScoring != null)
            sectionScoring.style.display =
                (sectionName == "scoring") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionFeatures != null)
            sectionFeatures.style.display =
                (sectionName == "features") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionAircraft != null)
            sectionAircraft.style.display =
                (sectionName == "aircraft") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionTactics != null)
            sectionTactics.style.display =
                (sectionName == "tactics") ? DisplayStyle.Flex : DisplayStyle.None;
        if (sectionFuture != null)
            sectionFuture.style.display =
                (sectionName == "future") ? DisplayStyle.Flex : DisplayStyle.None;

        UpdateButtonState(btnControls, sectionName == "controls");
        UpdateButtonState(btnController, sectionName == "controller");
        UpdateButtonState(btnTips, sectionName == "tips");
        UpdateButtonState(btnMission, sectionName == "mission");
        UpdateButtonState(btnCharacters, sectionName == "characters");
        UpdateButtonState(btnHud, sectionName == "hud");
        UpdateButtonState(btnWarnings, sectionName == "warnings");
        UpdateButtonState(btnCombat, sectionName == "combat");
        UpdateButtonState(btnScoring, sectionName == "scoring");
        UpdateButtonState(btnFeatures, sectionName == "features");
        UpdateButtonState(btnAircraft, sectionName == "aircraft");
        UpdateButtonState(btnTactics, sectionName == "tactics");
        UpdateButtonState(btnFuture, sectionName == "future");

        var scrollView = root.Q<ScrollView>();
        if (scrollView != null)
        {
            scrollView.scrollOffset = Vector2.zero;
        }
    }

    void UpdateButtonState(Button btn, bool isActive)
    {
        if (btn == null)
            return;

        if (isActive)
            btn.AddToClassList("active");
        else
            btn.RemoveFromClassList("active");
    }

    void OnBackClicked()
    {
        SetVisible(false);

        if (mainMenuController != null)
        {
            mainMenuController.ShowMainMenu();
        }
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    public void Show()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuOpen();
        }

        SetVisible(true);
    }

    public void Hide()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuClose();
        }

        SetVisible(false);
    }

    string GetReadableActionName(string bindingKey)
    {
        return bindingKey switch
        {
            "throttle-up" => "Throttle Up",
            "throttle-down" => "Throttle Down",
            "pitch-up" => "Pitch Up",
            "pitch-down" => "Pitch Down",
            "roll-left" => "Roll Left",
            "roll-right" => "Roll Right",
            "yaw-left" => "Yaw Left",
            "yaw-right" => "Yaw Right",
            "flaps" => "Toggle Flaps",
            "fire-missile" => "Fire Missile",
            "fire-cannon" => "Fire Cannon",
            "deploy-flares" => "Deploy Flares",
            "toggle-ai" => "Toggle AI",
            _ => bindingKey,
        };
    }

    string GetReadableKeyName(Key key)
    {
        return key switch
        {
            Key.LeftShift => "Shift",
            Key.RightShift => "R-Shift",
            Key.LeftCtrl => "Ctrl",
            Key.RightCtrl => "R-Ctrl",
            Key.LeftAlt => "Alt",
            Key.RightAlt => "R-Alt",
            Key.Space => "Space",
            Key.Enter => "Enter",
            Key.Escape => "Escape",
            Key.Tab => "Tab",
            Key.Backspace => "Backspace",
            Key.UpArrow => "Up",
            Key.DownArrow => "Down",
            Key.LeftArrow => "Left",
            Key.RightArrow => "Right",
            Key.F1 => "F1",
            Key.F2 => "F2",
            Key.F3 => "F3",
            Key.F4 => "F4",
            Key.F5 => "F5",
            Key.F6 => "F6",
            Key.F7 => "F7",
            Key.F8 => "F8",
            Key.F9 => "F9",
            Key.F10 => "F10",
            Key.F11 => "F11",
            Key.F12 => "F12",
            _ => key.ToString().ToUpper(),
        };
    }

    string GetReadableKeyNameFromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "Unknown";

        int lastSlash = path.LastIndexOf('/');
        if (lastSlash >= 0 && lastSlash < path.Length - 1)
        {
            string keyPart = path.Substring(lastSlash + 1);
            return keyPart.ToUpper();
        }

        return path;
    }

    void OnDestroy()
    {
        if (backButton != null)
            backButton.clicked -= OnBackClicked;
        if (resetButton != null)
            resetButton.clicked -= OnResetClicked;
        if (cancelRebindButton != null)
            cancelRebindButton.clicked -= OnCancelRebind;

        if (currentRebindOperation != null)
        {
            currentRebindOperation.Dispose();
        }
    }
}
