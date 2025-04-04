using System;

namespace MOAR
{
    /// <summary>
    /// Represents all server-configurable parameters for AI and spawn behavior.
    /// Used for syncing between client/server and applying presets.
    /// </summary>
    public class ConfigSettings
    {
        // --- Preset Metadata ---
        public string Name { get; set; }
        public string Label { get; set; }

        // --- Difficulty ---
        public double pmcDifficulty { get; set; }
        public double scavDifficulty { get; set; }

        // --- PMC Spawning ---
        public double pmcWaveDistribution { get; set; }
        public double pmcWaveQuantity { get; set; }

        // --- Scav Spawning ---
        public double scavWaveDistribution { get; set; }
        public double scavWaveQuantity { get; set; }

        // --- Zombie Behavior ---
        public bool zombiesEnabled { get; set; }
        public double zombieWaveDistribution { get; set; }
        public double zombieWaveQuantity { get; set; }
        public double zombieHealth { get; set; }

        // --- Spawn Behavior ---
        public bool startingPmcs { get; set; }
        public bool spawnSmoothing { get; set; }
        public bool randomSpawns { get; set; }

        // --- Bot Limits ---
        public int maxBotCap { get; set; }
        public int maxBotPerZone { get; set; }

        // --- Grouping Chances ---
        public double scavGroupChance { get; set; }
        public double pmcGroupChance { get; set; }
        public double sniperGroupChance { get; set; }

        // --- Group Sizes ---
        public int pmcMaxGroupSize { get; set; }
        public int scavMaxGroupSize { get; set; }
        public double sniperMaxGroupSize { get; set; }

        // --- Boss Spawning ---
        public bool bossOpenZones { get; set; }
        public bool randomRaiderGroup { get; set; }
        public int randomRaiderGroupChance { get; set; }
        public bool randomRogueGroup { get; set; }
        public int randomRogueGroupChance { get; set; }
        public bool disableBosses { get; set; }
        public int mainBossChanceBuff { get; set; }
        public bool bossInvasion { get; set; }
        public int bossInvasionSpawnChance { get; set; }
        public bool gradualBossInvasion { get; set; }

        // --- Debug ---
        public bool debug { get; set; }
    }
}
