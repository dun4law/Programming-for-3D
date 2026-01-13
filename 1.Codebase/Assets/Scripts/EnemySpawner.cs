using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField]
    private GameObject enemyPrefab;

    [Header("Enemy Aircraft")]
    [SerializeField]
    private List<string> enemyAircraftIds = new List<string>
    {
        "F15",
        "Su27",
        "Mig29",
        "fa18e",
        "Hawk_200",
        "mig21",
        "panavia-tornado",
        "rafalemf3",
        "Typhoon",
    };

    [Header("Spawn Settings")]
    [SerializeField]
    private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

    [SerializeField]
    private float minSpawnDistance = 2000f;

    [SerializeField]
    private float maxSpawnDistance = 5000f;

    [SerializeField]
    private float spawnHeight = 1000f;

    [SerializeField]
    private float spawnHeightVariation = 500f;

    [Header("Wave Settings")]
    [SerializeField]
    private List<Wave> waves = new List<Wave>();

    [SerializeField]
    private float timeBetweenWaves = 30f;

    [SerializeField]
    private bool autoStartWaves = true;

    [Header("Target")]
    [SerializeField]
    private Transform playerTransform;

    [Header("Debug")]
    [SerializeField]
    private bool showSpawnGizmos = true;

    private int currentWaveIndex = 0;
    private int enemiesAliveInWave = 0;
    private bool spawningActive = false;
    private List<Plane> activeEnemies = new List<Plane>();

    public System.Action<int> OnWaveStarted;
    public System.Action<int> OnWaveCompleted;
    public System.Action OnAllWavesCompleted;
    public System.Action<Plane> OnEnemySpawned;
    public System.Action<Plane> OnEnemyDestroyed;

    [System.Serializable]
    public class SpawnPoint
    {
        public string name = "Spawn Point";
        public Vector3 position;
        public float radius = 500f;
        public bool enabled = true;

        [Tooltip("If set, enemies will face this target position.")]
        public Vector3 facingDirection = Vector3.forward;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public int enemyCount = 3;
        public float spawnDelay = 2f;
        public List<int> useSpawnPointIndices = new List<int>();
        public bool facePlayer = true;

        [Tooltip("Additional enemy attribute adjustments.")]
        public float healthMultiplier = 1f;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            if (GetComponent<EnemyCallsignManager>() == null)
            {
                gameObject.AddComponent<EnemyCallsignManager>();
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Menu and story" || currentScene == "Loading")
        {
            Debug.Log($"[EnemySpawner] Skipping spawn in menu scene: {currentScene}");
            return;
        }

        if (playerTransform == null)
        {
            var playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
            }
        }

        if (autoStartWaves && waves.Count > 0)
        {
            StartCoroutine(DelayedStartWaves());
        }

        StartCoroutine(DelayedInitializeSceneEnemies());
    }

    private IEnumerator DelayedInitializeSceneEnemies()
    {
        yield return null;
        yield return new WaitForFixedUpdate();

        InitializeSceneEnemies();
    }

    private void InitializeSceneEnemies()
    {
        var allPlanes = FindObjectsByType<Plane>(FindObjectsSortMode.None);

        foreach (var plane in allPlanes)
        {
            if (plane == null)
                continue;
            if (plane.team != Team.Enemy)
                continue;
            if (activeEnemies.Contains(plane))
                continue;

            Debug.Log(
                $"[EnemySpawner] Found pre-existing enemy: {plane.name} - Disabling it (EnemySpawner will handle spawning)"
            );
            plane.gameObject.SetActive(false);
        }

        Debug.Log(
            $"[EnemySpawner] Scene enemy check complete. Active enemies: {activeEnemies.Count}"
        );
    }

    IEnumerator DelayedStartWaves()
    {
        float waitTime = 0f;
        float maxWait = 10f;

        while (playerTransform == null && waitTime < maxWait)
        {
            var playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
            {
                playerTransform = playerController.transform;
                break;
            }
            waitTime += Time.deltaTime;
            yield return null;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning(
                "[EnemySpawner] No player found after waiting, waves will not start automatically"
            );
            yield break;
        }

        yield return new WaitForSeconds(1f);

        Debug.Log("[EnemySpawner] Player found, starting waves...");
        StartCoroutine(StartWavesCoroutine());
    }

    void Update()
    {
        int removedCount = activeEnemies.RemoveAll(e => e == null || e.Dead);
        if (removedCount > 0)
        {
            Debug.Log(
                $"[EnemySpawner] Cleaned up {removedCount} dead/null enemies. Remaining: {activeEnemies.Count}"
            );
        }
    }

    public int GetAliveEnemyCount()
    {
        activeEnemies.RemoveAll(e => e == null || e.Dead);
        return activeEnemies.Count;
    }

    public void StartWaves()
    {
        if (!spawningActive)
        {
            StartCoroutine(StartWavesCoroutine());
        }
    }

    IEnumerator StartWavesCoroutine()
    {
        spawningActive = true;
        currentWaveIndex = 0;

        int maxWaves = PlayerPrefs.GetInt("MaxWaves", 0);
        int wavesToSpawn = (maxWaves == 0) ? waves.Count : Mathf.Min(maxWaves, waves.Count);

        Debug.Log(
            $"[EnemySpawner] Starting waves: maxWaves={maxWaves}, total waves={waves.Count}, will spawn={wavesToSpawn}"
        );

        if (wavesToSpawn == 0)
        {
            Debug.LogWarning("[EnemySpawner] No waves configured! Triggering victory...");
            TriggerVictory();
            yield break;
        }

        while (currentWaveIndex < wavesToSpawn)
        {
            yield return StartCoroutine(SpawnWave(waves[currentWaveIndex]));

            Debug.Log(
                $"[EnemySpawner] Wave {currentWaveIndex + 1} spawned, waiting for {activeEnemies.Count} enemies to be destroyed..."
            );

            while (GetAliveEnemyCount() > 0)
            {
                yield return new WaitForSeconds(0.5f);

                activeEnemies.RemoveAll(e => e == null || e.Dead);
            }

            enemiesAliveInWave = 0;
            OnWaveCompleted?.Invoke(currentWaveIndex);
            Debug.Log(
                $" Wave {currentWaveIndex + 1} completed! Remaining waves: {wavesToSpawn - currentWaveIndex - 1}"
            );

            currentWaveIndex++;

            if (currentWaveIndex < wavesToSpawn)
            {
                Debug.Log($"[EnemySpawner] Next wave in {timeBetweenWaves} seconds...");
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        Debug.Log($" All {wavesToSpawn} waves completed! Triggering victory...");
        TriggerVictory();
    }

    private void TriggerVictory()
    {
        OnAllWavesCompleted?.Invoke();
        spawningActive = false;

        if (GameManager.Instance != null)
        {
            Debug.Log("[EnemySpawner] Calling GameManager.CompleteMission(true)...");
            GameManager.Instance.CompleteMission(true);
        }
        else
        {
            Debug.LogError(
                "[EnemySpawner] GameManager.Instance is NULL! Cannot trigger victory screen."
            );

            MainMenuController.ShouldStartImmediately = false;
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu and story");
        }
    }

    IEnumerator SpawnWave(Wave wave)
    {
        OnWaveStarted?.Invoke(currentWaveIndex);

        float difficultyMultiplier = DifficultySettings.GetEnemyCountMultiplier();
        int adjustedEnemyCount = Mathf.Max(
            1,
            Mathf.RoundToInt(wave.enemyCount * difficultyMultiplier)
        );

        Debug.Log(
            $" Wave {currentWaveIndex + 1}: {wave.waveName} - {adjustedEnemyCount} enemies (base: {wave.enemyCount}, difficulty: {difficultyMultiplier:F2}x)"
        );

        for (int i = 0; i < adjustedEnemyCount; i++)
        {
            SpawnEnemy(wave, i);
            enemiesAliveInWave++;

            if (i < adjustedEnemyCount - 1)
            {
                yield return new WaitForSeconds(wave.spawnDelay);
            }
        }
    }

    void SpawnEnemy(Wave wave, int index)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned!");
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition(wave, index);
        Quaternion spawnRotation = GetSpawnRotation(spawnPosition, wave.facePlayer);

        var enemyGO = Instantiate(enemyPrefab, spawnPosition, spawnRotation);
        var enemyPlane = enemyGO.GetComponent<Plane>();

        if (enemyPlane != null)
        {
            float healthMultiplier =
                wave.healthMultiplier * DifficultySettings.GetEnemyHealthMultiplier();
            float damageMultiplier = DifficultySettings.GetEnemyDamageMultiplier();

            enemyPlane.team = Team.Enemy;

            string selectedAircraft = GetRandomEnemyAircraftId();
            Debug.Log(
                $"[EnemySpawner] Applying enemy aircraft '{selectedAircraft}' to {enemyPlane.name}"
            );
            AircraftSelectionApplier.ApplySelectedAircraft(enemyPlane, selectedAircraft);

            var enemyTarget = enemyPlane.GetComponent<Target>();
            string callsign =
                enemyTarget != null && !string.IsNullOrEmpty(enemyTarget.Name)
                    ? enemyTarget.Name
                    : $"BANDIT-{activeEnemies.Count + 1}";
            enemyPlane.SetCallsign(callsign);
            if (enemyTarget != null)
            {
                enemyTarget.SetName(callsign);
            }

            enemyPlane.MaxHealth *= healthMultiplier;

            var enemyDamageComponent = enemyGO.AddComponent<EnemyDifficultyDamage>();
            if (enemyDamageComponent != null)
            {
                enemyDamageComponent.damageMultiplier = damageMultiplier;
            }

            if (playerTransform != null)
            {
                var playerTarget = playerTransform.GetComponent<Target>();
                if (playerTarget != null)
                {
                    enemyPlane.SetTarget(playerTarget);
                }
            }

            activeEnemies.Add(enemyPlane);
            OnEnemySpawned?.Invoke(enemyPlane);

            StartCoroutine(MonitorEnemyDeath(enemyPlane));
        }

        Debug.Log($" Spawned enemy at {spawnPosition}");
    }

    string GetRandomEnemyAircraftId()
    {
        if (enemyAircraftIds == null || enemyAircraftIds.Count == 0)
        {
            Debug.LogWarning("[EnemySpawner] Enemy aircraft list is empty, defaulting to F15");
            return "F15";
        }

        int index = Random.Range(0, enemyAircraftIds.Count);
        string id = enemyAircraftIds[index];
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning(
                $"[EnemySpawner] Enemy aircraft entry at index {index} is empty, defaulting to F15"
            );
            return "F15";
        }
        return id;
    }

    Vector3 GetSpawnPosition(Wave wave, int index)
    {
        if (wave.useSpawnPointIndices.Count > 0 && spawnPoints.Count > 0)
        {
            int spawnIndex = wave.useSpawnPointIndices[index % wave.useSpawnPointIndices.Count];
            if (
                spawnIndex >= 0
                && spawnIndex < spawnPoints.Count
                && spawnPoints[spawnIndex].enabled
            )
            {
                var sp = spawnPoints[spawnIndex];
                return sp.position + Random.insideUnitSphere * sp.radius;
            }
        }

        var enabledPoints = spawnPoints.FindAll(p => p.enabled);
        if (enabledPoints.Count > 0)
        {
            var sp = enabledPoints[Random.Range(0, enabledPoints.Count)];
            Vector3 randomOffset = Random.insideUnitSphere * sp.radius;
            randomOffset.y = Random.Range(-spawnHeightVariation, spawnHeightVariation);
            return sp.position + randomOffset;
        }

        if (playerTransform != null)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            float height = spawnHeight + Random.Range(-spawnHeightVariation, spawnHeightVariation);

            return playerTransform.position
                + new Vector3(Mathf.Cos(angle) * distance, height, Mathf.Sin(angle) * distance);
        }

        return new Vector3(
            Random.Range(-maxSpawnDistance, maxSpawnDistance),
            spawnHeight + Random.Range(-spawnHeightVariation, spawnHeightVariation),
            Random.Range(-maxSpawnDistance, maxSpawnDistance)
        );
    }

    Quaternion GetSpawnRotation(Vector3 spawnPosition, bool facePlayer)
    {
        if (facePlayer && playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - spawnPosition).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer.sqrMagnitude > 0.001f)
            {
                return Quaternion.LookRotation(directionToPlayer);
            }
        }

        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    IEnumerator MonitorEnemyDeath(Plane enemy)
    {
        while (enemy != null && !enemy.Dead)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (enemy != null)
        {
            Debug.Log($"[EnemySpawner] Enemy died: {enemy.name}");
            OnEnemyDestroyed?.Invoke(enemy);
            activeEnemies.Remove(enemy);
        }
        else
        {
            Debug.Log("[EnemySpawner] Enemy was destroyed (null reference)");
        }

        enemiesAliveInWave = Mathf.Max(0, enemiesAliveInWave - 1);
        Debug.Log(
            $"[EnemySpawner] Enemies remaining: activeEnemies={activeEnemies.Count}, enemiesAliveInWave={enemiesAliveInWave}"
        );
    }

    public Plane SpawnSingleEnemy(Vector3 position, Quaternion rotation)
    {
        if (enemyPrefab == null)
            return null;

        var enemyGO = Instantiate(enemyPrefab, position, rotation);
        var enemyPlane = enemyGO.GetComponent<Plane>();

        if (enemyPlane != null)
        {
            string selectedAircraft = GetRandomEnemyAircraftId();
            Debug.Log(
                $"[EnemySpawner] Applying enemy aircraft '{selectedAircraft}' to {enemyPlane.name}"
            );
            AircraftSelectionApplier.ApplySelectedAircraft(enemyPlane, selectedAircraft);

            var enemyTarget = enemyPlane.GetComponent<Target>();
            string callsign =
                enemyTarget != null && !string.IsNullOrEmpty(enemyTarget.Name)
                    ? enemyTarget.Name
                    : $"BANDIT-{activeEnemies.Count + 1}";
            enemyPlane.SetCallsign(callsign);
            if (enemyTarget != null)
            {
                enemyTarget.SetName(callsign);
            }

            activeEnemies.Add(enemyPlane);
            OnEnemySpawned?.Invoke(enemyPlane);
            StartCoroutine(MonitorEnemyDeath(enemyPlane));
        }

        return enemyPlane;
    }

    public void ForceNextWave()
    {
        if (currentWaveIndex < waves.Count && !spawningActive)
        {
            StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            currentWaveIndex++;
        }
    }

    public int GetCurrentWave()
    {
        return currentWaveIndex;
    }

    public int GetTotalWaves()
    {
        return waves.Count;
    }

    void OnDrawGizmos()
    {
        if (!showSpawnGizmos)
            return;

        Gizmos.color = Color.red;
        foreach (var sp in spawnPoints)
        {
            if (sp.enabled)
            {
                Gizmos.DrawWireSphere(sp.position, sp.radius);
                Gizmos.DrawLine(sp.position, sp.position + sp.facingDirection.normalized * 200f);
            }
        }
    }
}
