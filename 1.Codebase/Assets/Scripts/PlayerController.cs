using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    new Camera camera;

    [SerializeField]
    Plane plane;

    [SerializeField]
    PlaneHUD planeHUD;

    [Header("Targeting (for HUD / missiles)")]
    [SerializeField]
    bool autoAcquireTarget = true;

    [SerializeField]
    float retargetInterval = 0.25f;

    [SerializeField]
    float maxAcquireRange = 8000f;

    [SerializeField]
    float maxAcquireAngle = 25f;

    [SerializeField]
    bool retargetEvenIfCurrentValid = false;

    [SerializeField]
    bool requireLineOfSight = false;

    [SerializeField]
    LayerMask lineOfSightMask = ~0;

    Vector3 controlInput;
    PlaneCamera planeCamera;
    AIController aiController;
    Target selfTarget;
    float retargetTimer;
    MobileInputController mobileInput;

    private float mouseSensitivity = 1f;
    private float deadzone = 0.1f;
    private bool invertY = false;
    private bool invertX = false;
    private bool autoStabilize = true;
    private bool autoAimAssist = false;

    void Start()
    {
        planeCamera = GetComponent<PlaneCamera>();
        mobileInput = GetComponent<MobileInputController>();
        SetPlane(plane);
        LoadControlSettings();
        AircraftSelectionApplier.ApplySelectedAircraft(plane);

        if (plane != null && selfTarget != null)
        {
            string playerCallsign = PlayerPrefs.GetString("PlayerCallsign", "PHOENIX");
            selfTarget.SetName(playerCallsign);
        }

        Debug.Log(
            $"[PlayerController] Started! Plane: {(plane != null ? plane.DisplayName : "NULL")}, Camera: {(camera != null ? camera.name : "NULL")}"
        );

        if (plane != null)
        {
            var cm = plane.GetComponent<Countermeasures>();
            Debug.Log(
                $"[PlayerController] Countermeasures on plane: {(cm != null ? "FOUND" : "MISSING!")}"
            );
        }
    }

    void SetPlane(Plane plane)
    {
        this.plane = plane;
        aiController = plane != null ? plane.GetComponent<AIController>() : null;
        selfTarget = plane != null ? plane.GetComponent<Target>() : null;
        if (aiController != null)
            aiController.enabled = false;

        if (planeHUD != null)
        {
            planeHUD.SetPlane(plane);
            planeHUD.SetCamera(camera);
        }

        planeCamera.SetPlane(plane);
    }

    public void OnToggleHelp(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        if (context.phase == InputActionPhase.Performed)
        {
            planeHUD.ToggleHelpDialogs();
        }
    }

    public void SetThrottleInput(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;
        if (aiController != null && aiController.enabled)
            return;

        plane.SetThrottleInput(context.ReadValue<float>());
    }

    public void OnRollPitchInput(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        var input = context.ReadValue<Vector2>();

        if (Mathf.Abs(input.x) < deadzone)
            input.x = 0f;
        if (Mathf.Abs(input.y) < deadzone)
            input.y = 0f;

        input *= mouseSensitivity;

        float pitchInput = invertY ? -input.y : input.y;
        float rollInput = invertX ? input.x : -input.x;

        controlInput = new Vector3(pitchInput, controlInput.y, rollInput);
    }

    public void OnYawInput(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        var input = context.ReadValue<float>();

        if (Mathf.Abs(input) < deadzone)
            input = 0f;

        input *= mouseSensitivity;

        controlInput = new Vector3(controlInput.x, input, controlInput.z);
    }

    public void OnCameraInput(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        var input = context.ReadValue<Vector2>();
        planeCamera.SetInput(input);
    }

    public void OnFlapsInput(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        if (context.phase == InputActionPhase.Performed)
        {
            plane.ToggleFlaps();
        }
    }

    public void OnFireMissile(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        if (context.phase == InputActionPhase.Performed)
        {
            plane.TryFireMissile();
        }
    }

    public void OnFireCannon(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        if (
            context.phase == InputActionPhase.Started
            || context.phase == InputActionPhase.Performed
        )
        {
            plane.SetCannonInput(true);
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            plane.SetCannonInput(false);
        }
    }

    public void OnDeployFlares(InputAction.CallbackContext context)
    {
        Debug.Log(
            $"[PlayerController] OnDeployFlares called! Phase: {context.phase}, Plane: {(plane != null ? plane.DisplayName : "NULL")}"
        );

        if (plane == null)
        {
            Debug.LogWarning("[PlayerController] Cannot deploy flares - plane is NULL!");
            return;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            Debug.Log("[PlayerController] F key pressed - calling plane.DeployFlares()");
            plane.DeployFlares();
        }
    }

    public void OnToggleAI(InputAction.CallbackContext context)
    {
        if (plane == null)
            return;

        if (aiController != null)
        {
            aiController.enabled = !aiController.enabled;
        }
    }

    void Update()
    {
        if (plane == null)
            return;

        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.xKey.wasPressedThisFrame)
        {
            Debug.Log(
                "[PlayerController] X key detected via Keyboard.current! Deploying flares manually..."
            );
            plane.DeployFlares();
        }

        if (aiController != null && aiController.enabled)
            return;

        AutoAcquireTarget(Time.deltaTime);

        Vector3 finalControlInput = controlInput;
        if (autoAimAssist && plane.Target != null)
        {
            finalControlInput = ApplyAutoAimAssist(controlInput);
        }

        plane.SetControlInput(finalControlInput);
    }

    void AutoAcquireTarget(float dt)
    {
        if (!autoAcquireTarget)
            return;
        if (plane == null || plane.Rigidbody == null)
            return;

        retargetTimer = Mathf.Max(0, retargetTimer - dt);
        if (retargetTimer > 0)
            return;
        retargetTimer = Mathf.Max(0.02f, retargetInterval);

        var activeCamera = camera != null ? camera : Camera.main;
        if (activeCamera == null)
            return;

        var current = plane.Target;
        if (current != null)
        {
            if (!IsValidTarget(activeCamera, current))
            {
                plane.SetTarget(null);
                current = null;
            }
            else if (!retargetEvenIfCurrentValid)
            {
                return;
            }
        }

        Target best = FindBestTarget(activeCamera);
        if (best != current)
            plane.SetTarget(best);
    }

    bool IsValidTarget(Camera activeCamera, Target candidate)
    {
        if (candidate == null)
            return false;
        if (candidate == selfTarget)
            return false;

        var candidatePlane = candidate.Plane;
        if (candidatePlane == null || candidatePlane.Dead || candidatePlane.MaxHealth <= 0)
            return false;

        float dist = Vector3.Distance(plane.Rigidbody.position, candidate.Position);
        if (dist > maxAcquireRange)
            return false;

        var toTarget = candidate.Position - activeCamera.transform.position;
        if (toTarget.sqrMagnitude < 0.0001f)
            return false;

        float angle = Vector3.Angle(activeCamera.transform.forward, toTarget);
        if (angle > maxAcquireAngle)
            return false;

        if (!requireLineOfSight)
            return true;

        var dir = toTarget.normalized;
        if (
            Physics.Raycast(
                activeCamera.transform.position,
                dir,
                out var hit,
                toTarget.magnitude,
                lineOfSightMask.value,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            var hitTarget =
                hit.collider != null ? hit.collider.GetComponentInParent<Target>() : null;
            return hitTarget == candidate;
        }

        return true;
    }

    Target FindBestTarget(Camera activeCamera)
    {
        var targets = FindObjectsByType<Target>(FindObjectsSortMode.None);

        Target best = null;
        float bestScore = float.PositiveInfinity;

        foreach (var t in targets)
        {
            if (!IsValidTarget(activeCamera, t))
                continue;

            float score = ScoreTarget(activeCamera, t);
            if (score < bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        return best;
    }

    float ScoreTarget(Camera activeCamera, Target t)
    {
        var vp = activeCamera.WorldToViewportPoint(t.Position);
        float centerDist = Vector2.Distance(new Vector2(vp.x, vp.y), new Vector2(0.5f, 0.5f));
        float distance01 =
            maxAcquireRange > 0
                ? Mathf.Clamp01(
                    Vector3.Distance(plane.Rigidbody.position, t.Position) / maxAcquireRange
                )
                : 0f;

        return centerDist + distance01 * 0.25f;
    }

    private void LoadControlSettings()
    {
        mouseSensitivity = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
        deadzone = PlayerPrefs.GetFloat("JoystickDeadzone", 0.1f);
        invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
        invertX = PlayerPrefs.GetInt("InvertX", 0) == 1;
        autoStabilize = PlayerPrefs.GetInt("AutoStabilize", 1) == 1;
        autoAimAssist = PlayerPrefs.GetInt("AutoAimAssist", 0) == 1;

        Debug.Log(
            $"[PlayerController] Controls loaded - Sensitivity={mouseSensitivity:F2}, Deadzone={deadzone:F2}, InvertY={invertY}, InvertX={invertX}, AutoStabilize={autoStabilize}"
        );
    }

    public void UpdateControlSettings()
    {
        LoadControlSettings();
        Debug.Log("[PlayerController] Control settings updated");
    }

    private Vector3 ApplyAutoAimAssist(Vector3 playerInput)
    {
        if (plane == null || plane.Target == null || camera == null)
        {
            return playerInput;
        }

        Vector3 toTarget = plane.Target.Position - plane.Rigidbody.position;
        Vector3 targetDirCameraSpace = camera.transform.InverseTransformDirection(
            toTarget.normalized
        );

        float desiredPitch = -targetDirCameraSpace.y;
        float desiredRoll = -targetDirCameraSpace.x;

        float assistStrength = 0.3f;
        float finalPitch = Mathf.Lerp(playerInput.x, desiredPitch, assistStrength);
        float finalRoll = Mathf.Lerp(playerInput.z, desiredRoll, assistStrength);

        return new Vector3(finalPitch, playerInput.y, finalRoll);
    }

    public Vector3 ControlInput => controlInput;
    public Plane Plane => plane;
    public AIController AIController => aiController;

    public void SetControlInputFromMobile(Vector3 input)
    {
        if (plane == null)
            return;
        if (aiController != null && aiController.enabled)
            return;

        controlInput = new Vector3(input.x, controlInput.y, input.z);
    }
}
