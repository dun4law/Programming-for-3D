using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Team
{
    Player,
    Enemy,
}

public class Plane : MonoBehaviour
{
    [Header("Team")]
    [SerializeField]
    public Team team = Team.Enemy;

    [SerializeField]
    float maxHealth;

    [SerializeField]
    float health;

    [SerializeField]
    float maxThrust;

    [SerializeField]
    float throttleSpeed;

    [SerializeField]
    float gLimit;

    [SerializeField]
    float gLimitPitch;

    [Header("Lift")]
    [SerializeField]
    float liftPower;

    [SerializeField]
    AnimationCurve liftAOACurve;

    [SerializeField]
    float inducedDrag;

    [SerializeField]
    AnimationCurve inducedDragCurve;

    [SerializeField]
    float rudderPower;

    [SerializeField]
    AnimationCurve rudderAOACurve;

    [SerializeField]
    AnimationCurve rudderInducedDragCurve;

    [SerializeField]
    float flapsLiftPower;

    [SerializeField]
    float flapsAOABias;

    [SerializeField]
    float flapsDrag;

    [SerializeField]
    float flapsRetractSpeed;

    [Header("Steering")]
    [SerializeField]
    Vector3 turnSpeed;

    [SerializeField]
    Vector3 turnAcceleration;

    [SerializeField]
    AnimationCurve steeringCurve;

    [Header("Drag")]
    [SerializeField]
    AnimationCurve dragForward;

    [SerializeField]
    AnimationCurve dragBack;

    [SerializeField]
    AnimationCurve dragLeft;

    [SerializeField]
    AnimationCurve dragRight;

    [SerializeField]
    AnimationCurve dragTop;

    [SerializeField]
    AnimationCurve dragBottom;

    [SerializeField]
    Vector3 angularDrag;

    [SerializeField]
    float airbrakeDrag;

    [Header("Camera")]
    [SerializeField]
    [Tooltip(
        "Custom camera offset for this aircraft. If zero, PlaneCamera will use its default offset."
    )]
    Vector3 cameraOffset = Vector3.zero;

    public Vector3 CameraOffset => cameraOffset;

    [Header("Misc")]
    [SerializeField]
    List<Collider> landingGear;

    [SerializeField]
    PhysicsMaterial landingGearBrakesMaterial;

    [SerializeField]
    List<GameObject> graphics;

    [SerializeField]
    GameObject damageEffect;

    [SerializeField]
    GameObject deathEffect;

    [SerializeField]
    bool flapsDeployed;

    [SerializeField]
    float initialSpeed;

    [Header("Weapons")]
    [SerializeField]
    List<Transform> hardpoints;

    [SerializeField]
    float missileReloadTime;

    [SerializeField]
    float missileDebounceTime;

    [SerializeField]
    GameObject missilePrefab;

    [SerializeField]
    Target target;

    [SerializeField]
    float lockRange;

    [SerializeField]
    float lockSpeed;

    [SerializeField]
    float lockAngle;

    [SerializeField]
    [Tooltip("Firing rate in Rounds Per Minute")]
    float cannonFireRate;

    [SerializeField]
    float cannonDebounceTime;

    [SerializeField]
    float cannonSpread;

    [SerializeField]
    Transform cannonSpawnPoint;

    [SerializeField]
    GameObject bulletPrefab;

    new PlaneAnimation animation;

    float throttleInput;
    Vector3 controlInput;

    Vector3 lastVelocity;
    bool hasLastVelocity;
    PhysicsMaterial landingGearDefaultMaterial;

    int missileIndex;
    List<float> missileReloadTimers;
    float missileDebounceTimer;
    Vector3 missileLockDirection;

    bool cannonFiring;
    float cannonDebounceTimer;
    float cannonFiringTimer;

    bool unlimitedAmmo = false;

    [Header("Debug")]
    [SerializeField]
    bool enableDeathReasonLogs = true;

    Plane lastDamageAttacker;
    string lastDamageWeapon;
    string lastDamageReason;
    float lastDamageAmount;
    float lastUnderAttackTime = -999f;
    float lastTargetLockedTime = -999f;
    bool wasMissileLocked = false;
    bool isDying = false;

