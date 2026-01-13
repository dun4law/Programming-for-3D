using System.Collections.Generic;
using UnityEngine;

public static class BadgeTracker
{
    private const string PREFS_KEY = "EarnedBadges";
    private static HashSet<string> earnedBadges = new HashSet<string>();
    private static List<BadgeInfo> pendingNewBadges = new List<BadgeInfo>();
    private static bool initialized = false;
    private static string lastCheckSource = "";

    public class BadgeInfo
    {
        public string id;
        public string name;
        public string description;
    }

    private static readonly Dictionary<string, BadgeInfo> allBadges = new Dictionary<
        string,
        BadgeInfo
    >
    {
        {
            "badge-first-kill",
            new BadgeInfo
            {
                id = "badge-first-kill",
                name = "First Blood",
                description = "Score your first kill",
            }
        },
        {
            "badge-5-kills",
            new BadgeInfo
            {
                id = "badge-5-kills",
                name = "Ace Pilot",
                description = "5 total kills",
            }
        },
        {
            "badge-10-kills",
            new BadgeInfo
            {
                id = "badge-10-kills",
                name = "Double Ace",
                description = "10 total kills",
            }
        },
        {
            "badge-25-kills",
            new BadgeInfo
            {
                id = "badge-25-kills",
                name = "Hunter",
                description = "25 total kills",
            }
        },
        {
            "badge-50-kills",
            new BadgeInfo
            {
                id = "badge-50-kills",
                name = "Veteran",
                description = "50 total kills",
            }
        },
        {
            "badge-100-kills",
            new BadgeInfo
            {
                id = "badge-100-kills",
                name = "Top Gun",
                description = "100 total kills",
            }
        },
        {
            "badge-250-kills",
            new BadgeInfo
            {
                id = "badge-250-kills",
                name = "Sky Terror",
                description = "250 total kills",
            }
        },
        {
            "badge-500-kills",
            new BadgeInfo
            {
                id = "badge-500-kills",
                name = "Air Supremacy",
                description = "500 total kills",
            }
        },
        {
            "badge-first-win",
            new BadgeInfo
            {
                id = "badge-first-win",
                name = "Victorious",
                description = "Win your first mission",
            }
        },
        {
            "badge-5-wins",
            new BadgeInfo
            {
                id = "badge-5-wins",
                name = "Proven Pilot",
                description = "5 victories",
            }
        },
        {
            "badge-10-wins",
            new BadgeInfo
            {
                id = "badge-10-wins",
                name = "Centurion",
                description = "10 victories",
            }
        },
        {
            "badge-25-wins",
            new BadgeInfo
            {
                id = "badge-25-wins",
                name = "War Hero",
                description = "25 victories",
            }
        },
        {
            "badge-50-wins",
            new BadgeInfo
            {
                id = "badge-50-wins",
                name = "Legend",
                description = "50 victories",
            }
        },
        {
            "badge-1hr-flight",
            new BadgeInfo
            {
                id = "badge-1hr-flight",
                name = "Airborne",
                description = "1 hour total flight",
            }
        },
        {
            "badge-5hr-flight",
            new BadgeInfo
            {
                id = "badge-5hr-flight",
                name = "Sky Dweller",
                description = "5 hours total flight",
            }
        },
        {
            "badge-10hr-flight",
            new BadgeInfo
            {
                id = "badge-10hr-flight",
                name = "Iron Wings",
                description = "10 hours total flight",
            }
        },
        {
            "badge-24hr-flight",
            new BadgeInfo
            {
                id = "badge-24hr-flight",
                name = "Marathon Pilot",
                description = "24 hours total flight",
            }
        },
        {
            "badge-10-sorties",
            new BadgeInfo
            {
                id = "badge-10-sorties",
                name = "Frequent Flyer",
                description = "10 sorties flown",
            }
        },
        {
            "badge-25-sorties",
            new BadgeInfo
            {
                id = "badge-25-sorties",
                name = "Dedicated",
                description = "25 sorties flown",
            }
        },
        {
            "badge-50-sorties",
            new BadgeInfo
            {
                id = "badge-50-sorties",
                name = "Combat Ready",
                description = "50 sorties flown",
            }
        },
        {
            "badge-100-sorties",
            new BadgeInfo
            {
                id = "badge-100-sorties",
                name = "Tireless Warrior",
                description = "100 sorties flown",
            }
        },
        {
            "badge-flew-f15",
            new BadgeInfo
            {
                id = "badge-flew-f15",
                name = "Eagle Driver",
                description = "Fly the F-15C Eagle",
            }
        },
        {
            "badge-flew-su27",
            new BadgeInfo
            {
                id = "badge-flew-su27",
                name = "Flanker Ace",
                description = "Fly the Su-27 Flanker",
            }
        },
        {
            "badge-flew-mig29",
            new BadgeInfo
            {
                id = "badge-flew-mig29",
                name = "Fulcrum Master",
                description = "Fly the MiG-29 Fulcrum",
            }
        },
        {
            "badge-flew-fa18e",
            new BadgeInfo
            {
                id = "badge-flew-fa18e",
                name = "Hornet Pilot",
                description = "Fly the F/A-18E",
            }
        },
        {
            "badge-flew-hawk200",
            new BadgeInfo
            {
                id = "badge-flew-hawk200",
                name = "Hawk Handler",
                description = "Fly the Hawk 200",
            }
        },
        {
            "badge-flew-mig21",
            new BadgeInfo
            {
                id = "badge-flew-mig21",
                name = "Fishbed Flyer",
                description = "Fly the MiG-21",
            }
        },
        {
            "badge-flew-tornado",
            new BadgeInfo
            {
                id = "badge-flew-tornado",
                name = "Storm Chaser",
                description = "Fly the Tornado",
            }
        },
        {
            "badge-flew-rafale",
            new BadgeInfo
            {
                id = "badge-flew-rafale",
                name = "Rafale Rider",
                description = "Fly the Rafale",
            }
        },
        {
            "badge-flew-typhoon",
            new BadgeInfo
            {
                id = "badge-flew-typhoon",
                name = "Typhoon Tamer",
                description = "Fly the Typhoon",
            }
        },
        {
            "badge-f15-10kills",
            new BadgeInfo
            {
                id = "badge-f15-10kills",
                name = "Eagle Sharpshooter",
                description = "10 kills in F-15C",
            }
        },
        {
            "badge-su27-10kills",
            new BadgeInfo
            {
                id = "badge-su27-10kills",
                name = "Flanker Elite",
                description = "10 kills in Su-27",
            }
        },
        {
            "badge-mig29-10kills",
            new BadgeInfo
            {
                id = "badge-mig29-10kills",
                name = "Fulcrum Fury",
                description = "10 kills in MiG-29",
            }
        },
        {
            "badge-fa18e-10kills",
            new BadgeInfo
            {
                id = "badge-fa18e-10kills",
                name = "Hornet's Sting",
                description = "10 kills in F/A-18E",
            }
        },
        {
            "badge-hawk200-10kills",
            new BadgeInfo
            {
                id = "badge-hawk200-10kills",
                name = "Hawk Strike",
                description = "10 kills in Hawk 200",
            }
        },
        {
            "badge-mig21-10kills",
            new BadgeInfo
            {
                id = "badge-mig21-10kills",
                name = "Fishbed Fury",
                description = "10 kills in MiG-21",
            }
        },
        {
            "badge-tornado-10kills",
            new BadgeInfo
            {
                id = "badge-tornado-10kills",
                name = "Storm Bringer",
                description = "10 kills in Tornado",
            }
        },
        {
            "badge-rafale-10kills",
            new BadgeInfo
            {
                id = "badge-rafale-10kills",
                name = "Rafale Rampage",
                description = "10 kills in Rafale",
            }
        },
        {
            "badge-typhoon-10kills",
            new BadgeInfo
            {
                id = "badge-typhoon-10kills",
                name = "Typhoon Terror",
                description = "10 kills in Typhoon",
            }
        },
        {
            "badge-all-aircraft",
            new BadgeInfo
            {
                id = "badge-all-aircraft",
                name = "Fleet Commander",
                description = "Fly all 9 aircraft",
            }
        },
        {
            "badge-rank-5",
            new BadgeInfo
            {
                id = "badge-rank-5",
                name = "Rising Star",
                description = "Reach Rank 5",
            }
        },
        {
            "badge-rank-10",
            new BadgeInfo
            {
                id = "badge-rank-10",
                name = "Elite Pilot",
                description = "Reach Rank 10",
            }
        },
        {
            "badge-missiles-100",
            new BadgeInfo
            {
                id = "badge-missiles-100",
                name = "Missile Master",
                description = "Fire 100 missiles",
            }
        },
        {
            "badge-missiles-500",
            new BadgeInfo
            {
                id = "badge-missiles-500",
                name = "Arsenal",
                description = "Fire 500 missiles",
            }
        },
    };

