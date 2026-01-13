using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Aircraft/Aircraft Tuning Database")]
public class AircraftTuningDatabase : ScriptableObject
{
    [SerializeField]
    private List<AircraftTuning> aircraft = new List<AircraftTuning>();

    [SerializeField]
    private AircraftTuning fallback;

    private const float BaseSpeedPercent = 90f;
    private const float BaseMobilityPercent = 80f;
    private const float BaseFirepowerPercent = 95f;
    private const float BaseMaxThrust = 222411f;
    private const float BaseThrottleSpeed = 1f;
    private const float BaseGLimit = 9f;
    private const float BaseGLimitPitch = 11f;
    private const float BaseMissileReload = 5f;
    private const float BaseMissileDebounce = 0.125f;
    private const float BaseStallAOA = 15f;
    private const float BaseCriticalAOA = 25f;
    private const float BaseStallMinSpeed = 50f;
    private const float BaseStallCriticalSpeed = 30f;
    private const float BaseAiMaxSpeed = 300f;
    private const float BaseAiMinSpeed = 175f;
    private const float BaseAiRecoverMin = 125f;
    private const float BaseAiRecoverMax = 150f;
    private const float BaseAiGroundAvoidMin = 125f;
    private const float BaseAiGroundAvoidMax = 150f;
    private const float BaseAiMissileLockDelay = 1f;
    private const float BaseAiMissileCooldown = 5f;
    private const float BaseRadarRange = 4000f;

    public AircraftTuning Get(string aircraftId)
    {
        if (string.IsNullOrWhiteSpace(aircraftId))
            return fallback;

        for (int i = 0; i < aircraft.Count; i++)
        {
            var entry = aircraft[i];
            if (entry != null && entry.Matches(aircraftId))
                return entry;
        }

        return fallback;
    }

    public static AircraftTuningDatabase LoadOrCreateDefault()
    {
        var db = Resources.Load<AircraftTuningDatabase>("AircraftTuningDatabase");
        if (db != null)
            return db;

        return CreateDefault();
    }

    private static AircraftTuningDatabase CreateDefault()
    {
        var db = CreateInstance<AircraftTuningDatabase>();

        var defaults = new List<AircraftTuning>
        {
            Build("F15", 90f, 80f, 95f),
            Build("Su27", 88f, 92f, 90f),
            Build("Mig29", 85f, 88f, 82f),
            Build("fa18e", 82f, 85f, 92f),
            Build("Hawk_200", 70f, 82f, 60f),
            Build("mig21", 88f, 75f, 65f),
            Build("panavia-tornado", 86f, 78f, 88f),
            Build("rafalemf3", 87f, 90f, 94f),
            Build("Typhoon", 89f, 92f, 91f),
        };

        db.aircraft = defaults;
        db.fallback = defaults[0];
        return db;
    }

    private static AircraftTuning Build(
        string aircraftId,
        float speedPercent,
        float mobilityPercent,
        float firepowerPercent
    )
    {
        float speedScale = speedPercent / BaseSpeedPercent;
        float mobilityScale = mobilityPercent / BaseMobilityPercent;
        float firepowerScale = firepowerPercent / BaseFirepowerPercent;

        var tuning = new AircraftTuning
        {
            aircraftId = aircraftId,
            maxThrust = BaseMaxThrust * speedScale,
            throttleSpeed = BaseThrottleSpeed * speedScale,
            gLimit = Mathf.Clamp(BaseGLimit * mobilityScale, 6f, 12f),
            gLimitPitch = Mathf.Clamp(BaseGLimitPitch * mobilityScale, 7f, 13f),
            missileReloadTime = Mathf.Clamp(BaseMissileReload / firepowerScale, 3.5f, 8.5f),
            missileDebounceTime = Mathf.Clamp(BaseMissileDebounce / firepowerScale, 0.1f, 0.25f),
            stallAOA = Mathf.Clamp(BaseStallAOA * mobilityScale, 10f, 20f),
            criticalAOA = Mathf.Clamp(BaseCriticalAOA * mobilityScale, 16f, 30f),
            stallMinSpeed = Mathf.Clamp(BaseStallMinSpeed * speedScale, 30f, 80f),
            stallCriticalSpeed = Mathf.Clamp(BaseStallCriticalSpeed * speedScale, 20f, 70f),
            aiMaxSpeed = Mathf.Clamp(BaseAiMaxSpeed * speedScale, 200f, 360f),
            aiMinSpeed = Mathf.Clamp(BaseAiMinSpeed * speedScale, 120f, 260f),
            aiRecoverSpeedMin = Mathf.Clamp(BaseAiRecoverMin * speedScale, 90f, 220f),
            aiRecoverSpeedMax = Mathf.Clamp(BaseAiRecoverMax * speedScale, 110f, 250f),
            aiGroundAvoidMinSpeed = Mathf.Clamp(BaseAiGroundAvoidMin * speedScale, 90f, 220f),
            aiGroundAvoidMaxSpeed = Mathf.Clamp(BaseAiGroundAvoidMax * speedScale, 110f, 250f),
            aiMissileLockDelay = Mathf.Clamp(BaseAiMissileLockDelay / firepowerScale, 0.6f, 1.5f),
            aiMissileCooldown = Mathf.Clamp(BaseAiMissileCooldown / firepowerScale, 3f, 8f),
            radarRangeMeters = Mathf.Clamp(BaseRadarRange * speedScale, 2500f, 5000f),
        };

        if (tuning.gLimitPitch < tuning.gLimit + 1f)
        {
            tuning.gLimitPitch = tuning.gLimit + 1f;
        }

        tuning.blackoutStartG = Mathf.Clamp(tuning.gLimit - 1f, 5f, 11f);
        tuning.blackoutFullG = Mathf.Clamp(
            Mathf.Max(tuning.blackoutStartG + 0.5f, tuning.gLimit),
            6f,
            12f
        );
        tuning.redoutStartG = -tuning.blackoutStartG;
        tuning.redoutFullG = -tuning.blackoutFullG;

        return tuning;
    }
}

[Serializable]
public class AircraftTuning
{
    public string aircraftId;
    public float maxThrust;
    public float throttleSpeed;
    public float gLimit;
    public float gLimitPitch;
    public float missileReloadTime;
    public float missileDebounceTime;
    public float stallAOA;
    public float criticalAOA;
    public float stallMinSpeed;
    public float stallCriticalSpeed;
    public float aiMaxSpeed;
    public float aiMinSpeed;
    public float aiRecoverSpeedMin;
    public float aiRecoverSpeedMax;
    public float aiGroundAvoidMinSpeed;
    public float aiGroundAvoidMaxSpeed;
    public float aiMissileLockDelay;
    public float aiMissileCooldown;
    public float radarRangeMeters;
    public float blackoutStartG;
    public float blackoutFullG;
    public float redoutStartG;
    public float redoutFullG;

    public bool Matches(string id)
    {
        return !string.IsNullOrWhiteSpace(aircraftId)
            && string.Equals(aircraftId, id, StringComparison.OrdinalIgnoreCase);
    }
}