    public float MaxHealth
    {
        get { return maxHealth; }
        set { maxHealth = Mathf.Max(0, value); }
    }

    public float Health
    {
        get { return health; }
        private set
        {
            health = Mathf.Clamp(value, 0, maxHealth);

            if (health <= MaxHealth * .5f && health > 0)
            {
                damageEffect.SetActive(true);
            }
            else
            {
                damageEffect.SetActive(false);
            }

            if (health == 0 && MaxHealth != 0 && !Dead)
            {
                Die();
            }
        }
    }

    public bool Dead { get; private set; }

    public Rigidbody Rigidbody { get; private set; }
    public float Throttle { get; private set; }
    public Vector3 EffectiveInput { get; private set; }
    public Vector3 Velocity { get; private set; }
    public Vector3 LocalVelocity { get; private set; }
    public Vector3 LocalGForce { get; private set; }
    public Vector3 LocalAngularVelocity { get; private set; }
    public float AngleOfAttack { get; private set; }
    public float AngleOfAttackYaw { get; private set; }
    public bool AirbrakeDeployed { get; private set; }

    public string AircraftType { get; private set; }
    public string Callsign { get; private set; }

    public string DisplayName
    {
        get
        {
            bool hasCallsign = !string.IsNullOrEmpty(Callsign);
            bool hasType = !string.IsNullOrEmpty(AircraftType);

            if (hasCallsign && hasType)
                return $"{Callsign} ({AircraftType})";
            if (hasCallsign)
                return Callsign;
            if (hasType)
                return AircraftType;
            return gameObject.name;
        }
    }

    public string ShortName => !string.IsNullOrEmpty(Callsign) ? Callsign : DisplayName;

    public void SetAircraftType(string aircraftType)
    {
        AircraftType = aircraftType;
        Debug.Log(
            $"[Plane] Aircraft type set to: {aircraftType} (GameObject: {gameObject.name}, Callsign: {Callsign ?? "none"})"
        );
    }

    public void SetCallsign(string callsign)
    {
        Callsign = callsign;
        Debug.Log($"[Plane] Callsign set to: {callsign} (AircraftType: {AircraftType ?? "none"})");
    }

    public bool FlapsDeployed
    {
        get { return flapsDeployed; }
        private set
        {
            flapsDeployed = value;

            foreach (var lg in landingGear)
            {
                lg.enabled = value;
            }
        }
    }

    public bool MissileLocked { get; private set; }
    public bool MissileTracking { get; private set; }
    public Target Target
    {
        get { return target; }
    }

    public void SetTarget(Target newTarget)
    {
        if (newTarget == GetComponent<Target>())
            return;
        target = newTarget;
    }

    public Vector3 MissileLockDirection
    {
        get { return Rigidbody.rotation * missileLockDirection; }
    }

    public void ApplyTuning(AircraftTuning tuning)
    {
        if (tuning == null)
            return;

        maxThrust = tuning.maxThrust;
        throttleSpeed = tuning.throttleSpeed;
        gLimit = tuning.gLimit;
        gLimitPitch = tuning.gLimitPitch;
        missileReloadTime = tuning.missileReloadTime;
        missileDebounceTime = tuning.missileDebounceTime;
    }

    void Start()
    {
        animation = GetComponent<PlaneAnimation>();
        Rigidbody = GetComponent<Rigidbody>();

        if (landingGear.Count > 0)
        {
            landingGearDefaultMaterial = landingGear[0].sharedMaterial;
        }

        missileReloadTimers = new List<float>(hardpoints.Count);

        foreach (var h in hardpoints)
        {
            missileReloadTimers.Add(0);
        }

        missileLockDirection = Vector3.forward;

        Rigidbody.linearVelocity = Rigidbody.rotation * new Vector3(0, 0, initialSpeed);

        if (team == Team.Player && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayEngineLoop();
            AudioManager.Instance.PlayGameMusic();
        }

        Velocity = Rigidbody.linearVelocity;
        lastVelocity = Velocity;
        LocalGForce = Vector3.zero;
        hasLastVelocity = true;
    }

    public void SetThrottleInput(float input)
    {
        if (Dead)
            return;
        throttleInput = input;
    }

