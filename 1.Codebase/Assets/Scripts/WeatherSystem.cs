using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public static WeatherSystem Instance { get; private set; }

    [Header("Skybox Settings")]
    [SerializeField]
    private Material[] clearSkyboxes;

    [SerializeField]
    private Material[] rainySkyboxes;
#pragma warning disable 0414
    [SerializeField]
    private float skyboxTransitionSpeed = 0.5f;
#pragma warning restore 0414

    [Header("Rain Settings")]
    [SerializeField]
    private ParticleSystem rainParticleSystem;

    [SerializeField]
    private int rainParticleCount = 15000;

    [SerializeField]
    private float rainAreaSize = 400f;

    [SerializeField]
    private float rainHeight = 80f;

    [SerializeField]
    private float rainSpeed = 35f;

    [SerializeField]
    [Tooltip("How far ahead of the camera to position rain center")]
    private float rainForwardOffset = 100f;

    [Header("Fog Settings")]
    [SerializeField]
    private bool enableFogDuringRain = true;

    [SerializeField]
    private Color rainFogColor = new Color(0.5f, 0.55f, 0.6f, 1f);

    [SerializeField]
    private float rainFogDensity = 0.002f;

    [SerializeField]
    private float normalFogDensity = 0.0005f;

    [Header("Lighting Settings")]
    [SerializeField]
    private Light directionalLight;

    [SerializeField]
    private float normalLightIntensity = 1f;

    [SerializeField]
    private float rainLightIntensity = 0.4f;

    [SerializeField]
    private Color normalLightColor = Color.white;

    [SerializeField]
    private Color rainLightColor = new Color(0.7f, 0.75f, 0.8f, 1f);

    [Header("Audio")]
    [SerializeField]
    private AudioSource rainAudioSource;

    [SerializeField]
    private AudioClip rainLoopClip;

    [SerializeField]
    [Range(0f, 1f)]
    private float rainVolume = 0.3f;

    [Header("Random Weather")]
    [SerializeField]
    private bool enableRandomWeather = true;

    [SerializeField]
    private float minTimeBetweenWeatherChange = 60f;

    [SerializeField]
    private float maxTimeBetweenWeatherChange = 180f;

    [SerializeField]
    private float rainDurationMin = 30f;

    [SerializeField]
    private float rainDurationMax = 90f;

    [SerializeField]
    [Range(0f, 1f)]
    private float rainChance = 0.4f;

    [Header("Transition")]
    [SerializeField]
    private float weatherTransitionDuration = 5f;

    [Header("Camera Follow")]
    [SerializeField]
    private Transform cameraToFollow;

    private bool isRaining = false;
    private float weatherTimer = 0f;
    private float nextWeatherChangeTime = 0f;
    private float currentRainDuration = 0f;
    private float transitionProgress = 0f;
    private bool isTransitioning = false;
    private bool transitionToRain = false;

    private bool originalFogEnabled;
    private Color originalFogColor;
    private float originalFogDensity;
    private FogMode originalFogMode;
    private Material originalSkybox;
    private float originalLightIntensity;
    private Color originalLightColor;

    private Material currentClearSkybox;
    private Material currentRainySkybox;
    private Material targetSkybox;

    public bool IsRaining => isRaining;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("[WeatherSystem] Instance created");
        }
        else
        {
            Debug.Log("[WeatherSystem] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        originalFogEnabled = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogMode = RenderSettings.fogMode;
        originalSkybox = RenderSettings.skybox;

        if (directionalLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
        }

        if (directionalLight != null)
        {
            originalLightIntensity = directionalLight.intensity;
            originalLightColor = directionalLight.color;
            normalLightIntensity = directionalLight.intensity;
            normalLightColor = directionalLight.color;
        }

        if (
            (clearSkyboxes == null || clearSkyboxes.Length == 0)
            || (rainySkyboxes == null || rainySkyboxes.Length == 0)
        )
        {
            LoadSkyboxesFromPath();
        }

        SelectRandomSkyboxes();

        if (cameraToFollow == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraToFollow = mainCam.transform;
            }
        }

        if (rainParticleSystem == null)
        {
            CreateRainParticleSystem();
        }

        if (rainAudioSource == null)
        {
            CreateRainAudioSource();
        }

        if (enableRandomWeather)
        {
            nextWeatherChangeTime = Random.Range(
                minTimeBetweenWeatherChange * 0.5f,
                maxTimeBetweenWeatherChange * 0.5f
            );
        }

        if (currentClearSkybox != null)
        {
            RenderSettings.skybox = currentClearSkybox;
        }

        StopRainImmediate();

        Debug.Log(
            $"[WeatherSystem] Initialized - RandomWeather={enableRandomWeather}, "
                + $"ClearSkyboxes={clearSkyboxes?.Length ?? 0}, RainySkyboxes={rainySkyboxes?.Length ?? 0}, "
                + $"NextWeatherChange={nextWeatherChangeTime:F0}s, RainChance={rainChance:P0}"
        );
    }

    private void LoadSkyboxesFromPath()
    {
        Debug.Log("[WeatherSystem] Please assign skybox materials in the Inspector:");
        Debug.Log(
            "  Clear: Sky_Anime_03_Day_a, Sky_Anime_15_Day_a, Sky_Anime_18_day_a, Sky_LowPoly_01_Day_a"
        );
        Debug.Log("  Rainy: Sky_Anime_11_morning_a, Sky_LowPoly_02_Night_a");
    }

    private void SelectRandomSkyboxes()
    {
        if (clearSkyboxes != null && clearSkyboxes.Length > 0)
        {
            currentClearSkybox = clearSkyboxes[Random.Range(0, clearSkyboxes.Length)];
        }

        if (rainySkyboxes != null && rainySkyboxes.Length > 0)
        {
            currentRainySkybox = rainySkyboxes[Random.Range(0, rainySkyboxes.Length)];
        }
    }

    void Update()
    {
        if (cameraToFollow != null && rainParticleSystem != null)
        {
            Vector3 forwardOffset = cameraToFollow.forward * rainForwardOffset;
            Vector3 targetPos = cameraToFollow.position + forwardOffset + Vector3.up * rainHeight;
            rainParticleSystem.transform.position = targetPos;

            rainParticleSystem.transform.rotation = Quaternion.identity;
        }

        if (isTransitioning)
        {
            UpdateTransition();
        }

        if (enableRandomWeather && !isTransitioning)
        {
            weatherTimer += Time.deltaTime;

            if (isRaining)
            {
                if (weatherTimer >= currentRainDuration)
                {
                    StartTransition(false);
                }
            }
            else
            {
                if (weatherTimer >= nextWeatherChangeTime)
                {
                    if (Random.value < rainChance)
                    {
                        StartTransition(true);
                    }
                    else
                    {
                        weatherTimer = 0f;
                        nextWeatherChangeTime = Random.Range(
                            minTimeBetweenWeatherChange,
                            maxTimeBetweenWeatherChange
                        );

                        if (Random.value < 0.3f)
                        {
                            ChangeClearSkybox();
                        }
                    }
                }
            }
        }
    }

    private void ChangeClearSkybox()
    {
        if (clearSkyboxes == null || clearSkyboxes.Length <= 1)
            return;

        Material newSkybox;
        do
        {
            newSkybox = clearSkyboxes[Random.Range(0, clearSkyboxes.Length)];
        } while (newSkybox == currentClearSkybox && clearSkyboxes.Length > 1);

        currentClearSkybox = newSkybox;
        RenderSettings.skybox = currentClearSkybox;
        Debug.Log($" Weather: Changed skybox to {currentClearSkybox.name}");
    }

    private void CreateRainParticleSystem()
    {
        GameObject rainGO = new GameObject("RainParticles");
        rainGO.transform.SetParent(transform);
        rainParticleSystem = rainGO.AddComponent<ParticleSystem>();

        var main = rainParticleSystem.main;
        main.loop = true;
        main.startLifetime = 4f;
        main.startSpeed = rainSpeed;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.maxParticles = rainParticleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = new Color(0.7f, 0.75f, 0.85f, 0.5f);
        main.gravityModifier = 0.5f;

        var emission = rainParticleSystem.emission;
        emission.rateOverTime = rainParticleCount;

        var shape = rainParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(rainAreaSize, 1f, rainAreaSize);
        shape.rotation = new Vector3(0f, 0f, 0f);

        var velocity = rainParticleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-3f, 3f);
        velocity.y = new ParticleSystem.MinMaxCurve(0f, 0f);
        velocity.z = new ParticleSystem.MinMaxCurve(-3f, 3f);

        var renderer = rainParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = 3f;
        renderer.velocityScale = 0.15f;

        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.color = new Color(0.8f, 0.85f, 0.9f, 0.4f);

        var collision = rainParticleSystem.collision;
        collision.enabled = false;

        var noise = rainParticleSystem.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.5f;

        rainParticleSystem.Stop();
        Debug.Log(
            $"[WeatherSystem] Rain particle system created - area={rainAreaSize}m, height={rainHeight}m, particles={rainParticleCount}, forwardOffset={rainForwardOffset}m"
        );
    }

    private void CreateRainAudioSource()
    {
        GameObject audioGO = new GameObject("RainAudio");
        audioGO.transform.SetParent(transform);
        rainAudioSource = audioGO.AddComponent<AudioSource>();
        rainAudioSource.loop = true;
        rainAudioSource.playOnAwake = false;
        rainAudioSource.volume = 0f;
        rainAudioSource.spatialBlend = 0f;

        if (rainLoopClip != null)
        {
            rainAudioSource.clip = rainLoopClip;
        }
    }

    private void StartTransition(bool toRain)
    {
        isTransitioning = true;
        transitionToRain = toRain;
        transitionProgress = 0f;

        if (toRain)
        {
            if (rainySkyboxes != null && rainySkyboxes.Length > 0)
            {
                currentRainySkybox = rainySkyboxes[Random.Range(0, rainySkyboxes.Length)];
            }
            targetSkybox = currentRainySkybox;

            if (rainParticleSystem != null)
            {
                rainParticleSystem.Play();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRainLoop();
            }
            else if (rainAudioSource != null && rainLoopClip != null)
            {
                rainAudioSource.clip = rainLoopClip;
                rainAudioSource.Play();
            }

            currentRainDuration = Random.Range(rainDurationMin, rainDurationMax);

            Debug.Log(
                $" Weather: Starting rain (duration: {currentRainDuration:F0}s, skybox: {currentRainySkybox?.name})"
            );
        }
        else
        {
            if (clearSkyboxes != null && clearSkyboxes.Length > 0)
            {
                currentClearSkybox = clearSkyboxes[Random.Range(0, clearSkyboxes.Length)];
            }
            targetSkybox = currentClearSkybox;

            Debug.Log($" Weather: Rain stopping (next skybox: {currentClearSkybox?.name})");
        }
    }

    private void UpdateTransition()
    {
        transitionProgress += Time.deltaTime / weatherTransitionDuration;

        if (transitionProgress >= 1f)
        {
            transitionProgress = 1f;
            isTransitioning = false;
            isRaining = transitionToRain;
            weatherTimer = 0f;

            if (targetSkybox != null)
            {
                RenderSettings.skybox = targetSkybox;
            }

            if (!isRaining)
            {
                if (rainParticleSystem != null)
                {
                    rainParticleSystem.Stop();
                }

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.StopRainLoop();
                }
                if (rainAudioSource != null)
                {
                    rainAudioSource.Stop();
                }

                if (enableFogDuringRain)
                {
                    RenderSettings.fog = originalFogEnabled;
                    RenderSettings.fogColor = originalFogColor;
                    RenderSettings.fogDensity = originalFogDensity;
                }

                if (directionalLight != null)
                {
                    directionalLight.intensity = normalLightIntensity;
                    directionalLight.color = normalLightColor;
                }

                nextWeatherChangeTime = Random.Range(
                    minTimeBetweenWeatherChange,
                    maxTimeBetweenWeatherChange
                );
            }
        }

        float t = transitionToRain ? transitionProgress : (1f - transitionProgress);

        if (enableFogDuringRain)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = Color.Lerp(originalFogColor, rainFogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(normalFogDensity, rainFogDensity, t);
        }

        if (directionalLight != null)
        {
            directionalLight.intensity = Mathf.Lerp(normalLightIntensity, rainLightIntensity, t);
            directionalLight.color = Color.Lerp(normalLightColor, rainLightColor, t);
        }

        if (rainAudioSource != null)
        {
            rainAudioSource.volume = Mathf.Lerp(0f, rainVolume, t);
        }

        if (rainParticleSystem != null)
        {
            var emission = rainParticleSystem.emission;
            emission.rateOverTime = Mathf.Lerp(0f, rainParticleCount / 2f, t);
        }

        if (
            transitionProgress >= 0.5f
            && targetSkybox != null
            && RenderSettings.skybox != targetSkybox
        )
        {
            RenderSettings.skybox = targetSkybox;
        }
    }

    public void StartRain()
    {
        Debug.Log(
            $"[WeatherSystem] StartRain called - isRaining={isRaining}, isTransitioning={isTransitioning}"
        );
        if (!isRaining && !isTransitioning)
        {
            StartTransition(true);
        }
    }

    public void StopRain()
    {
        Debug.Log(
            $"[WeatherSystem] StopRain called - isRaining={isRaining}, isTransitioning={isTransitioning}"
        );
        if (isRaining && !isTransitioning)
        {
            StartTransition(false);
        }
    }

    public void StopRainImmediate()
    {
        Debug.Log("[WeatherSystem] StopRainImmediate called - forcing weather clear");
        isRaining = false;
        isTransitioning = false;

        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop();
            rainParticleSystem.Clear();
        }

        if (rainAudioSource != null)
        {
            rainAudioSource.Stop();
            rainAudioSource.volume = 0f;
        }

        if (enableFogDuringRain)
        {
            RenderSettings.fog = originalFogEnabled;
            RenderSettings.fogColor = originalFogColor;
            RenderSettings.fogDensity = originalFogDensity;
            RenderSettings.fogMode = originalFogMode;
        }

        if (directionalLight != null)
        {
            directionalLight.intensity = normalLightIntensity;
            directionalLight.color = normalLightColor;
        }
        Debug.Log("[WeatherSystem] Weather cleared immediately");
    }

    public void SetCamera(Transform camera)
    {
        cameraToFollow = camera;
    }

    public void SetRandomWeatherEnabled(bool enabled)
    {
        enableRandomWeather = enabled;
        if (!enabled)
        {
            StopRain();
        }
    }

    public void SetSkybox(Material skybox)
    {
        if (skybox != null)
        {
            RenderSettings.skybox = skybox;
        }
    }

    void OnDestroy()
    {
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogMode = originalFogMode;

        if (originalSkybox != null)
        {
            RenderSettings.skybox = originalSkybox;
        }

        if (directionalLight != null)
        {
            directionalLight.intensity = originalLightIntensity;
            directionalLight.color = originalLightColor;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
