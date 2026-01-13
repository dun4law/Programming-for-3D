using UnityEngine;

public class MobileInputController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Plane plane;

    [SerializeField]
    private PlayerController playerController;

    [Header("Touch Settings")]
    [SerializeField]
    private float doubleTapTime = 0.3f;

    [SerializeField]
    private float touchSensitivity = 0.005f;

    [Header("Gyroscope Settings")]
    [SerializeField]
    private float gyroSensitivity = 1.5f;

    [SerializeField]
    private float gyroDeadzone = 0.05f;

    private AIController aiController;
    private float lastTapTime;
    private bool gyroEnabled;
    private Gyroscope gyro;
    private Quaternion gyroInitialRotation;

    private Vector2 touchStartPos;
    private bool isDragging;
    private float pitchInput;
    private float rollInput;
    private bool hasMobileInput;

    void Start()
    {
        if (plane == null)
            plane = GetComponent<Plane>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        aiController = plane != null ? plane.GetComponent<AIController>() : null;

        EnableGyroscope();

        Input.multiTouchEnabled = true;

        Debug.Log($"[MobileInput] Started - Gyro: {(gyroEnabled ? "ENABLED" : "NOT AVAILABLE")}");
    }

    void EnableGyroscope()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
            gyroEnabled = true;
            gyroInitialRotation = gyro.attitude;
            Debug.Log("[MobileInput] Gyroscope enabled");
        }
        else
        {
            gyroEnabled = false;
            Debug.Log("[MobileInput] Gyroscope not supported on this device");
        }
    }

    void Update()
    {
        if (plane == null)
            return;

        HandleTouchInput();
        HandleGyroscopeInput();
        ApplyMobileInput();
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            isDragging = false;
            pitchInput = 0f;
            rollInput = 0f;
            hasMobileInput = false;
            return;
        }

        hasMobileInput = true;
        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            float timeSinceLastTap = Time.time - lastTapTime;

            if (timeSinceLastTap <= doubleTapTime)
            {
                OnDoubleTap();
                lastTapTime = 0f;
                isDragging = false;
            }
            else
            {
                lastTapTime = Time.time;
                touchStartPos = touch.position;
                isDragging = false;
            }
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            Vector2 delta = touch.position - touchStartPos;

            if (delta.magnitude > 20f)
            {
                isDragging = true;
            }

            if (isDragging)
            {
                pitchInput = Mathf.Clamp(-delta.y * touchSensitivity, -1f, 1f);
                rollInput = Mathf.Clamp(-delta.x * touchSensitivity, -1f, 1f);
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            float tapDuration = Time.time - lastTapTime;

            if (!isDragging && tapDuration < doubleTapTime && lastTapTime > 0)
            {
                StartCoroutine(WaitForDoubleTapOrFire());
            }

            isDragging = false;
            pitchInput = 0f;
            rollInput = 0f;
        }
    }

    System.Collections.IEnumerator WaitForDoubleTapOrFire()
    {
        float waitTime = doubleTapTime;
        float startTime = Time.time;

        while (Time.time - startTime < waitTime)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                yield break;
            }
            yield return null;
        }

        OnSingleTap();
    }

    void OnSingleTap()
    {
        if (plane != null)
        {
            plane.TryFireMissile();
            Debug.Log("[MobileInput] Single tap - Fire missile");
        }
    }

    void OnDoubleTap()
    {
        if (aiController != null)
        {
            aiController.enabled = !aiController.enabled;
            Debug.Log($"[MobileInput] Double tap - AI Control: {(aiController.enabled ? "ON" : "OFF")}");
        }
    }

    void HandleGyroscopeInput()
    {
        if (!gyroEnabled || gyro == null)
            return;

        if (aiController != null && aiController.enabled)
            return;

        if (isDragging)
            return;

        if (Input.touchCount > 0)
            return;

        Quaternion currentAttitude = gyro.attitude;

        Quaternion relativeRotation = Quaternion.Inverse(gyroInitialRotation) * currentAttitude;

        float gyroRoll = relativeRotation.eulerAngles.y;
        if (gyroRoll > 180f) gyroRoll -= 360f;

        gyroRoll = gyroRoll / 90f;

        if (Mathf.Abs(gyroRoll) < gyroDeadzone)
            gyroRoll = 0f;

        gyroRoll = Mathf.Clamp(gyroRoll * gyroSensitivity, -1f, 1f);

        if (Mathf.Abs(gyroRoll) > 0.01f)
        {
            rollInput = -gyroRoll;
            hasMobileInput = true;
        }
    }

    void ApplyMobileInput()
    {
        if (aiController != null && aiController.enabled)
            return;

        if (!hasMobileInput)
            return;

        if (playerController != null)
        {
            playerController.SetControlInputFromMobile(new Vector3(pitchInput, 0, rollInput));
        }
    }

    public void ResetGyroReference()
    {
        if (gyroEnabled && gyro != null)
        {
            gyroInitialRotation = gyro.attitude;
            Debug.Log("[MobileInput] Gyro reference reset");
        }
    }

    public void SetGyroSensitivity(float sensitivity)
    {
        gyroSensitivity = Mathf.Clamp(sensitivity, 0.5f, 3f);
    }

    public bool IsGyroEnabled => gyroEnabled;
    public bool IsAIEnabled => aiController != null && aiController.enabled;
}
