using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneCamera : MonoBehaviour
{
    [SerializeField]
    new Camera camera;

    [SerializeField]
    Vector3 cameraOffset;

    [SerializeField]
    Vector2 lookAngle;

    [SerializeField]
    float movementScale;

    [SerializeField]
    float lookAlpha;

    [SerializeField]
    float movementAlpha;

    [SerializeField]
    Vector3 deathOffset;

    [SerializeField]
    float deathSensitivity;

    Transform cameraTransform;
    Plane plane;
    Transform planeTransform;
    Vector2 lookInput;
    bool dead;

    Vector2 look;
    Vector2 lookAverage;
    Vector3 avAverage;
    Vector3 baseCameraOffset;
    bool hasBaseOffset;

    void Awake()
    {
        cameraTransform = camera.GetComponent<Transform>();
        baseCameraOffset = cameraOffset;
        hasBaseOffset = true;

        if (camera != null)
        {
            camera.nearClipPlane = 0.1f;
        }
    }

    public void SetPlane(Plane plane)
    {
        this.plane = plane;

        if (plane == null)
        {
            planeTransform = null;
        }
        else
        {
            planeTransform = plane.GetComponent<Transform>();

            string selectedAircraft = PlayerPrefs.GetString(
                AircraftSelectionApplier.SelectedAircraftKey,
                "F15"
            );
            Vector3 aircraftOffset = AircraftSelectionApplier.GetCameraOffsetForAircraft(
                selectedAircraft
            );

            Debug.Log(
                $"[PlaneCamera] SetPlane called. Selected aircraft: '{selectedAircraft}', offset from dict: {aircraftOffset}"
            );

            if (aircraftOffset != Vector3.zero)
            {
                cameraOffset = aircraftOffset;
                baseCameraOffset = cameraOffset;
                Debug.Log(
                    $"[PlaneCamera] Applied camera offset for {selectedAircraft}: {cameraOffset}"
                );
            }
            else if (plane.CameraOffset != Vector3.zero)
            {
                cameraOffset = plane.CameraOffset;
                baseCameraOffset = cameraOffset;
                Debug.Log(
                    $"[PlaneCamera] Using custom camera offset from {plane.DisplayName}: {cameraOffset}"
                );
            }
            else
            {
                Debug.Log($"[PlaneCamera] Using default scene camera offset: {cameraOffset}");
            }
        }

        cameraTransform.SetParent(planeTransform);
    }

    public void SetInput(Vector2 input)
    {
        lookInput = input;
    }

    public void SetCameraDistanceScale(float scale)
    {
        if (!hasBaseOffset)
        {
            baseCameraOffset = cameraOffset;
            hasBaseOffset = true;
        }

        float clampedScale = Mathf.Max(0.1f, scale);
        cameraOffset = baseCameraOffset * clampedScale;
    }

    void LateUpdate()
    {
        if (plane == null)
            return;

        var cameraOffset = GetCameraOffsetForView();

        if (plane.Dead)
        {
            look += lookInput * deathSensitivity * Time.deltaTime;
            look.x = (look.x + 360f) % 360f;
            look.y = Mathf.Clamp(look.y, -lookAngle.y, lookAngle.y);

            lookAverage = look;
            avAverage = new Vector3();

            cameraOffset = deathOffset;
        }
        else
        {
            var targetLookAngle = Vector2.Scale(lookInput, lookAngle);
            lookAverage = (lookAverage * (1 - lookAlpha)) + (targetLookAngle * lookAlpha);

            var angularVelocity = plane.LocalAngularVelocity;
            angularVelocity.z = -angularVelocity.z;

            avAverage = (avAverage * (1 - movementAlpha)) + (angularVelocity * movementAlpha);
        }

        var rotation = Quaternion.Euler(-lookAverage.y, lookAverage.x, 0);
        var turningRotation = Quaternion.Euler(
            new Vector3(-avAverage.x, -avAverage.y, avAverage.z) * movementScale
        );

        cameraTransform.localPosition = rotation * turningRotation * cameraOffset;
        cameraTransform.localRotation = rotation * turningRotation;

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log(
                $"[PlaneCamera] Camera localPos={cameraTransform.localPosition}, offset used={cameraOffset}"
            );
        }
    }

    private Vector3 GetCameraOffsetForView()
    {
        int cameraView = PlayerPrefs.GetInt("CameraView", 0);
        Vector3 result;

        switch (cameraView)
        {
            case 1:
                result = new Vector3(
                    cameraOffset.x * 0.3f,
                    cameraOffset.y * 0.5f,
                    cameraOffset.z * 0.3f
                );
                break;

            case 2:
                result = new Vector3(
                    cameraOffset.x * 2f,
                    cameraOffset.y * 1.5f,
                    cameraOffset.z * 2f
                );
                break;

            default:
                result = cameraOffset;
                break;
        }

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log(
                $"[PlaneCamera] CameraView={cameraView}, baseOffset={cameraOffset}, actualOffset={result}"
            );
        }

        return result;
    }
}
