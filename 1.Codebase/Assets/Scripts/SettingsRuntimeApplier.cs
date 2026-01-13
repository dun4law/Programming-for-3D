using UnityEngine;
using UnityEngine.UI;

public class SettingsRuntimeApplier : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField]
    private Canvas hudCanvas;

    [SerializeField]
    private CanvasGroup hudCanvasGroup;

    [SerializeField]
    private GameObject minimap;

    private SettingsController settingsController;

    void Start()
    {
        if (hudCanvas == null)
        {
            hudCanvas = GameObject.Find("HUD")?.GetComponent<Canvas>();
        }

        if (hudCanvasGroup == null && hudCanvas != null)
        {
            hudCanvasGroup = hudCanvas.GetComponent<CanvasGroup>();
            if (hudCanvasGroup == null)
            {
                hudCanvasGroup = hudCanvas.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (minimap == null)
        {
            minimap = GameObject.Find("Minimap");
        }

        ApplyHUDSettings();
    }

    void Update()
    {
        ApplyHUDSettings();
    }

    private void ApplyHUDSettings()
    {
        bool showHUD = PlayerPrefs.GetInt("ShowHUD", 1) == 1;
        bool showMinimap = PlayerPrefs.GetInt("ShowMinimap", 1) == 1;
        float hudOpacity = PlayerPrefs.GetFloat("HUDOpacity", 0.9f);

        if (hudCanvas != null)
        {
            hudCanvas.enabled = showHUD;
        }

        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = showHUD ? hudOpacity : 0f;
        }

        if (minimap != null)
        {
            minimap.SetActive(showMinimap);
        }
    }
}