    public static void Initialize()
    {
        if (initialized)
            return;

        LoadEarnedBadges();
        initialized = true;
        Debug.Log("[BadgeTracker] ====== BADGE SYSTEM INITIALIZED ======");
    }

    private static void LoadEarnedBadges()
    {
        earnedBadges.Clear();
        string savedBadges = PlayerPrefs.GetString(PREFS_KEY, "");

        if (!string.IsNullOrEmpty(savedBadges))
        {
            string[] badges = savedBadges.Split(',');
            foreach (string badge in badges)
            {
                if (!string.IsNullOrWhiteSpace(badge))
                {
                    earnedBadges.Add(badge.Trim());
                }
            }
        }

        Debug.Log(
            $"[BadgeTracker] Loaded {earnedBadges.Count} previously earned badges from save data"
        );
    }

    private static void SaveEarnedBadges()
    {
        string badgesString = string.Join(",", earnedBadges);
        PlayerPrefs.SetString(PREFS_KEY, badgesString);
        PlayerPrefs.Save();
        Debug.Log($"[BadgeTracker] Saved {earnedBadges.Count} badges to PlayerPrefs");
    }

    public static void CheckForNewBadges(string source = "Unknown")
    {
        if (!initialized)
            Initialize();

        lastCheckSource = source;
        Debug.Log($"[BadgeTracker] ====== CHECKING FOR NEW BADGES ======");
        Debug.Log($"[BadgeTracker] Check triggered from: {source}");

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[BadgeTracker] GameManager.Instance is null! Cannot check badges.");
            return;
        }

