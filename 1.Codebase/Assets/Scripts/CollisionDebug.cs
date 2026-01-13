using UnityEngine;

public class CollisionDebug : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField]
    private bool enableDebug = true;

    [SerializeField]
    private Color rayColor = Color.red;

    [SerializeField]
    private float rayLength = 50f;

    private Rigidbody rb;
    private Collider[] myColliders;

    void Start()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "Menu and story" || currentScene == "Loading")
        {
            enabled = false;
            return;
        }

        rb = GetComponent<Rigidbody>();
        myColliders = GetComponents<Collider>();

        Debug.Log($"=== Collision Debug Start ===");
        Debug.Log($"Object: {gameObject.name}");
        Debug.Log($"Has Rigidbody: {rb != null}");
        Debug.Log($"Rigidbody isKinematic: {(rb != null ? rb.isKinematic.ToString() : "N/A")}");
        Debug.Log($"Collider Count: {myColliders.Length}");

        foreach (var col in myColliders)
        {
            Debug.Log(
                $"  - Collider: {col.GetType().Name}, isTrigger: {col.isTrigger}, Layer: {LayerMask.LayerToName(col.gameObject.layer)}"
            );
        }

        CheckSceneColliders();
    }

    void CheckSceneColliders()
    {
        GameObject hongKong = GameObject.Find("Hong_Kong");
        if (hongKong == null)
        {
            hongKong = GameObject.Find("Hong Kong");
        }

        if (hongKong != null)
        {
            Debug.Log($"=== Map Object: {hongKong.name} ===");

            Collider[] mapColliders = hongKong.GetComponentsInChildren<Collider>();
            Debug.Log($"Map Colliders count: {mapColliders.Length}");

            if (mapColliders.Length == 0)
            {
                Debug.LogWarning(" Map has no Colliders! Planes cannot collide.");
                Debug.LogWarning("Solution: Add Component  Mesh Collider on Hong_Kong object");
            }
            else
            {
                foreach (var col in mapColliders)
                {
                    Debug.Log(
                        $"  - {col.gameObject.name}: {col.GetType().Name}, isTrigger: {col.isTrigger}"
                    );
                }
            }
        }
        else
        {
            Debug.LogWarning("Cannot find Hong_Kong or Hong Kong object");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!enableDebug)
            return;

        Debug.Log($" Collision! {gameObject.name} hit {collision.gameObject.name}");
        Debug.Log($"   Collision point: {collision.contacts[0].point}");
        Debug.Log($"   Collision Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
    }

    void OnTriggerEnter(Collider other)
    {
        if (!enableDebug)
            return;

        Debug.Log($" Trigger Enter! {gameObject.name} entered {other.gameObject.name}");
        Debug.Log(
            $"    This is a Trigger not a collision! If the map is a Trigger, disable 'Is Trigger'"
        );
    }

    void Update()
    {
        if (!enableDebug)
            return;

        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = -transform.up;

        Debug.DrawRay(origin, direction * rayLength, rayColor);

        if (Physics.Raycast(origin, direction, out hit, rayLength))
        {
            Debug.DrawLine(origin, hit.point, Color.green);

            if (Time.frameCount % 60 == 0)
            {
                Debug.Log(
                    $" Detected below: {hit.collider.gameObject.name} distance: {hit.distance:F1}m"
                );
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
