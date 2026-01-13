using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIController : MonoBehaviour
{
    [SerializeField]
    Plane plane;

    [SerializeField]
    float steeringSpeed;

    [Header("Steering Tuning")]
    [SerializeField]
    float pitchFullInputAngle = 30f;

    [SerializeField]
    float minSpeed;

    [SerializeField]
    float maxSpeed;

    [SerializeField]
    float recoverSpeedMin;

    [SerializeField]
    float recoverSpeedMax;

    [SerializeField]
    LayerMask groundCollisionMask;

    [SerializeField]
    float groundCollisionDistance;

    [SerializeField]
    float groundAvoidanceAngle;

    [SerializeField]
    float groundAvoidanceMinSpeed;

    [SerializeField]
    float groundAvoidanceMaxSpeed;

    [SerializeField]
    float pitchUpThreshold;

    [SerializeField]
    float fineSteeringAngle;

    [SerializeField]
    float rollFactor;

    [SerializeField]
    float yawFactor;

    [SerializeField]
    bool canUseMissiles;

    [SerializeField]
    bool canUseCannon;

    [SerializeField]
    float missileLockFiringDelay;

    [SerializeField]
    float missileFiringCooldown;

    [SerializeField]
    float missileMinRange;

    [SerializeField]
    float missileMaxRange;

    [SerializeField]
    float missileMaxFireAngle;

    [SerializeField]
    float bulletSpeed;

    [SerializeField]
    float cannonRange;

    [SerializeField]
    float cannonMaxFireAngle;

    [SerializeField]
    float cannonBurstLength;

    [SerializeField]
    float cannonBurstCooldown;

    [SerializeField]
    float minMissileDodgeDistance;

    [SerializeField]
    [Tooltip(
        "If true, AI will deploy countermeasures when missiles are incoming. Only works for player team."
    )]
    bool canUseCountermeasures = false;

    [SerializeField]
    float countermeasureCooldown = 2f;

    [SerializeField]
    float reactionDelayMin;

    [SerializeField]
    float reactionDelayMax;

    [SerializeField]
    float reactionDelayDistance;

    [Header("Targeting")]
    [SerializeField]
    bool autoAcquireTarget = true;

    [SerializeField]
    float retargetInterval = 1f;

    [SerializeField]
    float maxAcquireRange = 10000f;

    [SerializeField]
    float pursuitLeadFactor = 0.6f;

    [SerializeField]
    float pursuitMaxLeadTime = 2f;

    Target selfTarget;
    Plane targetPlane;
    Vector3 lastInput;
    bool isRecoveringSpeed;

    float missileDelayTimer;
    float missileCooldownTimer;
    bool wasMissileLocked;
    Target lastMissileTarget;

    bool cannonFiring;
    float cannonBurstTimer;
    float cannonCooldownTimer;

    Countermeasures countermeasures;
    float lastCountermeasureTime;

    struct ControlInput
    {
        public float time;
        public Vector3 targetPosition;
    }

    Queue<ControlInput> inputQueue;

    bool dodging;
    Vector3 lastDodgePoint;
    List<Vector3> dodgeOffsets;
    const float dodgeUpdateInterval = 0.25f;
    float dodgeTimer;

    float retargetTimer;

    void Start()
    {
        if (plane == null)
        {
            plane = GetComponent<Plane>();
        }
        if (plane == null)
        {
            enabled = false;
            return;
        }

        selfTarget = plane.GetComponent<Target>();
        countermeasures = plane.GetComponent<Countermeasures>();

        if (canUseCountermeasures && plane.team == Team.Player && countermeasures == null)
        {
            Debug.LogWarning(
                $"[AIController] canUseCountermeasures is enabled but no Countermeasures component found on {plane.DisplayName}"
            );
        }

        UpdateTargetPlane();

        dodgeOffsets = new List<Vector3>();
        inputQueue = new Queue<ControlInput>();
    }

    public void ApplyTuning(AircraftTuning tuning)
    {
        if (tuning == null)
            return;

        maxSpeed = tuning.aiMaxSpeed;
        minSpeed = Mathf.Min(tuning.aiMinSpeed, maxSpeed - 5f);

        recoverSpeedMin = Mathf.Min(tuning.aiRecoverSpeedMin, maxSpeed - 10f);
        recoverSpeedMax = Mathf.Min(tuning.aiRecoverSpeedMax, maxSpeed - 5f);

        groundAvoidanceMinSpeed = Mathf.Min(tuning.aiGroundAvoidMinSpeed, maxSpeed - 10f);
        groundAvoidanceMaxSpeed = Mathf.Min(tuning.aiGroundAvoidMaxSpeed, maxSpeed - 5f);

        missileLockFiringDelay = tuning.aiMissileLockDelay;
        missileFiringCooldown = tuning.aiMissileCooldown;
    }

    public void SetCanUseCountermeasures(bool enabled)
    {
        canUseCountermeasures = enabled;
        Debug.Log(
            $"[AIController] {(plane != null ? plane.DisplayName : name)} canUseCountermeasures set to {enabled}"
        );
    }

    void UpdateTargetPlane()
    {
        var currentTarget = plane.Target;
        targetPlane = currentTarget != null ? currentTarget.GetComponent<Plane>() : null;
    }

    void RefreshTarget(float dt)
    {
        if (!autoAcquireTarget || plane == null)
            return;
        if (plane.Rigidbody == null)
            return;

        retargetTimer = Mathf.Max(0, retargetTimer - dt);
        if (retargetTimer > 0)
            return;
        retargetTimer = retargetInterval;

        if (plane.Target != null && (plane.Target.Plane == null || plane.Target.Plane.Dead))
        {
            plane.SetTarget(null);
        }

        if (plane.Target == null)
        {
            Target best = null;
            float bestDist = float.PositiveInfinity;

            var targets = FindObjectsByType<Target>(FindObjectsSortMode.None);
            foreach (var t in targets)
            {
                if (t == null || t == selfTarget)
                    continue;
                if (t.Plane == null || t.Plane.Dead)
                    continue;
                if (plane.team == t.Plane.team)
                    continue;

                float dist = Vector3.Distance(t.Position, plane.Rigidbody.position);
                if (dist > maxAcquireRange)
                    continue;

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = t;
                }
            }

            if (best != null)
            {
                plane.SetTarget(best);
            }
        }

        UpdateTargetPlane();
    }

    Vector3 AvoidGround()
    {
        var roll = plane.Rigidbody.rotation.eulerAngles.z;
        if (roll > 180f)
            roll -= 360f;
        return new Vector3(-1, 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    Vector3 RecoverSpeed()
    {
        var roll = plane.Rigidbody.rotation.eulerAngles.z;
        var pitch = plane.Rigidbody.rotation.eulerAngles.x;
        if (roll > 180f)
            roll -= 360f;
        if (pitch > 180f)
            pitch -= 360f;
        return new Vector3(Mathf.Clamp(-pitch, -1, 1), 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    Vector3 GetTargetPosition()
    {
        if (plane.Target == null)
        {
            return plane.Rigidbody.position;
        }

        var targetPosition = plane.Target.Position;
        var targetVelocity = plane.Target.Velocity;

        var error = targetPosition - plane.Rigidbody.position;
        float distance = error.magnitude;
        float ownSpeed = Mathf.Max(1f, plane.Rigidbody.linearVelocity.magnitude);
        float leadTime = Mathf.Clamp(
            (distance / ownSpeed) * pursuitLeadFactor,
            0f,
            pursuitMaxLeadTime
        );
        var ledTargetPosition = targetPosition + targetVelocity * leadTime;

        if (distance < cannonRange)
        {
            return Utilities.FirstOrderIntercept(
                plane.Rigidbody.position,
                plane.Rigidbody.linearVelocity,
                bulletSpeed,
                targetPosition,
                targetVelocity
            );
        }

        return ledTargetPosition;
    }

    Vector3 CalculateSteering(float dt, Vector3 targetPosition)
    {
        if (
            plane.Target != null
            && (targetPlane == null || targetPlane.Dead || targetPlane.Health == 0)
        )
        {
            return new Vector3();
        }

        var error = targetPosition - plane.Rigidbody.position;
        error = Quaternion.Inverse(plane.Rigidbody.rotation) * error;

        if (error.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        var errorDir = error.normalized;
        var pitchError = new Vector3(0, error.y, error.z).normalized;
        var rollError = new Vector3(error.x, error.y, 0).normalized;
        var yawError = new Vector3(error.x, 0, error.z).normalized;

        var targetInput = new Vector3();

        var pitch = Vector3.SignedAngle(Vector3.forward, pitchError, Vector3.right);
        if (-pitch < pitchUpThreshold)
            pitch += 360f;
        targetInput.x = pitchFullInputAngle > 0 ? (pitch / pitchFullInputAngle) : pitch;

        if (Vector3.Angle(Vector3.forward, errorDir) < fineSteeringAngle)
        {
            var yaw = Vector3.SignedAngle(Vector3.forward, yawError, Vector3.up);

            targetInput.y =
                fineSteeringAngle > 0 ? ((yaw / fineSteeringAngle) * yawFactor) : (yaw * yawFactor);
        }
        else
        {
            var roll = Vector3.SignedAngle(Vector3.up, rollError, Vector3.forward);
            targetInput.z = roll * rollFactor;
        }

        targetInput.x = Mathf.Clamp(targetInput.x, -1, 1);
        targetInput.y = Mathf.Clamp(targetInput.y, -1, 1);
        targetInput.z = Mathf.Clamp(targetInput.z, -1, 1);

        var input = Vector3.MoveTowards(lastInput, targetInput, steeringSpeed * dt);
        lastInput = input;

        return input;
    }

    Vector3 GetMissileDodgePosition(float dt, Missile missile)
    {
        dodgeTimer = Mathf.Max(0, dodgeTimer - dt);
        var missilePos = missile.Rigidbody.position;

        var dist = Mathf.Max(
            minMissileDodgeDistance,
            Vector3.Distance(missilePos, plane.Rigidbody.position)
        );

        if (dodgeTimer == 0)
        {
            var missileForward = missile.Rigidbody.rotation * Vector3.forward;
            dodgeOffsets.Clear();

            dodgeOffsets.Add(new Vector3(0, dist, 0));
            dodgeOffsets.Add(new Vector3(0, -dist, 0));
            dodgeOffsets.Add(Vector3.Cross(missileForward, Vector3.up) * dist);
            dodgeOffsets.Add(Vector3.Cross(missileForward, Vector3.up) * -dist);

            dodgeTimer = dodgeUpdateInterval;
        }

        float min = float.PositiveInfinity;
        Vector3 minDodge = missilePos + dodgeOffsets[0];

        foreach (var offset in dodgeOffsets)
        {
            var dodgePosition = missilePos + offset;
            var offsetDist = Vector3.Distance(dodgePosition, lastDodgePoint);

            if (offsetDist < min)
            {
                minDodge = dodgePosition;
                min = offsetDist;
            }
        }

        lastDodgePoint = minDodge;
        return minDodge;
    }

    float CalculateThrottle(float minSpeed, float maxSpeed)
    {
        float input = 0;

        if (plane.LocalVelocity.z < minSpeed)
        {
            input = 1;
        }
        else if (plane.LocalVelocity.z > maxSpeed)
        {
            input = -1;
        }

        return input;
    }

    void CalculateWeapons(float dt)
    {
        if (plane.Target == null)
            return;
        if (targetPlane == null)
        {
            UpdateTargetPlane();
        }
        if (targetPlane == null || targetPlane.Dead || targetPlane.Health == 0)
        {
            if (cannonFiring)
            {
                cannonFiring = false;
                plane.SetCannonInput(false);
            }
            return;
        }

        if (canUseMissiles)
        {
            CalculateMissiles(dt);
        }

        if (canUseCannon)
        {
            CalculateCannon(dt);
        }
    }

    void CalculateMissiles(float dt)
    {
        missileDelayTimer = Mathf.Max(0, missileDelayTimer - dt);
        missileCooldownTimer = Mathf.Max(0, missileCooldownTimer - dt);

        if (plane.Target != lastMissileTarget)
        {
            lastMissileTarget = plane.Target;
            missileDelayTimer = missileLockFiringDelay;
            wasMissileLocked = false;
        }

        var error = plane.Target.Position - plane.Rigidbody.position;
        var range = error.magnitude;
        var targetDir = error.normalized;
        var targetAngle = Vector3.Angle(targetDir, plane.Rigidbody.rotation * Vector3.forward);

        if (
            !plane.MissileLocked
            || !(targetAngle < missileMaxFireAngle || (180f - targetAngle) < missileMaxFireAngle)
        )
        {
            missileDelayTimer = missileLockFiringDelay;
        }

        if (plane.MissileLocked && !wasMissileLocked)
        {
            missileDelayTimer = missileLockFiringDelay;
        }
        wasMissileLocked = plane.MissileLocked;

        if (
            range < missileMaxRange
            && range > missileMinRange
            && missileDelayTimer == 0
            && missileCooldownTimer == 0
        )
        {
            plane.TryFireMissile();
            missileCooldownTimer = missileFiringCooldown;
        }
    }

    void CalculateCannon(float dt)
    {
        if (targetPlane == null || targetPlane.Dead || targetPlane.Health == 0)
        {
            cannonFiring = false;
            return;
        }

        if (cannonFiring)
        {
            cannonBurstTimer = Mathf.Max(0, cannonBurstTimer - dt);

            if (cannonBurstTimer == 0)
            {
                cannonFiring = false;
                cannonCooldownTimer = cannonBurstCooldown;
                plane.SetCannonInput(false);
            }
        }
        else
        {
            cannonCooldownTimer = Mathf.Max(0, cannonCooldownTimer - dt);

            var targetPosition = Utilities.FirstOrderIntercept(
                plane.Rigidbody.position,
                plane.Rigidbody.linearVelocity,
                bulletSpeed,
                plane.Target.Position,
                plane.Target.Velocity
            );

            var error = targetPosition - plane.Rigidbody.position;
            var range = error.magnitude;
            var targetDir = error.normalized;
            var targetAngle = Vector3.Angle(targetDir, plane.Rigidbody.rotation * Vector3.forward);

            if (range < cannonRange && targetAngle < cannonMaxFireAngle && cannonCooldownTimer == 0)
            {
                cannonFiring = true;
                cannonBurstTimer = cannonBurstLength;
                plane.SetCannonInput(true);
            }
        }
    }

    void SteerToTarget(float dt, Vector3 targetPositionNow)
    {
        bool foundTarget = false;
        Vector3 steering = Vector3.zero;
        Vector3 targetPosition = Vector3.zero;

        var delay = reactionDelayMax;

        if (Vector3.Distance(targetPositionNow, plane.Rigidbody.position) < reactionDelayDistance)
        {
            delay = reactionDelayMin;
        }

        while (inputQueue.Count > 0)
        {
            var input = inputQueue.Peek();

            if (input.time + delay <= Time.time)
            {
                targetPosition = input.targetPosition;
                inputQueue.Dequeue();
                foundTarget = true;
            }
            else
            {
                break;
            }
        }

        if (foundTarget)
        {
            steering = CalculateSteering(dt, targetPosition);
        }

        plane.SetControlInput(steering);
    }

    void FixedUpdate()
    {
        if (plane.Dead)
            return;
        if (plane.Rigidbody == null)
            return;
        var dt = Time.fixedDeltaTime;

        RefreshTarget(dt);

        Vector3 steering = Vector3.zero;
        float throttle;
        bool emergency = false;
        Vector3 targetPosition =
            plane.Target != null
                ? plane.Target.Position
                : (plane.Rigidbody.position + plane.Rigidbody.rotation * Vector3.forward * 1000f);

        Vector3 velocity = plane.Rigidbody.linearVelocity;
        Vector3 forward =
            velocity.sqrMagnitude > 0.0001f
                ? velocity.normalized
                : (plane.Rigidbody.rotation * Vector3.forward);
        var velocityRot = Quaternion.LookRotation(forward);
        var ray = new Ray(
            plane.Rigidbody.position,
            velocityRot * Quaternion.Euler(groundAvoidanceAngle, 0, 0) * Vector3.forward
        );

        float rayDistance = groundCollisionDistance + Mathf.Max(0f, plane.LocalVelocity.z);
        if (
            Physics.Raycast(
                ray,
                rayDistance,
                groundCollisionMask.value,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            steering = AvoidGround();
            throttle = CalculateThrottle(groundAvoidanceMinSpeed, groundAvoidanceMaxSpeed);
            emergency = true;
        }
        else
        {
            var incomingMissile = selfTarget.GetIncomingMissile();
            if (incomingMissile != null)
            {
                if (dodging == false)
                {
                    dodging = true;
                    lastDodgePoint = plane.Rigidbody.position;
                    dodgeTimer = 0;
                    Debug.Log(
                        $"[AIController] {plane.DisplayName} detected incoming missile, starting dodge! canUseCountermeasures={canUseCountermeasures}, team={plane.team}, countermeasures={(countermeasures != null ? "OK" : "NULL")}"
                    );
                }

                TryDeployCountermeasures(incomingMissile);

                var dodgePosition = GetMissileDodgePosition(dt, incomingMissile);
                steering = CalculateSteering(dt, dodgePosition);
                emergency = true;
            }
            else
            {
                dodging = false;
                targetPosition = GetTargetPosition();
            }

            if (
                incomingMissile == null
                && (plane.LocalVelocity.z < recoverSpeedMin || isRecoveringSpeed)
            )
            {
                isRecoveringSpeed = plane.LocalVelocity.z < recoverSpeedMax;

                steering = RecoverSpeed();
                throttle = 1;
                emergency = true;
            }
            else
            {
                throttle = CalculateThrottle(minSpeed, maxSpeed);
            }
        }

        inputQueue.Enqueue(new ControlInput { time = Time.time, targetPosition = targetPosition });

        plane.SetThrottleInput(throttle);

        if (plane.Target == null)
        {
            inputQueue.Clear();
            lastInput = Vector3.zero;
        }

        if (emergency)
        {
            if (isRecoveringSpeed)
            {
                steering.x = Mathf.Clamp(steering.x, -0.5f, 0.5f);
            }

            plane.SetControlInput(steering);
        }
        else
        {
            SteerToTarget(dt, targetPosition);
        }

        CalculateWeapons(dt);
    }

    private const float OptimalFlareDeployDistance = 120f;
    private const float MinFlareDeployDistance = 50f;

    void TryDeployCountermeasures(Missile incomingMissile)
    {
        if (!canUseCountermeasures)
        {
            if (Time.frameCount % 300 == 0)
                Debug.Log($"[AIController] {plane.DisplayName} canUseCountermeasures=false");
            return;
        }
        if (plane.team != Team.Player)
        {
            return;
        }
        if (countermeasures == null)
        {
            if (Time.frameCount % 300 == 0)
                Debug.Log($"[AIController] {plane.DisplayName} countermeasures component is NULL");
            return;
        }
        if (Time.time - lastCountermeasureTime < countermeasureCooldown)
            return;

        if (incomingMissile != null && incomingMissile.Rigidbody != null)
        {
            float distanceToMissile = Vector3.Distance(
                plane.Rigidbody.position,
                incomingMissile.Rigidbody.position
            );

            if (distanceToMissile > OptimalFlareDeployDistance)
            {
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log(
                        $"[AIController] {plane.DisplayName} waiting to deploy flares - missile at {distanceToMissile:F0}m (optimal: {OptimalFlareDeployDistance}m)"
                    );
                }
                return;
            }

            if (distanceToMissile < MinFlareDeployDistance)
            {
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log(
                        $"[AIController] {plane.DisplayName} missile too close ({distanceToMissile:F0}m) - flares may not help"
                    );
                }
            }
        }

        Debug.Log(
            $"[AIController] {plane.DisplayName} deploying countermeasures at optimal distance! (Team: {plane.team})"
        );

        bool deployed = countermeasures.DeployFlares();
        if (deployed)
        {
            lastCountermeasureTime = Time.time;
            Debug.Log(
                $"[AIController] {plane.DisplayName} deployed countermeasures against incoming missile!"
            );
        }
    }
}
