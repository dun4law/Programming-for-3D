using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField]
    float lifetime;

    [SerializeField]
    float speed;

    [SerializeField]
    float trackingAngle;

    [SerializeField]
    float damage;

    [SerializeField]
    float damageRadius;

    [SerializeField]
    float turningGForce;

    [Header("Flare Countermeasures")]
    [SerializeField]
    [Tooltip("Maximum distance to detect flares.")]
    float flareDetectionRange = 150f;

    [SerializeField]
    [Tooltip("Base chance (0-1) to be distracted by a flare when in range.")]
    float flareDistractionChance = 0.9f;

    [SerializeField]
    [Tooltip("How often (seconds) the missile checks for flares.")]
    float flareCheckInterval = 0.15f;

    [Header("Hit Assist")]
    [SerializeField]
    [Tooltip("Multiplier for the explosion radius used for proximity fuse detection.")]
    float proximityFuseRadiusMultiplier = 1.2f;

    [SerializeField]
    [Tooltip("Radius for SphereCast to make high-speed proximity hits easier to detect.")]
    float collisionSphereCastRadius = 0.2f;

    [Header("Debug")]
    [SerializeField]
    bool enableDebugLogs = true;

    [SerializeField]
    LayerMask collisionMask;

    [SerializeField]
    new MeshRenderer renderer;

    [SerializeField]
    GameObject explosionGraphic;

    Plane owner;
    Target target;
    Target warningTarget;
    bool exploded;
    Vector3 lastPosition;
    float timer;
    float flareCheckTimer;
    Flare trackedFlare;

    private static int nextLaunchId = 1;
    private int launchId;

    public Rigidbody Rigidbody { get; private set; }
    public Plane Owner => owner;
    public Target Target => target;
    public Target WarningTarget => warningTarget;

    public int LaunchId => launchId;

    public void Launch(Plane owner, Target target)
    {
        Launch(owner, target, target);
    }

    public void Launch(Plane owner, Target target, Target warningTarget)
    {
        launchId = nextLaunchId++;

        exploded = false;
        trackedFlare = null;
        flareCheckTimer = 0f;

        this.owner = owner;
        this.target = target;
        this.warningTarget = warningTarget;

        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.isKinematic = false;

        lastPosition = Rigidbody.position;
        timer = lifetime;

        if (warningTarget != null)
            warningTarget.NotifyMissileLaunched(this, true);
        if (enableDebugLogs)
        {
            string ownerName = owner != null ? owner.DisplayName : "None";
            string targetName = target != null ? target.Name : "None";
            Debug.Log(
                $" Missile launch: owner={ownerName}, target={targetName}, missileId={LaunchId}"
            );
        }
    }

    void Explode()
    {
        if (exploded)
            return;

        timer = lifetime;
        Rigidbody.isKinematic = true;
        renderer.enabled = false;
        exploded = true;
        explosionGraphic.SetActive(true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayExplosionAtPosition(Rigidbody.position);
        }

        var hits = Physics.OverlapSphere(Rigidbody.position, damageRadius, collisionMask.value);
        bool hitAnyPlane = false;
        var hitPlanes = new HashSet<Plane>();

        foreach (var hit in hits)
        {
            Plane other = hit.gameObject.GetComponentInParent<Plane>();

            if (other != null && other != owner && hitPlanes.Add(other))
            {
                hitAnyPlane = true;
                float healthBefore = other.Health;

                float finalDamage = damage;
                if (owner != null)
                {
                    var enemyDifficultyComponent = owner.GetComponent<EnemyDifficultyDamage>();
                    if (enemyDifficultyComponent != null)
                    {
                        finalDamage *= enemyDifficultyComponent.damageMultiplier;
                    }
                }

                other.ApplyDamage(finalDamage, owner, "Missile", "Explosion");
                float healthAfter = other.Health;

                if (enableDebugLogs)
                {
                    string ownerName = owner != null ? owner.DisplayName : "None";
                    string victimName = other.DisplayName;
                    Debug.Log(
                        $" Missile hit: owner={ownerName}, victim={victimName}, damage={finalDamage:F1}, hp={healthBefore:F1}->{healthAfter:F1}, missileId={LaunchId}"
                    );
                }

                if (KillTracker.Instance != null && owner != null && owner.team == Team.Player)
                {
                    var victimTarget = other.GetComponent<Target>();
                    KillTracker.Instance.AwardXPForMissileHit(
                        victimTarget != null ? victimTarget.Name : other.DisplayName
                    );
                }
            }
        }

        if (warningTarget != null)
            warningTarget.NotifyMissileLaunched(this, false);

        if (KillTracker.Instance != null && hitAnyPlane)
        {
            KillTracker.Instance.RecordMissileHit();
        }
        else if (enableDebugLogs && !hitAnyPlane)
        {
            string ownerName = owner != null ? owner.DisplayName : "None";
            Debug.Log($" Missile exploded (no plane hit): owner={ownerName}, missileId={LaunchId}");
        }
    }

    void CheckCollision()
    {
        var currentPosition = Rigidbody.position;
        var error = currentPosition - lastPosition;
        if (error.sqrMagnitude < 0.000001f)
            return;

        float distance = error.magnitude;
        var dir = error / distance;
        var ray = new Ray(lastPosition, dir);
        RaycastHit hit;

        if (
            Physics.SphereCast(
                ray,
                collisionSphereCastRadius,
                out hit,
                distance,
                collisionMask.value,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            Plane other = hit.collider.GetComponentInParent<Plane>();

            if (other == null || other != owner)
            {
                Rigidbody.position = hit.point;
                Explode();
            }
        }

        lastPosition = currentPosition;
    }

    void TrackTarget(float dt)
    {
        if (target == null)
            return;

        var missileVelocity = Rigidbody.linearVelocity;
        if (missileVelocity.sqrMagnitude < 0.001f)
        {
            missileVelocity = (Rigidbody.rotation * Vector3.forward) * speed;
        }
        var targetPosition = Utilities.FirstOrderIntercept(
            Rigidbody.position,
            missileVelocity,
            speed,
            target.Position,
            target.Velocity
        );

        var error = targetPosition - Rigidbody.position;
        if (error.sqrMagnitude < 0.0001f)
            return;
        var targetDir = error.normalized;
        var currentDir = Rigidbody.rotation * Vector3.forward;

        if (Vector3.Angle(currentDir, targetDir) > trackingAngle)
        {
            Explode();
            return;
        }

        float maxTurnRate = (turningGForce * 9.81f) / speed;
        var dir = Vector3.RotateTowards(currentDir, targetDir, maxTurnRate * dt, 0);

        if (dir.sqrMagnitude > 0.0001f)
        {
            Rigidbody.rotation = Quaternion.LookRotation(dir);
        }
    }

    void CheckForFlares(float dt)
    {
        if (trackedFlare != null)
            return;

        flareCheckTimer -= dt;
        if (flareCheckTimer > 0)
            return;

        flareCheckTimer = flareCheckInterval;

        var nearestFlare = Flare.GetNearestFlare(Rigidbody.position, flareDetectionRange);
        if (nearestFlare == null)
            return;

        float distanceToFlare = Vector3.Distance(Rigidbody.position, nearestFlare.Position);

        float normalizedDistance = distanceToFlare / flareDetectionRange;
        float distanceFactor = 1f - Mathf.Sqrt(normalizedDistance);

        distanceFactor = Mathf.Max(distanceFactor, 0.3f);
        float finalChance = flareDistractionChance * distanceFactor * nearestFlare.HeatSignature;

        if (Random.value < finalChance)
        {
            trackedFlare = nearestFlare;
            if (enableDebugLogs)
            {
                Debug.Log(
                    $"[Missile] Distracted by flare! Chance was {finalChance:P0}, missileId={LaunchId}"
                );
            }
        }
    }

    void TrackFlare(float dt)
    {
        if (trackedFlare == null)
            return;

        var flarePosition = trackedFlare.Position;
        var error = flarePosition - Rigidbody.position;
        if (error.sqrMagnitude < 0.0001f)
            return;

        var targetDir = error.normalized;
        var currentDir = Rigidbody.rotation * Vector3.forward;

        if (Vector3.Angle(currentDir, targetDir) > trackingAngle)
        {
            Explode();
            return;
        }

        float maxTurnRate = (turningGForce * 9.81f) / speed;
        var dir = Vector3.RotateTowards(currentDir, targetDir, maxTurnRate * dt, 0);

        if (dir.sqrMagnitude > 0.0001f)
        {
            Rigidbody.rotation = Quaternion.LookRotation(dir);
        }

        float fuseRadius = damageRadius * proximityFuseRadiusMultiplier;
        if (error.sqrMagnitude <= fuseRadius * fuseRadius)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[Missile] Exploded on flare! missileId={LaunchId}");
            }
            Explode();
        }
    }

    void FixedUpdate()
    {
        timer = Mathf.Max(0, timer - Time.fixedDeltaTime);

        if (timer == 0)
        {
            if (exploded)
            {
                Destroy(gameObject);
            }
            else
            {
                Explode();
            }
        }

        if (exploded)
            return;
        if (Rigidbody.isKinematic)
            return;

        CheckCollision();
        if (exploded)
            return;

        CheckForFlares(Time.fixedDeltaTime);

        if (trackedFlare != null)
        {
            TrackFlare(Time.fixedDeltaTime);
        }
        else
        {
            TrackTarget(Time.fixedDeltaTime);
        }
        if (exploded)
            return;

        if (!exploded && target != null)
        {
            float fuseRadius = damageRadius * proximityFuseRadiusMultiplier;
            var toTarget = target.Position - Rigidbody.position;
            if (toTarget.sqrMagnitude <= fuseRadius * fuseRadius)
            {
                Explode();
            }
        }
        if (exploded)
            return;

        Rigidbody.linearVelocity = Rigidbody.rotation * new Vector3(0, 0, speed);
    }
}
