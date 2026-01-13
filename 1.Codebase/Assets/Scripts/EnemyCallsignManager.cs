using System.Collections.Generic;
using UnityEngine;

public class EnemyCallsignManager : MonoBehaviour
{
    public static EnemyCallsignManager Instance { get; private set; }

    [Header("Callsign Settings")]
    [SerializeField]
    private bool useRandomCallsigns = true;

    [SerializeField]
    private bool allowDuplicateCallsigns = false;

    private static readonly string[] TopGunCallsigns = new string[]
    {
        "ICEMAN",
        "VIPER",
        "GOOSE",
        "JESTER",
        "HOLLYWOOD",
        "WOLFMAN",
        "SLIDER",
        "MERLIN",
        "SUNDOWN",
        "CHIPPER",
        "COUGAR",
        "STINGER",
        "ROOSTER",
        "HANGMAN",
        "PHOENIX",
        "BOB",
        "PAYBACK",
        "FANBOY",
        "COYOTE",
        "HONDO",
        "WARLOCK",
        "CYCLONE",
        "GHOST",
        "REAPER",
        "VENOM",
        "BLAZE",
        "STORM",
        "SHADOW",
        "RAPTOR",
        "HAVOC",
        "HUNTER",
        "COBRA",
    };

    private List<string> availableCallsigns;
    private Dictionary<Plane, string> assignedCallsigns;
    private int sequentialIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeCallsigns();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemySpawned += OnEnemySpawned;
            EnemySpawner.Instance.OnEnemyDestroyed += OnEnemyDestroyed;
        }
    }

    void OnDestroy()
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.OnEnemySpawned -= OnEnemySpawned;
            EnemySpawner.Instance.OnEnemyDestroyed -= OnEnemyDestroyed;
        }
    }

    private void InitializeCallsigns()
    {
        availableCallsigns = new List<string>(TopGunCallsigns);
        assignedCallsigns = new Dictionary<Plane, string>();
        sequentialIndex = 0;
    }

    private void OnEnemySpawned(Plane enemyPlane)
    {
        if (enemyPlane == null)
            return;

        string callsign = GetNextCallsign();
        AssignCallsignToPlane(enemyPlane, callsign);
    }

    private void OnEnemyDestroyed(Plane enemyPlane)
    {
        if (enemyPlane == null)
            return;

        if (assignedCallsigns.TryGetValue(enemyPlane, out string callsign))
        {
            if (!allowDuplicateCallsigns && !availableCallsigns.Contains(callsign))
            {
                availableCallsigns.Add(callsign);
            }
            assignedCallsigns.Remove(enemyPlane);
            Debug.Log($" Recycled callsign: {callsign}");
        }
    }

    private string GetNextCallsign()
    {
        if (useRandomCallsigns)
        {
            return GetRandomCallsign();
        }
        else
        {
            return GetSequentialCallsign();
        }
    }

    private string GetRandomCallsign()
    {
        if (availableCallsigns.Count == 0)
        {
            if (!allowDuplicateCallsigns)
            {
                availableCallsigns = new List<string>(TopGunCallsigns);
                Debug.Log(" All callsigns used, resetting pool");
            }
            else
            {
                return TopGunCallsigns[Random.Range(0, TopGunCallsigns.Length)];
            }
        }

        int index = Random.Range(0, availableCallsigns.Count);
        string callsign = availableCallsigns[index];

        if (!allowDuplicateCallsigns)
        {
            availableCallsigns.RemoveAt(index);
        }

        return callsign;
    }

    private string GetSequentialCallsign()
    {
        string callsign = TopGunCallsigns[sequentialIndex % TopGunCallsigns.Length];
        sequentialIndex++;
        return callsign;
    }

    private void AssignCallsignToPlane(Plane plane, string callsign)
    {
        var target = plane.GetComponent<Target>();
        plane.SetCallsign(callsign);
        assignedCallsigns[plane] = callsign;
        if (target != null)
        {
            target.SetName(callsign);
            Debug.Log($" Assigned callsign '{callsign}' to enemy");
        }
        else
        {
            Debug.LogWarning(
                $" Could not assign callsign - Target component not found on {plane.name}"
            );
        }
    }

    public void AssignCallsign(Plane plane, string callsign)
    {
        AssignCallsignToPlane(plane, callsign);
    }

    public string GetCallsign(Plane plane)
    {
        if (assignedCallsigns.TryGetValue(plane, out string callsign))
        {
            return callsign;
        }
        return null;
    }

    public void ResetCallsigns()
    {
        InitializeCallsigns();
        Debug.Log(" Callsign pool reset");
    }
}