    public void SetControlInput(Vector3 input)
    {
        if (Dead)
            return;
        controlInput = Vector3.ClampMagnitude(input, 1);
    }

    public void SetCannonInput(bool input)
    {
        if (Dead)
            return;

        bool wasCannonFiring = cannonFiring;
        cannonFiring = input;

        if (team == Team.Player && AudioManager.Instance != null)
        {
            if (cannonFiring && !wasCannonFiring)
            {
                AudioManager.Instance.StartCannonLoop();
            }
            else if (!cannonFiring && wasCannonFiring)
            {
                AudioManager.Instance.StopCannonLoop();
            }
        }
    }

    public void ToggleFlaps()
    {
        if (LocalVelocity.z < flapsRetractSpeed)
        {
            FlapsDeployed = !FlapsDeployed;
        }
    }

    public void DeployFlares()
    {
        if (Dead)
            return;

        var countermeasures = GetComponent<Countermeasures>();
        if (countermeasures != null)
        {
            bool deployed = countermeasures.DeployFlares();
            if (deployed)
            {
                Debug.Log($"[{name}] Flares deployed!");
            }
            else
            {
                Debug.Log($"[{name}] Cannot deploy flares (empty or cooling down)");
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] No Countermeasures component found!");
        }
    }

    public void ApplyDamage(float damage)
    {
        ApplyDamage(damage, null, null, null);
    }

    public void ApplyDamage(float damage, Plane attacker, string weapon, string reason)
    {
        if (Dead || isDying || Health <= 0)
            return;

        lastDamageAttacker = attacker;
        lastDamageWeapon = weapon;
        lastDamageReason = reason;
        lastDamageAmount = damage;

        float healthAfterDamage = Health - damage;
        if (healthAfterDamage <= 0)
        {
            isDying = true;
        }

        if (AudioManager.Instance != null)
        {
            if (weapon == "Missile" && team == Team.Player)
            {
                AudioManager.Instance.StopMissileIncoming();
            }

            AudioManager.Instance.PlayHit();

            if (team == Team.Player && damage >= 10f && Time.time - lastUnderAttackTime > 5f)
            {
                AudioManager.Instance.PlayUnderAttack();
                lastUnderAttackTime = Time.time;
            }
        }

        Health -= damage;
    }

    void Die()
    {
        throttleInput = 0;
        Throttle = 0;
        Dead = true;
        cannonFiring = false;

        damageEffect.GetComponent<ParticleSystem>().Pause();
        deathEffect.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosionAtPosition(transform.position);

            if (team == Team.Player)
            {
                AudioManager.Instance.StopEngineLoop();
                AudioManager.Instance.StopCannonLoop();
            }
        }

        if (enableDeathReasonLogs)
        {
            string killerName =
                lastDamageAttacker != null ? lastDamageAttacker.DisplayName : "Environment";
            string weaponName = string.IsNullOrWhiteSpace(lastDamageWeapon)
                ? "Unknown"
                : lastDamageWeapon;
            string reasonName = string.IsNullOrWhiteSpace(lastDamageReason)
                ? "Unknown"
                : lastDamageReason;
            Debug.Log(
                $" Plane died: victim={DisplayName}, killer={killerName}, weapon={weaponName}, reason={reasonName}, lastDamage={lastDamageAmount:F1}"
            );
        }

        bool victimIsPlayer = team == Team.Player;

        Debug.Log(
            $" Die() debug: name={DisplayName}, victimIsPlayer={victimIsPlayer}, GameManager.Instance={(GameManager.Instance != null ? "exists" : "null")}"
        );

        if (KillTracker.Instance != null)
        {
            bool killerIsPlayer =
                lastDamageAttacker != null && lastDamageAttacker.team == Team.Player;
            string killerName =
                lastDamageAttacker != null ? lastDamageAttacker.DisplayName : "Environment";
            var victimTarget = GetComponent<Target>();
            string victimName = victimTarget != null ? victimTarget.Name : DisplayName;
            string reasonName = string.IsNullOrWhiteSpace(lastDamageReason)
                ? null
                : lastDamageReason;

            KillTracker.Instance.RecordDeath(killerName, victimIsPlayer, reasonName);

            if (lastDamageAttacker != null && lastDamageAttacker != this)
            {
                KillTracker.Instance.RecordKill(killerName, victimName, killerIsPlayer);
            }
        }

