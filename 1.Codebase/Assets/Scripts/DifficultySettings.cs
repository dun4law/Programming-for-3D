using UnityEngine;

public static class DifficultySettings
{
    public enum DifficultyLevel
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Realistic = 3,
    }

    [System.Serializable]
    public class DifficultyMultipliers
    {
        public float enemyCount = 1.0f;
        public float enemyHealth = 1.0f;
        public float enemyDamage = 1.0f;

        public DifficultyMultipliers(float count, float health, float damage)
        {
            enemyCount = count;
            enemyHealth = health;
            enemyDamage = damage;
        }
    }

    private static readonly DifficultyMultipliers[] difficultyMultipliers =
        new DifficultyMultipliers[]
        {
            new DifficultyMultipliers(0.75f, 0.8f, 0.8f),
            new DifficultyMultipliers(1.0f, 1.0f, 1.0f),
            new DifficultyMultipliers(1.25f, 1.25f, 1.25f),
            new DifficultyMultipliers(1.5f, 1.5f, 1.5f),
        };

    public static DifficultyLevel GetCurrentDifficulty()
    {
        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);
        return (DifficultyLevel)Mathf.Clamp(difficulty, 0, 3);
    }

    public static DifficultyMultipliers GetCurrentMultipliers()
    {
        return difficultyMultipliers[(int)GetCurrentDifficulty()];
    }

    public static float GetEnemyCountMultiplier()
    {
        return GetCurrentMultipliers().enemyCount;
    }

    public static float GetEnemyHealthMultiplier()
    {
        return GetCurrentMultipliers().enemyHealth;
    }

    public static float GetEnemyDamageMultiplier()
    {
        return GetCurrentMultipliers().enemyDamage;
    }

    public static string GetDifficultyName(DifficultyLevel level)
    {
        return level switch
        {
            DifficultyLevel.Easy => "Easy",
            DifficultyLevel.Normal => "Normal",
            DifficultyLevel.Hard => "Hard",
            DifficultyLevel.Realistic => "Realistic",
            _ => "Normal",
        };
    }

    public static string GetDifficultyDescription(DifficultyLevel level)
    {
        var m = difficultyMultipliers[(int)level];
        return $"Enemies {m.enemyCount:P0}, Health {m.enemyHealth:P0}, Damage {m.enemyDamage:P0}";
    }

    public static string GetDifficultyDescriptionEN(DifficultyLevel level)
    {
        var m = difficultyMultipliers[(int)level];
        return $"Enemy Count {m.enemyCount:P0}, Health {m.enemyHealth:P0}, Damage {m.enemyDamage:P0}";
    }

    public static void LogCurrentDifficulty()
    {
        var level = GetCurrentDifficulty();
        var m = GetCurrentMultipliers();
        Debug.Log(
            $"[Difficulty] Current: {GetDifficultyName(level)} - "
                + $"Enemy Count: {m.enemyCount:F2}x, Health: {m.enemyHealth:F2}x, Damage: {m.enemyDamage:F2}x"
        );
    }
}
