using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class UIKeyboardNavigation : MonoBehaviour
{
    [Header("First Selected Buttons")]
    [SerializeField]
    private Selectable mainMenuFirstButton;

    [SerializeField]
    private Selectable pauseMenuFirstButton;

    [Header("References")]
    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject pauseMenuPanel;

    private EventSystem eventSystem;

    private void Awake()
    {
        EnsureEventSystem();
    }

    void Start()
    {
        EnsureEventSystem();

        if (mainMenuPanel == null)
            mainMenuPanel =
                GameObject.Find("MainMenuPanel") ?? UIFindInSceneIncludingInactive("MainMenuPanel");
        if (pauseMenuPanel == null)
            pauseMenuPanel =
                GameObject.Find("PauseMenuPanel")
                ?? UIFindInSceneIncludingInactive("PauseMenuPanel");

        if (mainMenuFirstButton == null)
            mainMenuFirstButton = UIFindFirstSelectable(mainMenuPanel);
        if (pauseMenuFirstButton == null)
            pauseMenuFirstButton = UIFindFirstSelectable(pauseMenuPanel);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;
        if (!EnsureEventSystem())
            return;

        EnsureButtonSelected();

        HandleNavigation();

        HandleSubmit();
    }

    private void EnsureButtonSelected()
    {
        if (eventSystem == null)
            return;

        if (
            eventSystem.currentSelectedGameObject == null
            || !eventSystem.currentSelectedGameObject.activeInHierarchy
        )
        {
            if (
                mainMenuPanel != null
                && mainMenuPanel.activeInHierarchy
                && mainMenuFirstButton != null
            )
            {
                eventSystem.SetSelectedGameObject(mainMenuFirstButton.gameObject);
            }
            else if (
                pauseMenuPanel != null
                && pauseMenuPanel.activeInHierarchy
                && pauseMenuFirstButton != null
            )
            {
                eventSystem.SetSelectedGameObject(pauseMenuFirstButton.gameObject);
            }
        }
    }

    private void HandleNavigation()
    {
        if (eventSystem == null)
            return;

        var current = eventSystem.currentSelectedGameObject;
        if (current == null)
            return;

        var selectable = current.GetComponent<Selectable>();
        if (selectable == null)
            return;

        Selectable next = null;

        if (
            Keyboard.current.upArrowKey.wasPressedThisFrame
            || Keyboard.current.wKey.wasPressedThisFrame
        )
        {
            next = selectable.FindSelectableOnUp();
        }
        else if (
            Keyboard.current.downArrowKey.wasPressedThisFrame
            || Keyboard.current.sKey.wasPressedThisFrame
        )
        {
            next = selectable.FindSelectableOnDown();
        }
        else if (
            Keyboard.current.leftArrowKey.wasPressedThisFrame
            || Keyboard.current.aKey.wasPressedThisFrame
        )
        {
            next = selectable.FindSelectableOnLeft();
        }
        else if (
            Keyboard.current.rightArrowKey.wasPressedThisFrame
            || Keyboard.current.dKey.wasPressedThisFrame
        )
        {
            next = selectable.FindSelectableOnRight();
        }

        if (next != null)
        {
            eventSystem.SetSelectedGameObject(next.gameObject);
        }
    }

    private void HandleSubmit()
    {
        if (eventSystem == null)
            return;

        if (
            Keyboard.current.enterKey.wasPressedThisFrame
            || Keyboard.current.spaceKey.wasPressedThisFrame
        )
        {
            var current = eventSystem.currentSelectedGameObject;
            if (current == null)
                return;

            var button = current.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                button.onClick.Invoke();
            }
        }
    }

    public void SelectMainMenuButton()
    {
        if (!EnsureEventSystem())
            return;

        if (mainMenuFirstButton != null)
        {
            eventSystem.SetSelectedGameObject(mainMenuFirstButton.gameObject);
        }
    }

    public void SelectPauseMenuButton()
    {
        if (!EnsureEventSystem())
            return;

        if (pauseMenuFirstButton != null)
        {
            eventSystem.SetSelectedGameObject(pauseMenuFirstButton.gameObject);
        }
    }

    private bool EnsureEventSystem()
    {
        if (eventSystem != null)
            return true;

        eventSystem =
            EventSystem.current != null
                ? EventSystem.current
                : FindFirstObjectByType<EventSystem>();
        if (eventSystem != null)
            return true;

        var go = new GameObject("EventSystem");
        eventSystem = go.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
        if (go.GetComponent<InputSystemUIInputModule>() == null)
        {
            go.AddComponent<InputSystemUIInputModule>();
        }
#endif

        return eventSystem != null;
    }

    private static Selectable UIFindFirstSelectable(GameObject panel)
    {
        if (panel == null)
            return null;

        var selectables = panel.GetComponentsInChildren<Selectable>(true);
        foreach (var s in selectables)
        {
            if (s == null)
                continue;
            if (!s.gameObject.activeInHierarchy)
                continue;
            if (!s.interactable)
                continue;
            return s;
        }

        return selectables != null && selectables.Length > 0 ? selectables[0] : null;
    }

    private static GameObject UIFindInSceneIncludingInactive(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
            return null;

        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
        {
            if (go == null)
                continue;
            if (!go.scene.IsValid())
                continue;
            if (!string.Equals(go.name, objectName, System.StringComparison.Ordinal))
                continue;
            return go;
        }

        return null;
    }
}
