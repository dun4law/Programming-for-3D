using UnityEngine;

public class VaporTrail : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Plane plane;

    [Header("Trail Renderers")]
    [SerializeField]
    private TrailRenderer[] wingTipTrails;

    [SerializeField]
    private TrailRenderer[] engineExhaustTrails;

    [Header("Auto-Create Settings")]
    [SerializeField]
    private bool autoCreateTrails = true;

    [SerializeField]
    private float wingSpan = 5f;

    [SerializeField]
    private float engineOffset = -3f;

    [SerializeField]
    private Color trailColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Trigger Settings")]
    [SerializeField]
    private float gThreshold = 3f;

    [SerializeField]
    private float altitudeMin = 500f;

    [SerializeField]
    private float speedMin = 100f;

    [Header("Trail Properties")]
    [SerializeField]
    private float trailTime = 2f;

    [SerializeField]
    private float trailWidth = 0.3f;

    [SerializeField]
    private float fadeSpeed = 2f;

    private float[] originalTrailTimes;
    private bool trailsActive = false;

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponent<Plane>();
        }

        if (autoCreateTrails)
        {
            if (wingTipTrails == null || wingTipTrails.Length == 0)
            {
                CreateWingTipTrails();
            }
            if (engineExhaustTrails == null || engineExhaustTrails.Length == 0)
            {
                CreateEngineExhaustTrails();
            }
        }

        InitializeTrails(wingTipTrails);
        InitializeTrails(engineExhaustTrails);
    }

    void CreateWingTipTrails()
    {
        wingTipTrails = new TrailRenderer[2];

        GameObject leftWing = new GameObject("LeftWingTipTrail");
        leftWing.transform.SetParent(transform);
        leftWing.transform.localPosition = new Vector3(-wingSpan / 2f, 0, 0);
        leftWing.transform.localRotation = Quaternion.identity;
        wingTipTrails[0] = CreateTrailRenderer(leftWing);

        GameObject rightWing = new GameObject("RightWingTipTrail");
        rightWing.transform.SetParent(transform);
        rightWing.transform.localPosition = new Vector3(wingSpan / 2f, 0, 0);
        rightWing.transform.localRotation = Quaternion.identity;
        wingTipTrails[1] = CreateTrailRenderer(rightWing);

        Debug.Log(" VaporTrail: Automatically creating wingtip trails");
    }

    void CreateEngineExhaustTrails()
    {
        engineExhaustTrails = new TrailRenderer[1];

        GameObject exhaust = new GameObject("EngineExhaustTrail");
        exhaust.transform.SetParent(transform);
        exhaust.transform.localPosition = new Vector3(0, 0, engineOffset);
        exhaust.transform.localRotation = Quaternion.identity;
        engineExhaustTrails[0] = CreateTrailRenderer(exhaust);

        Debug.Log(" VaporTrail: Automatically creating engine exhaust trails");
    }

    TrailRenderer CreateTrailRenderer(GameObject obj)
    {
        TrailRenderer trail = obj.AddComponent<TrailRenderer>();

        trail.time = 0;
        trail.startWidth = trailWidth;
        trail.endWidth = 0f;
        trail.minVertexDistance = 0.1f;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(trailColor, 0.0f),
                new GradientColorKey(trailColor, 1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(trailColor.a, 0.0f),
                new GradientAlphaKey(0f, 1.0f),
            }
        );
        trail.colorGradient = gradient;

        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.material.color = trailColor;

        trail.emitting = false;

        return trail;
    }

    void InitializeTrails(TrailRenderer[] trails)
    {
        if (trails == null)
            return;

        foreach (var trail in trails)
        {
            if (trail != null)
            {
                trail.time = 0;
                trail.emitting = false;
            }
        }
    }

    void Update()
    {
        if (plane == null || plane.Dead)
            return;

        float dt = Time.deltaTime;
        float gForce = Mathf.Abs(plane.LocalGForce.y / 9.81f);
        float altitude = plane.transform.position.y;
        float speed = plane.LocalVelocity.z;

        bool shouldShow = (gForce > gThreshold) || (altitude > altitudeMin && speed > speedMin);

        if (shouldShow && !trailsActive)
        {
            ActivateTrails();
        }
        else if (!shouldShow && trailsActive)
        {
            DeactivateTrails();
        }

        if (trailsActive)
        {
            float intensity = Mathf.Clamp01((gForce - gThreshold) / 5f);
            UpdateTrailIntensity(intensity);
        }

        UpdateTrailFade(dt);
    }

    void ActivateTrails()
    {
        trailsActive = true;

        if (wingTipTrails != null)
        {
            foreach (var trail in wingTipTrails)
            {
                if (trail != null)
                {
                    trail.time = trailTime;
                    trail.emitting = true;
                }
            }
        }

        if (engineExhaustTrails != null)
        {
            foreach (var trail in engineExhaustTrails)
            {
                if (trail != null)
                {
                    trail.time = trailTime;
                    trail.emitting = true;
                }
            }
        }
    }

    void DeactivateTrails()
    {
        trailsActive = false;

        if (wingTipTrails != null)
        {
            foreach (var trail in wingTipTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                }
            }
        }

        if (engineExhaustTrails != null)
        {
            foreach (var trail in engineExhaustTrails)
            {
                if (trail != null)
                {
                    trail.emitting = false;
                }
            }
        }
    }

    void UpdateTrailFade(float dt)
    {
        float targetTime = trailsActive ? trailTime : 0f;
        float step = Mathf.Max(0f, fadeSpeed) * dt;

        if (wingTipTrails != null)
        {
            foreach (var trail in wingTipTrails)
            {
                if (trail != null)
                {
                    trail.time = Mathf.MoveTowards(trail.time, targetTime, step);
                }
            }
        }

        if (engineExhaustTrails != null)
        {
            foreach (var trail in engineExhaustTrails)
            {
                if (trail != null)
                {
                    trail.time = Mathf.MoveTowards(trail.time, targetTime, step);
                }
            }
        }
    }

    void UpdateTrailIntensity(float intensity)
    {
        if (wingTipTrails == null)
            return;

        foreach (var trail in wingTipTrails)
        {
            if (trail != null)
            {
                trail.widthMultiplier = Mathf.Lerp(0.1f, 1f, intensity);
            }
        }
    }
}
