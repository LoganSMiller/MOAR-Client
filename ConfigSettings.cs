using System;

namespace MOAR
{
    /// <summary>
    /// Represents all server-driven config values for AI, spawning, groups, bosses, and debugging.
    /// This object defines the authoritative settings applied by the host.
    /// </summary>
    [Serializable]
    public class ConfigSettings
    {
        // === Metadata ===

        public string Name { get; set; } = "live-like";
        public string Label { get; set; } = "Live-Like";
        public bool EnableBotSpawning { get; set; } = true;

        // === AI Difficulty ===

        public double PmcDifficulty { get; set; } = 0.6;
        public double ScavDifficulty { get; set; } = 0.4;

        // === PMC Spawning ===

        public double PmcWaveDistribution { get; set; } = 0.6;
        public double PmcWaveQuantity { get; set; } = 1;

        // === Scav Spawning ===

        public double ScavWaveDistribution { get; set; } = 1.0;
        public double ScavWaveQuantity { get; set; } = 1;

        // === Zombie Spawning ===

        public bool ZombiesEnabled { get; set; } = false;
        public double ZombieWaveDistribution { get; set; } = 0.8;
        public double ZombieWaveQuantity { get; set; } = 1;
        public double ZombieHealth { get; set; } = 100;

        // === Spawn Logic ===

        public bool StartingPmcs { get; set; } = false;
        public bool SpawnSmoothing { get; set; } = true;
        public bool RandomSpawns { get; set; } = false;
        public int SpawnRadius { get; set; } = 20;
        public int SpawnDelay { get; set; } = 4;
        public bool ForceHotzonesOnly { get; set; } = false;

        // === Spawn Limits ===

        public int MaxBotCap { get; set; } = 20;
        public int MaxBotPerZone { get; set; } = 6;

        // === Group Chances ===

        public double ScavGroupChance { get; set; } = 0.2;
        public double PmcGroupChance { get; set; } = 0.2;
        public double SniperGroupChance { get; set; } = 0.1;

        // === Group Sizes ===

        public int PmcMaxGroupSize { get; set; } = 4;
        public int ScavMaxGroupSize { get; set; } = 3;
        public double SniperMaxGroupSize { get; set; } = 1;

        // === Boss Spawning ===

        public bool BossOpenZones { get; set; } = false;
        public bool RandomRaiderGroup { get; set; } = false;
        public int RandomRaiderGroupChance { get; set; } = 10;
        public bool RandomRogueGroup { get; set; } = false;
        public int RandomRogueGroupChance { get; set; } = 10;
        public bool DisableBosses { get; set; } = false;
        public int MainBossChanceBuff { get; set; } = 0;
        public bool BossInvasion { get; set; } = false;
        public int BossInvasionSpawnChance { get; set; } = 5;
        public bool GradualBossInvasion { get; set; } = true;
        public bool EnableBossOverrides { get; set; } = true;

        // === Debugging ===

        public bool Debug { get; set; } = false;
        public bool LogSpawnData { get; set; } = false;
        public bool LogBossOverrides { get; set; } = false;

        public override string ToString() =>
            $"[Config] {Label} | Bots: {EnableBotSpawning} | PMC Diff: {PmcDifficulty}, Scav Diff: {ScavDifficulty}";
    }
}
