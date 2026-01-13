using UnityEngine;

[RequireComponent(typeof(Plane))]
public class TerrainCollisionDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField]
    private float rayDistance = 20f;

    [SerializeField]
    private int rayCount = 5;

    [SerializeField]
    private LayerMask terrainLayers = -1;

    [SerializeField]
    private bool checkVelocityDirection = true;

    [SerializeField]
    private bool ignorePlanes = true;

    [Header("Debug")]
    [SerializeField]
    private bool showDebugRays = true;

    [SerializeField]
    private Color rayColor = Color.red;

    private Plane plane;
    private Rigidbody rb;
    private bool hasCrashed = false;

    void Start()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Menu and story" || currentScene == "Loading")
        {
            enabled = false;
            return;
        }

        plane = GetComponent<Plane>();
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Debug.Log("TerrainCollisionDetector: Set Continuous Dynamic collision detection");
        }
    }

    void FixedUpdate()
    {
        if (hasCrashed || plane == null || plane.Dead)
            return;

        CheckCollision();
    }

    void CheckCollision()
    {
        Vector3 origin = transform.position;

        Vector3[] directions = new Vector3[5];

        if (checkVelocityDirection && rb != null && rb.linearVelocity.sqrMagnitude > 1f)
        {
            directions[0] = rb.linearVelocity.normalized;
        }
        else
        {
            directions[0] = transform.forward;
        }

        directions[1] = -transform.up;
        directions[2] = transform.forward;
        directions[3] = transform.forward + (-transform.up * 0.5f);
        directions[4] = transform.forward + (-transform.up * 0.3f) + (transform.right * 0.2f);

        int usedRayCount = Mathf.Clamp(rayCount, 1, directions.Length);
        for (int i = 0; i < usedRayCount; i++)
        {
            var dir = directions[i];
            if (dir == Vector3.zero)
                continue;

            float speed = rb != null ? rb.linearVelocity.magnitude : 0f;
            float adjustedDistance = rayDistance + (speed * Time.fixedDeltaTime * 2f);

            if (showDebugRays)
            {
                Debug.DrawRay(origin, dir.normalized * adjustedDistance, rayColor);
            }

            var hits = Physics.RaycastAll(
                origin,
                dir.normalized,
                adjustedDistance,
                terrainLayers,
                QueryTriggerInteraction.Ignore
            );
            if (hits == null || hits.Length == 0)
                continue;

            bool foundValidHit = false;
            RaycastHit bestHit = default;
            float bestDistance = float.PositiveInfinity;

            foreach (var hit in hits)
            {
                if (hit.collider == null)
                    continue;

                if (rb != null && hit.collider.attachedRigidbody == rb)
                    continue;
                if (hit.collider.transform.IsChildOf(transform))
                    continue;

                if (ignorePlanes && hit.collider.GetComponentInParent<Plane>() != null)
                    continue;

                if (hit.distance < bestDistance)
                {
                    bestDistance = hit.distance;
                    bestHit = hit;
                    foundValidHit = true;
                }
            }

            if (foundValidHit)
            {
                OnTerrainHit(bestHit);
                return;
            }
        }
    }

    void OnTerrainHit(RaycastHit hit)
    {
        if (hasCrashed)
            return;
        hasCrashed = true;

        Debug.Log(
            $" TerrainCollisionDetector: {gameObject.name} hit {hit.collider.gameObject.name}!"
        );
        Debug.Log($"   Collision point: {hit.point}, distance: {hit.distance}");

        if (plane != null)
        {
            plane.ApplyDamage(plane.MaxHealth * 10f, null, "Terrain", "TerrainCollision");
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
    }

    public void ResetCrashState()
    {
        hasCrashed = false;
        if (rb != null)
        {
            rb.isKinematic = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rayDistance);
    }
}