        var gm = GameManager.Instance;

        Debug.Log($"[BadgeTracker] Current Stats:");
        Debug.Log($"[BadgeTracker]   - TotalKills: {gm.TotalKills}");
        Debug.Log($"[BadgeTracker]   - TotalWins: {gm.TotalWins}");
        Debug.Log(
            $"[BadgeTracker]   - TotalFlightTime: {gm.TotalFlightTime:F1}s ({gm.TotalFlightTime / 3600f:F2}h)"
        );
        Debug.Log($"[BadgeTracker]   - TotalSorties: {gm.TotalSorties}");
        Debug.Log($"[BadgeTracker]   - TotalMissilesFired: {gm.TotalMissilesFired}");
        Debug.Log($"[BadgeTracker]   - PlayerRank: {gm.PlayerRank}");

        int badgesBeforeCheck = pendingNewBadges.Count;

        CheckBadge("badge-first-kill", gm.TotalKills >= 1, $"TotalKills({gm.TotalKills}) >= 1");
        CheckBadge("badge-5-kills", gm.TotalKills >= 5, $"TotalKills({gm.TotalKills}) >= 5");
        CheckBadge("badge-10-kills", gm.TotalKills >= 10, $"TotalKills({gm.TotalKills}) >= 10");
        CheckBadge("badge-25-kills", gm.TotalKills >= 25, $"TotalKills({gm.TotalKills}) >= 25");
        CheckBadge("badge-50-kills", gm.TotalKills >= 50, $"TotalKills({gm.TotalKills}) >= 50");
        CheckBadge("badge-100-kills", gm.TotalKills >= 100, $"TotalKills({gm.TotalKills}) >= 100");
        CheckBadge("badge-250-kills", gm.TotalKills >= 250, $"TotalKills({gm.TotalKills}) >= 250");
        CheckBadge("badge-500-kills", gm.TotalKills >= 500, $"TotalKills({gm.TotalKills}) >= 500");

