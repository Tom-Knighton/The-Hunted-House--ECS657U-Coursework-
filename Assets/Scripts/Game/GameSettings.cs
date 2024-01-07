namespace Game
{
    public static class CurrentGameSettings
    {
        public static GameSettings Settings { get; set; } = new()
        {
            EnemySettings = new EnemySpawnSettings
            {
                MainBossAttack = 15,
                MainBossHealth = 500,
                MainBossRegenRate = 3,
                MiniBossAttack = 13,
                MiniBossHealth = 100,
                MiniBossRegenRate = 3
            },
            KeysRequired = 5,
            PlayerAttackMultiplier = 1
        };
    }
    
    public class GameSettings
    {
        public EnemySpawnSettings EnemySettings { get; set; }
        public int KeysRequired { get; set; }
        public float PlayerAttackMultiplier { get; set; }
    }

    public class EnemySpawnSettings
    {
        public float MainBossHealth { get; set; }
        public float MainBossRegenRate { get; set; }
        public float MainBossAttack { get; set; }
        
        public float MiniBossHealth { get; set; }
        public float MiniBossRegenRate { get; set; }
        public float MiniBossAttack { get; set; }
    }
}