        if (victimIsPlayer)
        {
            string killerDisplay =
                lastDamageAttacker != null ? lastDamageAttacker.ShortName : "Unknown";
            string weaponName = string.IsNullOrWhiteSpace(lastDamageWeapon)
                ? "Enemy Fire"
                : lastDamageWeapon;
            string reasonText =
                lastDamageAttacker != null
                    ? $"Shot down by {killerDisplay} ({weaponName})"
                    : $"Shot down by {weaponName}";

            if (GameManager.Instance == null)
            {
                var gm = FindAnyObjectByType<GameManager>();
                if (gm != null)
                {
                    Debug.Log("[Plane] Found GameManager via FindAnyObjectByType");
                }
            }

            if (GameManager.Instance != null)
            {
                Debug.Log($" Player died! Scheduling game over in 2 seconds. Reason: {reasonText}");
                GameManager.Instance.ScheduleGameOver(reasonText, 2f);
            }
            else
            {
                Debug.LogWarning(
                    " Player died but GameManager.Instance is null! Loading menu directly..."
                );
                StartCoroutine(FallbackGameOver(reasonText, 2f));
            }
        }
        else
        {
            Debug.Log($" Not triggering game over: victimIsPlayer={victimIsPlayer}");
        }
    }

    private System.Collections.IEnumerator FallbackGameOver(string reason, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        MainMenuController.ShouldStartImmediately = false;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu and story");
    }

    void UpdateThrottle(float dt)
    {
        float target = 0;
        if (throttleInput > 0)
            target = 1;

        Throttle = Utilities.MoveTo(Throttle, target, throttleSpeed * Mathf.Abs(throttleInput), dt);

        AirbrakeDeployed = Throttle == 0 && throttleInput == -1;

        if (AirbrakeDeployed)
        {
            foreach (var lg in landingGear)
            {
                lg.sharedMaterial = landingGearBrakesMaterial;
            }
        }
        else
        {
            foreach (var lg in landingGear)
            {
                lg.sharedMaterial = landingGearDefaultMaterial;
            }
        }
    }

    void UpdateFlaps()
    {
        if (LocalVelocity.z > flapsRetractSpeed)
        {
            FlapsDeployed = false;
        }
    }

    void CalculateAngleOfAttack()
    {
        if (LocalVelocity.sqrMagnitude < 0.1f)
        {
            AngleOfAttack = 0;
            AngleOfAttackYaw = 0;
            return;
        }

        AngleOfAttack = Mathf.Atan2(-LocalVelocity.y, LocalVelocity.z);
        AngleOfAttackYaw = Mathf.Atan2(LocalVelocity.x, LocalVelocity.z);
    }

    void CalculateGForce(float dt)
    {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        if (!hasLastVelocity)
        {
            lastVelocity = Velocity;
            LocalGForce = Vector3.zero;
            hasLastVelocity = true;
            return;
        }
        var acceleration = (Velocity - lastVelocity) / dt;
        LocalGForce = invRotation * acceleration;
        lastVelocity = Velocity;
    }

    void CalculateState(float dt)
    {
        var invRotation = Quaternion.Inverse(Rigidbody.rotation);
        Velocity = Rigidbody.linearVelocity;
        LocalVelocity = invRotation * Velocity;
        LocalAngularVelocity = invRotation * Rigidbody.angularVelocity;

        CalculateAngleOfAttack();
    }

    void UpdateThrust()
    {
        Rigidbody.AddRelativeForce(Throttle * maxThrust * Vector3.forward);
    }

    void UpdateDrag()
    {
        var lv = LocalVelocity;
        var lv2 = lv.sqrMagnitude;

        float airbrakeDrag = AirbrakeDeployed ? this.airbrakeDrag : 0;
        float flapsDrag = FlapsDeployed ? this.flapsDrag : 0;

        var coefficient = Utilities.Scale6(
            lv.normalized,
            dragRight.Evaluate(Mathf.Abs(lv.x)),
            dragLeft.Evaluate(Mathf.Abs(lv.x)),
            dragTop.Evaluate(Mathf.Abs(lv.y)),
            dragBottom.Evaluate(Mathf.Abs(lv.y)),
            dragForward.Evaluate(Mathf.Abs(lv.z)) + airbrakeDrag + flapsDrag,
            dragBack.Evaluate(Mathf.Abs(lv.z))
        );

        var drag = coefficient.magnitude * lv2 * -lv.normalized;

        Rigidbody.AddRelativeForce(drag);
    }

    Vector3 CalculateLift(
        float angleOfAttack,
        Vector3 rightAxis,
        float liftPower,
        AnimationCurve aoaCurve,
        AnimationCurve inducedDragCurve
    )
    {
        var liftVelocity = Vector3.ProjectOnPlane(LocalVelocity, rightAxis);
        var v2 = liftVelocity.sqrMagnitude;

        var liftCoefficient = aoaCurve.Evaluate(angleOfAttack * Mathf.Rad2Deg);
        var liftForce = v2 * liftCoefficient * liftPower;

        var liftDirection = Vector3.Cross(liftVelocity.normalized, rightAxis);
        var lift = liftDirection * liftForce;

        var dragForce = liftCoefficient * liftCoefficient;
        var dragDirection = -liftVelocity.normalized;
        var inducedDrag =
            dragDirection
            * v2
            * dragForce
            * this.inducedDrag
            * inducedDragCurve.Evaluate(Mathf.Max(0, LocalVelocity.z));

        return lift + inducedDrag;
    }

    void UpdateLift()
    {
        if (LocalVelocity.sqrMagnitude < 1f)
            return;

        float flapsLiftPower = FlapsDeployed ? this.flapsLiftPower : 0;
        float flapsAOABias = FlapsDeployed ? this.flapsAOABias : 0;

        var liftForce = CalculateLift(
            AngleOfAttack + (flapsAOABias * Mathf.Deg2Rad),
            Vector3.right,
            liftPower + flapsLiftPower,
            liftAOACurve,
            inducedDragCurve
        );

        var yawForce = CalculateLift(
            AngleOfAttackYaw,
            Vector3.up,
            rudderPower,
            rudderAOACurve,
            rudderInducedDragCurve
        );

        Rigidbody.AddRelativeForce(liftForce);
        Rigidbody.AddRelativeForce(yawForce);
    }

    void UpdateAngularDrag()
    {
        var av = LocalAngularVelocity;
        var drag = av.sqrMagnitude * -av.normalized;
        Rigidbody.AddRelativeTorque(Vector3.Scale(drag, angularDrag), ForceMode.Acceleration);
    }

    Vector3 CalculateGForce(Vector3 angularVelocity, Vector3 velocity)
    {
        return Vector3.Cross(angularVelocity, velocity);
    }

    Vector3 CalculateGForceLimit(Vector3 input)
    {
        return Utilities.Scale6(input, gLimit, gLimitPitch, gLimit, gLimit, gLimit, gLimit) * 9.81f;
    }

    float CalculateGLimiter(Vector3 controlInput, Vector3 maxAngularVelocity)
    {
        if (controlInput.magnitude < 0.01f)
        {
            return 1;
        }

        var maxInput = controlInput.normalized;

        var limit = CalculateGForceLimit(maxInput);
        var maxGForce = CalculateGForce(Vector3.Scale(maxInput, maxAngularVelocity), LocalVelocity);

        if (maxGForce.magnitude > limit.magnitude)
        {
            return limit.magnitude / maxGForce.magnitude;
        }

        return 1;
    }

    float CalculateSteering(
        float dt,
        float angularVelocity,
        float targetVelocity,
        float acceleration
    )
    {
        var error = targetVelocity - angularVelocity;
        var accel = acceleration * dt;
        return Mathf.Clamp(error, -accel, accel);
    }

    void UpdateSteering(float dt)
    {
        var speed = Mathf.Max(0, LocalVelocity.z);
        var steeringPower = steeringCurve.Evaluate(speed);

        var gForceScaling = CalculateGLimiter(
            controlInput,
            turnSpeed * Mathf.Deg2Rad * steeringPower
        );

        var targetAV = Vector3.Scale(controlInput, turnSpeed * steeringPower * gForceScaling);
        var av = LocalAngularVelocity * Mathf.Rad2Deg;

        var correction = new Vector3(
            CalculateSteering(dt, av.x, targetAV.x, turnAcceleration.x * steeringPower),
            CalculateSteering(dt, av.y, targetAV.y, turnAcceleration.y * steeringPower),
            CalculateSteering(dt, av.z, targetAV.z, turnAcceleration.z * steeringPower)
        );

        Rigidbody.AddRelativeTorque(correction * Mathf.Deg2Rad, ForceMode.VelocityChange);

        var correctionInput = new Vector3(
            Mathf.Clamp((targetAV.x - av.x) / turnAcceleration.x, -1, 1),
            Mathf.Clamp((targetAV.y - av.y) / turnAcceleration.y, -1, 1),
            Mathf.Clamp((targetAV.z - av.z) / turnAcceleration.z, -1, 1)
        );

        var effectiveInput = (correctionInput + controlInput) * gForceScaling;

        EffectiveInput = new Vector3(
            Mathf.Clamp(effectiveInput.x, -1, 1),
            Mathf.Clamp(effectiveInput.y, -1, 1),
            Mathf.Clamp(effectiveInput.z, -1, 1)
        );
    }

    public void TryFireMissile()
    {
        if (Dead)
            return;

        if (hardpoints == null || hardpoints.Count == 0)
            return;

        if (missilePrefab == null)
            return;

        for (int i = 0; i < hardpoints.Count; i++)
        {
            var index = (missileIndex + i) % hardpoints.Count;

            bool canFire = unlimitedAmmo || missileReloadTimers[index] == 0;

            if (missileDebounceTimer == 0 && canFire)
            {
                FireMissile(index);

                missileIndex = (index + 1) % hardpoints.Count;

                if (!unlimitedAmmo)
                {
                    missileReloadTimers[index] = missileReloadTime;
                }

                missileDebounceTimer = missileDebounceTime;

                animation.ShowMissileGraphic(index, false);
                break;
            }
        }
    }

    void FireMissile(int index)
    {
        var hardpoint = hardpoints[index];
        var missileGO = Instantiate(missilePrefab, hardpoint.position, hardpoint.rotation);
        var missile = missileGO.GetComponent<Missile>();
        var launchTarget = MissileLocked ? Target : null;

        Target warningTarget = null;
        if (
            team != Team.Player
            && Target != null
            && Target.Plane != null
            && Target.Plane.team == Team.Player
        )
        {
            warningTarget = Target;
        }

        missile.Launch(this, launchTarget, warningTarget);

        if (team == Team.Player && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMissileFire();
        }

        if (KillTracker.Instance != null)
        {
            KillTracker.Instance.RecordMissileFired();
        }

        string ownerName = DisplayName;
        string targetName = launchTarget != null ? launchTarget.Name : "None";
        Debug.Log(
            $" Missile fired: owner={ownerName}, target={targetName}, missileId={missile.LaunchId}"
        );
    }

    void UpdateWeapons(float dt)
    {
        UpdateWeaponCooldown(dt);
        UpdateMissileLock(dt);
        UpdateCannon(dt);
    }

    void UpdateWeaponCooldown(float dt)
    {
        missileDebounceTimer = Mathf.Max(0, missileDebounceTimer - dt);
        cannonDebounceTimer = Mathf.Max(0, cannonDebounceTimer - dt);
        cannonFiringTimer = Mathf.Max(0, cannonFiringTimer - dt);

        for (int i = 0; i < missileReloadTimers.Count; i++)
        {
            missileReloadTimers[i] = Mathf.Max(0, missileReloadTimers[i] - dt);

            if (missileReloadTimers[i] == 0)
            {
                animation.ShowMissileGraphic(i, true);
            }
        }
    }

    void UpdateMissileLock(float dt)
    {
        Vector3 targetDir = Vector3.forward;
        MissileTracking = false;

        if (Target != null && Target.Plane != null && !Target.Plane.Dead)
        {
            var error = target.Position - Rigidbody.position;
            var errorDir = Quaternion.Inverse(Rigidbody.rotation) * error.normalized;

            if (
                error.magnitude <= lockRange
                && Vector3.Angle(Vector3.forward, errorDir) <= lockAngle
            )
            {
                MissileTracking = true;
                targetDir = errorDir;
            }
        }

        missileLockDirection = Vector3.RotateTowards(
            missileLockDirection,
            targetDir,
            Mathf.Deg2Rad * lockSpeed * dt,
            0
        );

        MissileLocked =
            Target != null
            && MissileTracking
            && Vector3.Angle(missileLockDirection, targetDir) < lockSpeed * dt;

        if (team == Team.Player && MissileLocked && !wasMissileLocked)
        {
            if (AudioManager.Instance != null && Time.time - lastTargetLockedTime > 0.3f)
            {
                AudioManager.Instance.PlayTargetLocked();
                lastTargetLockedTime = Time.time;
            }
        }
        wasMissileLocked = MissileLocked;
    }

    void UpdateCannon(float dt)
    {
        if (cannonFiring && cannonFiringTimer == 0)
        {
            if (bulletPrefab == null)
                return;
            if (cannonSpawnPoint == null)
                return;

            cannonFiringTimer = 60f / cannonFireRate;

            var spread = Random.insideUnitCircle * cannonSpread;

            var bulletGO = Instantiate(
                bulletPrefab,
                cannonSpawnPoint.position,
                cannonSpawnPoint.rotation * Quaternion.Euler(spread.x, spread.y, 0)
            );
            var bullet = bulletGO.GetComponent<Bullet>();
            bullet.Fire(this);

            if (team != Team.Player && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayCannonFire();
            }

            if (KillTracker.Instance != null)
            {
                KillTracker.Instance.RecordBulletFired();
            }
        }
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        CalculateState(dt);
        CalculateGForce(dt);
        UpdateFlaps();

        UpdateThrottle(dt);

        if (!Dead)
        {
            UpdateThrust();
            UpdateLift();
            UpdateSteering(dt);
        }
        else
        {
            Vector3 velocity = Rigidbody.linearVelocity;
            if (velocity.sqrMagnitude > 0.0001f)
            {
                Vector3 up = Rigidbody.rotation * Vector3.up;
                Rigidbody.rotation = Quaternion.LookRotation(velocity.normalized, up);
            }
        }

        UpdateDrag();
        UpdateAngularDrag();

        if (team == Team.Player && AudioManager.Instance != null)
        {
            float enginePitch = Mathf.Lerp(0.8f, 1.5f, Throttle);
            AudioManager.Instance.SetEnginePitch(enginePitch);
        }

        CalculateState(dt);

        UpdateWeapons(dt);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Dead)
            return;

        ContactPoint crashContact = default;
        bool shouldCrash = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);
            if (!landingGear.Contains(contact.thisCollider))
            {
                crashContact = contact;
                shouldCrash = true;
                break;
            }
        }

        if (!shouldCrash)
            return;

        lastDamageAttacker = null;
        lastDamageWeapon = "Collision";
        lastDamageReason = "Crash";
        lastDamageAmount = Health;

        Health = 0;

        Rigidbody.isKinematic = true;
        Rigidbody.position = crashContact.point;
        Rigidbody.rotation = Quaternion.Euler(0, Rigidbody.rotation.eulerAngles.y, 0);

        foreach (var go in graphics)
        {
            go.SetActive(false);
        }
    }

    public void SetUnlimitedAmmo(bool enabled)
    {
        unlimitedAmmo = enabled;

        if (enabled && missileReloadTimers != null)
        {
            for (int i = 0; i < missileReloadTimers.Count; i++)
            {
                missileReloadTimers[i] = 0f;
                if (animation != null)
                {
                    animation.ShowMissileGraphic(i, true);
                }
            }
        }

        Debug.Log($"[Plane] Unlimited Ammo: {(enabled ? "ENABLED" : "DISABLED")}");
    }

    public void SetHardpoints(List<Transform> points)
    {
        hardpoints = points ?? new List<Transform>();

        missileReloadTimers = new List<float>(hardpoints.Count);
        for (int i = 0; i < hardpoints.Count; i++)
        {
            missileReloadTimers.Add(0f);
        }
    }

    public void SetCannonSpawnPoint(Transform point)
    {
        cannonSpawnPoint = point;
    }
}