        CheckBadge("badge-first-win", gm.TotalWins >= 1, $"TotalWins({gm.TotalWins}) >= 1");
        CheckBadge("badge-5-wins", gm.TotalWins >= 5, $"TotalWins({gm.TotalWins}) >= 5");
        CheckBadge("badge-10-wins", gm.TotalWins >= 10, $"TotalWins({gm.TotalWins}) >= 10");
        CheckBadge("badge-25-wins", gm.TotalWins >= 25, $"TotalWins({gm.TotalWins}) >= 25");
        CheckBadge("badge-50-wins", gm.TotalWins >= 50, $"TotalWins({gm.TotalWins}) >= 50");

        CheckBadge(
            "badge-1hr-flight",
            gm.TotalFlightTime >= 3600f,
            $"FlightTime({gm.TotalFlightTime:F0}s) >= 3600s"
        );
        CheckBadge(
            "badge-5hr-flight",
            gm.TotalFlightTime >= 18000f,
            $"FlightTime({gm.TotalFlightTime:F0}s) >= 18000s"
        );
        CheckBadge(
            "badge-10hr-flight",
            gm.TotalFlightTime >= 36000f,
            $"FlightTime({gm.TotalFlightTime:F0}s) >= 36000s"
        );
        CheckBadge(
            "badge-24hr-flight",
            gm.TotalFlightTime >= 86400f,
            $"FlightTime({gm.TotalFlightTime:F0}s) >= 86400s"
        );

        CheckBadge(
            "badge-10-sorties",
            gm.TotalSorties >= 10,
            $"TotalSorties({gm.TotalSorties}) >= 10"
        );
        CheckBadge(
            "badge-25-sorties",
            gm.TotalSorties >= 25,
            $"TotalSorties({gm.TotalSorties}) >= 25"
        );
        CheckBadge(
            "badge-50-sorties",
            gm.TotalSorties >= 50,
            $"TotalSorties({gm.TotalSorties}) >= 50"
        );
        CheckBadge(
            "badge-100-sorties",
            gm.TotalSorties >= 100,
            $"TotalSorties({gm.TotalSorties}) >= 100"
        );

        CheckBadge("badge-flew-f15", gm.F15Sorties >= 1, $"F15Sorties({gm.F15Sorties}) >= 1");
        CheckBadge("badge-flew-su27", gm.SU27Sorties >= 1, $"SU27Sorties({gm.SU27Sorties}) >= 1");
        CheckBadge(
            "badge-flew-mig29",
            gm.MIG29Sorties >= 1,
            $"MIG29Sorties({gm.MIG29Sorties}) >= 1"
        );
        CheckBadge(
            "badge-flew-fa18e",
            gm.FA18ESorties >= 1,
            $"FA18ESorties({gm.FA18ESorties}) >= 1"
        );
        CheckBadge(
            "badge-flew-hawk200",
            gm.Hawk200Sorties >= 1,
            $"Hawk200Sorties({gm.Hawk200Sorties}) >= 1"
        );
        CheckBadge(
            "badge-flew-mig21",
            gm.MIG21Sorties >= 1,
            $"MIG21Sorties({gm.MIG21Sorties}) >= 1"
        );
        CheckBadge(
            "badge-flew-tornado",
            gm.TornadoSorties >= 1,
            $"TornadoSorties({gm.TornadoSorties}) >= 1"
        );
        CheckBadge(
            "badge-flew-rafale",
            gm.RafaleSorties >= 1,
            $"RafaleSorties({gm.RafaleSorties}) >= 1"
        );
        CheckBadge(
            "badge-flew-typhoon",
            gm.TyphoonSorties >= 1,
            $"TyphoonSorties({gm.TyphoonSorties}) >= 1"
        );

