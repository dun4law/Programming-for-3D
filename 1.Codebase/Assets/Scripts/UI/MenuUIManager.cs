using UnityEngine;
using UnityEngine.UIElements;

public class MenuUIManager : MonoBehaviour
{
    [Header("UXML Assets")]
    [Tooltip("Main Menu UXML Asset (Assets/UI/UXML/MainMenu.uxml)")]
    [SerializeField]
    private VisualTreeAsset mainMenuAsset;

    [Tooltip("Pause Menu UXML Asset (Assets/UI/UXML/PauseMenu.uxml)")]
    [SerializeField]
    private VisualTreeAsset pauseMenuAsset;

    [Tooltip("Settings Menu UXML Asset (Assets/UI/UXML/SettingsMenu.uxml)")]
    [SerializeField]
    private VisualTreeAsset settingsMenuAsset;

    [Header("Style Sheet")]
    [Tooltip("Menu Stylesheet (Assets/UI/USS/MenuStyles.uss)")]
    [SerializeField]
    private StyleSheet menuStyleSheet;

    [Header("Panel Settings")]
    [Tooltip("UI Toolkit Panel Settings (Optional)")]
    [SerializeField]
    private PanelSettings panelSettings;

    [Header("Controllers")]
    [SerializeField]
    private MainMenuController mainMenuController;

    [SerializeField]
    private PauseMenuController pauseMenuController;

    [SerializeField]
    private SettingsController settingsController;

    private UIDocument uiDocument;

    private void Awake()
    {
        SetupUIDocument();
        SetupControllers();
    }

    private void SetupUIDocument()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            uiDocument = gameObject.AddComponent<UIDocument>();
        }

        if (panelSettings != null)
        {
            uiDocument.panelSettings = panelSettings;
        }

        var root = new VisualElement();
        root.style.flexGrow = 1;

        if (menuStyleSheet != null)
        {
            root.styleSheets.Add(menuStyleSheet);
        }

        if (mainMenuAsset != null)
        {
            var mainMenu = mainMenuAsset.CloneTree();
            root.Add(mainMenu);
        }

        if (pauseMenuAsset != null)
        {
            var pauseMenu = pauseMenuAsset.CloneTree();
            root.Add(pauseMenu);
        }

        if (settingsMenuAsset != null)
        {
            var settingsMenu = settingsMenuAsset.CloneTree();
            root.Add(settingsMenu);
        }

        uiDocument.rootVisualElement.Add(root);
    }

    private void SetupControllers()
    {
        if (mainMenuController != null)
        {
            var doc = mainMenuController.GetComponent<UIDocument>();
            if (doc == null)
            {
                doc = mainMenuController.gameObject.AddComponent<UIDocument>();
            }

            SetUIDocumentReference(mainMenuController, uiDocument);
        }

        if (pauseMenuController != null)
        {
            var doc = pauseMenuController.GetComponent<UIDocument>();
            if (doc == null)
            {
                doc = pauseMenuController.gameObject.AddComponent<UIDocument>();
            }
            SetUIDocumentReference(pauseMenuController, uiDocument);
        }

        if (settingsController != null)
        {
            var doc = settingsController.GetComponent<UIDocument>();
            if (doc == null)
            {
                doc = settingsController.gameObject.AddComponent<UIDocument>();
            }
            SetUIDocumentReference(settingsController, uiDocument);
        }
    }

    private void SetUIDocumentReference(MonoBehaviour controller, UIDocument doc)
    {
        var field = controller
            .GetType()
            .GetField(
                "uiDocument",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

        if (field != null)
        {
            field.SetValue(controller, doc);
        }
    }

    public UIDocument GetUIDocument() => uiDocument;
}
