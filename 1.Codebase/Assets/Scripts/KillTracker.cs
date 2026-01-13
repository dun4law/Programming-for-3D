using UnityEngine;

public class KillTracker : MonoBehaviour
{
    public static KillTracker Instance { get; private set; }

    [Header("Player Stats")]
    [SerializeField]
    private int kills = 0;

    [SerializeField]
    private int deaths = 0;

    [SerializeField]
    private int assists = 0;

    [SerializeField]
    private int missilesFired = 0;

    [SerializeField]
    private int missilesHit = 0;

    [SerializeField]
    private int bulletsFired = 0;

    [SerializeField]
    private int bulletsHit = 0;

    [SerializeField]
    private int xp = 0;

    [Header("XP Settings")]
    [SerializeField]
    private int xpPerMissileHit = 10;

    [Header("Settings")]
    [SerializeField]
    private float assistTimeWindow = 10f;

    public System.Action<int> OnKillCountChanged;
    public System.Action<int> OnDeathCountChanged;
    public System.Action<string, string, bool> OnKillAnnounced;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordKill(string killerName, string victimName, bool isPlayer)
    {
        if (isPlayer)
        {
            kills++;
            OnKillCountChanged?.Invoke(kills);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayEnemyDown();
            }

            CheckKillStreak();

            if (GameManager.Instance != null)
            {
                BadgeTracker.CheckForNewBadgesRealtime(
                    $"KillTracker.RecordKill(kill #{kills})",
                    kills,
                    missilesFired
                );
            }
        }

        OnKillAnnounced?.Invoke(killerName, victimName, isPlayer);

        Debug.Log($" Kill! {killerName}  {victimName} (Total kills: {kills})");
    }

    public void RecordDeath(string killerName, bool isPlayer)
    {
        RecordDeath(killerName, isPlayer, null);
    }

    public void RecordDeath(string killerName, bool isPlayer, string reason)
    {
        if (isPlayer)
        {
            deaths++;
            OnDeathCountChanged?.Invoke(deaths);
        }

        Debug.Log(
            $" Killed! Shot down by {killerName} (Total deaths: {deaths}){(string.IsNullOrWhiteSpace(reason) ? "" : $" reason={reason}")}"
        );
    }

    public void RecordMissileFired()
    {
        missilesFired++;
        Debug.Log(
            $" Missile fired (missilesFired={missilesFired}, missilesHit={missilesHit}, accuracy={GetAccuracy():F1}%)"
        );

        if (missilesFired % 50 == 0 && GameManager.Instance != null)
        {
            BadgeTracker.CheckForNewBadgesRealtime(
                $"KillTracker.RecordMissileFired(#{missilesFired})",
                kills,
                missilesFired
            );
        }
    }

    public void RecordMissileHit()
    {
        missilesHit++;
        Debug.Log(
            $" Missile hit (missilesFired={missilesFired}, missilesHit={missilesHit}, accuracy={GetAccuracy():F1}%)"
        );
    }

    public void AddXP(int amount, string reason = null)
    {
        if (amount == 0)
            return;
        int before = xp;
        xp = Mathf.Max(0, xp + amount);
        Debug.Log(
            $" XP {(amount > 0 ? "+" : "")}{amount} (xp={before}→{xp}){(string.IsNullOrWhiteSpace(reason) ? "" : $" reason={reason}")}"
        );
    }

    public void AwardXPForMissileHit(string victimName = null)
    {
        string reason = string.IsNullOrWhiteSpace(victimName)
            ? "MissileHit"
            : $"MissileHit victim={victimName}";
        AddXP(xpPerMissileHit, reason);
    }

    void CheckKillStreak()
    {
        switch (kills)
        {
            case 2:
                Debug.Log(" Double Kill!");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDoubleKill();
                }
                break;
            case 3:
                Debug.Log(" Triple Kill!");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayTripleKill();
                }
                break;
            case 5:
                Debug.Log(" Ace!");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayEnemyEliminated();
                    AudioManager.Instance.PlayGoodWork();
                }
                break;
            case 10:
                Debug.Log(" Legendary!");
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayMissionAccomplished();
                }
                break;
        }
    }

    public void ResetStats()
    {
        kills = 0;
        deaths = 0;
        assists = 0;
        missilesFired = 0;
        missilesHit = 0;
        bulletsFired = 0;
        bulletsHit = 0;
        xp = 0;
    }

    public float GetKDRatio()
    {
        if (deaths == 0)
            return kills;
        return (float)kills / deaths;
    }

    public float GetAccuracy()
    {
        if (missilesFired == 0)
            return 0f;
        return (float)missilesHit / missilesFired * 100f;
    }

    public void RecordBulletFired()
    {
        bulletsFired++;
    }

    public void RecordBulletHit()
    {
        bulletsHit++;
    }

    public int Kills => kills;
    public int Deaths => deaths;
    public int Assists => assists;
    public int MissilesFired => missilesFired;
    public int MissilesHit => missilesHit;
    public int BulletsFired => bulletsFired;
    public int BulletsHit => bulletsHit;
    public int XP => xp;
    public float AssistTimeWindow => assistTimeWindow;
}