        CheckBadge("badge-f15-10kills", gm.F15Kills >= 10, $"F15Kills({gm.F15Kills}) >= 10");
        CheckBadge("badge-su27-10kills", gm.SU27Kills >= 10, $"SU27Kills({gm.SU27Kills}) >= 10");
        CheckBadge(
            "badge-mig29-10kills",
            gm.MIG29Kills >= 10,
            $"MIG29Kills({gm.MIG29Kills}) >= 10"
        );
        CheckBadge(
            "badge-fa18e-10kills",
            gm.FA18EKills >= 10,
            $"FA18EKills({gm.FA18EKills}) >= 10"
        );
        CheckBadge(
            "badge-hawk200-10kills",
            gm.Hawk200Kills >= 10,
            $"Hawk200Kills({gm.Hawk200Kills}) >= 10"
        );
        CheckBadge(
            "badge-mig21-10kills",
            gm.MIG21Kills >= 10,
            $"MIG21Kills({gm.MIG21Kills}) >= 10"
        );
        CheckBadge(
            "badge-tornado-10kills",
            gm.TornadoKills >= 10,
            $"TornadoKills({gm.TornadoKills}) >= 10"
        );
        CheckBadge(
            "badge-rafale-10kills",
            gm.RafaleKills >= 10,
            $"RafaleKills({gm.RafaleKills}) >= 10"
        );
        CheckBadge(
            "badge-typhoon-10kills",
            gm.TyphoonKills >= 10,
            $"TyphoonKills({gm.TyphoonKills}) >= 10"
        );

        bool allAircraftFlown =
            gm.F15Sorties >= 1
            && gm.SU27Sorties >= 1
            && gm.MIG29Sorties >= 1
            && gm.FA18ESorties >= 1
            && gm.Hawk200Sorties >= 1
            && gm.MIG21Sorties >= 1
            && gm.TornadoSorties >= 1
            && gm.RafaleSorties >= 1
            && gm.TyphoonSorties >= 1;
        CheckBadge("badge-all-aircraft", allAircraftFlown, $"AllAircraftFlown={allAircraftFlown}");
        CheckBadge("badge-rank-5", gm.PlayerRank >= 5, $"PlayerRank({gm.PlayerRank}) >= 5");
        CheckBadge("badge-rank-10", gm.PlayerRank >= 10, $"PlayerRank({gm.PlayerRank}) >= 10");
        CheckBadge(
            "badge-missiles-100",
            gm.TotalMissilesFired >= 100,
            $"MissilesFired({gm.TotalMissilesFired}) >= 100"
        );
        CheckBadge(
            "badge-missiles-500",
            gm.TotalMissilesFired >= 500,
            $"MissilesFired({gm.TotalMissilesFired}) >= 500"
        );

        int newBadgesThisCheck = pendingNewBadges.Count - badgesBeforeCheck;

        if (newBadgesThisCheck > 0)
        {
            SaveEarnedBadges();
            Debug.Log($"[BadgeTracker] ====== {newBadgesThisCheck} NEW BADGE(S) EARNED! ======");
            Debug.Log($"[BadgeTracker] Total pending new badges: {pendingNewBadges.Count}");
        }
        else
        {
            Debug.Log(
                $"[BadgeTracker] No new badges earned this check. Total earned: {earnedBadges.Count}/44"
            );
        }

