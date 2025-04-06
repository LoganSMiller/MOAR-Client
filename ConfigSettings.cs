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
        // --- Metadata ---
        public string Name { get; set; } = "live-like";
        public string Label { get; set; } = "Live-Like";
        public bool enableBotSpawning { get; set; } = true;

        // --- AI Difficulty ---
        public double pmcDifficulty { get; set; } = 0.6;
        public double scavDifficulty { get; set; } = 0.4;

        // --- PMC Spawning ---
        public double pmcWaveDistribution { get; set; } = 0.6;
        public double pmcWaveQuantity { get; set; } = 1;

        // --- Scav Spawning ---
        public double scavWaveDistribution { get; set; } = 1;
        public double scavWaveQuantity { get; set; } = 1;

        // --- Zombie Behavior ---
        public bool zombiesEnabled { get; set; } = false;
        public double zombieWaveDistribution { get; set; } = 0.8;
        public double zombieWaveQuantity { get; set; } = 1;
        public double zombieHealth { get; set; } = 1;

        // --- Spawn Logic ---
        public bool startingPmcs { get; set; } = false;
        public bool spawnSmoothing { get; set; } = true;
        public bool randomSpawns { get; set; } = false;
        public int spawnRadius { get; set; } = 20;
        public int spawnDelay { get; set; } = 4;
        public bool forceHotzonesOnly { get; set; } = false;

        // --- Limits ---
        public int maxBotCap { get; set; } = 20;
        public int maxBotPerZone { get; set; } = 6;

        // --- Group Chances ---
        public double scavGroupChance { get; set; } = 0.2;
        public double pmcGroupChance { get; set; } = 0.2;
        public double sniperGroupChance { get; set; } = 0.1;

        // --- Group Sizes ---
        public int pmcMaxGroupSize { get; set; } = 4;
        public int scavMaxGroupSize { get; set; } = 3;
        public double sniperMaxGroupSize { get; set; } = 1;

        // --- Boss Spawning ---
        public bool bossOpenZones { get; set; } = false;
        public bool randomRaiderGroup { get; set; } = false;
        public int randomRaiderGroupChance { get; set; } = 10;
        public bool randomRogueGroup { get; set; } = false;
        public int randomRogueGroupChance { get; set; } = 10;
        public bool disableBosses { get; set; } = false;
        public int mainBossChanceBuff { get; set; } = 0;
        public bool bossInvasion { get; set; } = false;
        public int bossInvasionSpawnChance { get; set; } = 5;
        public bool gradualBossInvasion { get; set; } = true;
        public bool enableBossOverrides { get; set; } = true;

        // --- Debugging ---
        public bool debug { get; set; } = false;
        public bool logSpawnData { get; set; } = false;
        public bool logBossOverrides { get; set; } = false;

        public override string ToString() =>
            $"[Config] {Label} | Bots: {enableBotSpawning} | PMC Diff: {pmcDifficulty}, Scav Diff: {scavDifficulty}";
    }
}
