using TMPro;
using UnityEngine;

public class GameHUDController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Plane playerPlane;

    [Header("UI Text Elements")]
    [SerializeField]
    private TextMeshProUGUI speedText;

    [SerializeField]
    private TextMeshProUGUI altitudeText;

    [SerializeField]
    private TextMeshProUGUI throttleText;

    [SerializeField]
    private TextMeshProUGUI healthText;

    [Header("Optional UI Elements")]
    [SerializeField]
    private TextMeshProUGUI gForceText;

    [SerializeField]
    private TextMeshProUGUI headingText;

    [Header("Settings")]
    [SerializeField]
    private bool useKnots = true;

    [SerializeField]
    private bool useFeet = true;

    private const float MS_TO_KNOTS = 1.94384f;
    private const float MS_TO_KMH = 3.6f;
    private const float M_TO_FEET = 3.28084f;

    void Update()
    {
        if (playerPlane == null)
            return;

        UpdateSpeedDisplay();
        UpdateAltitudeDisplay();
        UpdateThrottleDisplay();
        UpdateHealthDisplay();
        UpdateOptionalDisplays();
    }

    private void UpdateSpeedDisplay()
    {
        if (speedText == null)
            return;

        float speed = playerPlane.LocalVelocity.z;

        if (useKnots)
        {
            float knots = speed * MS_TO_KNOTS;
            speedText.text = $"{knots:F0} KTS";
        }
        else
        {
            float kmh = speed * MS_TO_KMH;
            speedText.text = $"{kmh:F0} KM/H";
        }
    }

    private void UpdateAltitudeDisplay()
    {
        if (altitudeText == null)
            return;

        float altitude = playerPlane.transform.position.y;

        if (useFeet)
        {
            float feet = altitude * M_TO_FEET;
            altitudeText.text = $"{feet:F0} FT";
        }
        else
        {
            altitudeText.text = $"{altitude:F0} M";
        }
    }

    private void UpdateThrottleDisplay()
    {
        if (throttleText == null)
            return;

        float throttle = playerPlane.Throttle * 100f;
        throttleText.text = $"THR {throttle:F0}%";
    }

    private void UpdateHealthDisplay()
    {
        if (healthText == null)
            return;

        float healthPercent = (playerPlane.Health / playerPlane.MaxHealth) * 100f;
        healthText.text = $"HP {healthPercent:F0}%";

        if (healthPercent < 30f)
            healthText.color = Color.red;
        else if (healthPercent < 60f)
            healthText.color = Color.yellow;
        else
            healthText.color = Color.white;
    }

    private void UpdateOptionalDisplays()
    {
        if (gForceText != null)
        {
            float gForce = playerPlane.LocalGForce.magnitude / 9.81f;
            gForceText.text = $"{gForce:F1} G";
        }

        if (headingText != null)
        {
            float heading = playerPlane.transform.eulerAngles.y;
            headingText.text = $"{heading:F0}";
        }
    }

    public void SetPlayerPlane(Plane plane)
    {
        playerPlane = plane;
    }
}