        Debug.Log($"[BadgeTracker] ====== BADGE CHECK COMPLETE ======");
    }

    private static void CheckBadge(string badgeId, bool condition, string conditionDesc)
    {
        bool alreadyEarned = earnedBadges.Contains(badgeId);

        if (condition && !alreadyEarned)
        {
            earnedBadges.Add(badgeId);
            if (allBadges.TryGetValue(badgeId, out var badgeInfo))
            {
                pendingNewBadges.Add(badgeInfo);
                Debug.Log($"[BadgeTracker] ***** NEW BADGE UNLOCKED! *****");
                Debug.Log($"[BadgeTracker]   Badge: {badgeInfo.name}");
                Debug.Log($"[BadgeTracker]   ID: {badgeId}");
                Debug.Log($"[BadgeTracker]   Description: {badgeInfo.description}");
                Debug.Log($"[BadgeTracker]   Condition Met: {conditionDesc}");
                Debug.Log($"[BadgeTracker]   Triggered From: {lastCheckSource}");
                Debug.Log($"[BadgeTracker] *****************************");
            }
        }
    }

    public static void CheckForNewBadgesRealtime(
        string source,
        int sessionKills,
        int sessionMissilesFired
    )
    {
        if (!initialized)
            Initialize();

        if (GameManager.Instance == null)
            return;

        var gm = GameManager.Instance;
        lastCheckSource = source;

        int projectedKills = gm.TotalKills + sessionKills;
        int projectedMissiles = gm.TotalMissilesFired + sessionMissilesFired;

        Debug.Log(
            $"[BadgeTracker] Real-time check from {source}: Session Kills={sessionKills}, Projected Total={projectedKills}"
        );

        CheckBadge(
            "badge-first-kill",
            projectedKills >= 1,
            $"ProjectedKills({projectedKills}) >= 1"
        );
        CheckBadge("badge-5-kills", projectedKills >= 5, $"ProjectedKills({projectedKills}) >= 5");
        CheckBadge(
            "badge-10-kills",
            projectedKills >= 10,
            $"ProjectedKills({projectedKills}) >= 10"
        );
        CheckBadge(
            "badge-25-kills",
            projectedKills >= 25,
            $"ProjectedKills({projectedKills}) >= 25"
        );
        CheckBadge(
            "badge-50-kills",
            projectedKills >= 50,
            $"ProjectedKills({projectedKills}) >= 50"
        );
        CheckBadge(
            "badge-100-kills",
            projectedKills >= 100,
            $"ProjectedKills({projectedKills}) >= 100"
        );

        CheckBadge(
            "badge-missiles-100",
            projectedMissiles >= 100,
            $"ProjectedMissiles({projectedMissiles}) >= 100"
        );
        CheckBadge(
            "badge-missiles-500",
            projectedMissiles >= 500,
            $"ProjectedMissiles({projectedMissiles}) >= 500"
        );
    }

    public static List<BadgeInfo> GetPendingNewBadges()
    {
        return new List<BadgeInfo>(pendingNewBadges);
    }

    public static void ClearPendingBadges()
    {
        Debug.Log(
            $"[BadgeTracker] Clearing {pendingNewBadges.Count} pending badges (already shown to player)"
        );
        pendingNewBadges.Clear();
    }

    public static bool HasPendingBadges()
    {
        return pendingNewBadges.Count > 0;
    }

    public static int GetTotalEarnedBadges()
    {
        return earnedBadges.Count;
    }

    public static int GetTotalBadges()
    {
        return allBadges.Count;
    }

    public static void ResetAllBadges()
    {
        earnedBadges.Clear();
        pendingNewBadges.Clear();
        PlayerPrefs.DeleteKey(PREFS_KEY);
        PlayerPrefs.Save();
        Debug.Log("[BadgeTracker] ====== ALL BADGES RESET ======");
    }

    public static void DebugLogAllBadgeStatus()
    {
        Debug.Log("[BadgeTracker] ====== FULL BADGE STATUS REPORT ======");
        Debug.Log($"[BadgeTracker] Total Badges: {allBadges.Count}");
        Debug.Log($"[BadgeTracker] Earned Badges: {earnedBadges.Count}");
        Debug.Log($"[BadgeTracker] Pending New Badges: {pendingNewBadges.Count}");
        Debug.Log("[BadgeTracker] --- Earned Badges List ---");
        foreach (var badgeId in earnedBadges)
        {
            if (allBadges.TryGetValue(badgeId, out var info))
            {
                Debug.Log($"[BadgeTracker]   [EARNED] {info.name} ({badgeId})");
            }
        }
        Debug.Log("[BadgeTracker] --- Pending New Badges ---");
        foreach (var badge in pendingNewBadges)
        {
            Debug.Log($"[BadgeTracker]   [NEW] {badge.name} ({badge.id})");
        }
        Debug.Log("[BadgeTracker] ==========================================");
    }
}
