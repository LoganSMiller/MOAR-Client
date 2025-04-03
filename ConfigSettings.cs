using System;

namespace MOAR
{
    /// <summary>
    /// Represents server-synced configuration settings for spawn and AI control.
    /// Used to apply behavior changes via presets and real-time overrides.
    /// </summary>
    public class ConfigSettings
    {
        // --- Preset Metadata ---

        /// <summary>Internal name of the preset.</summary>
        public string Name { get; set; }

        /// <summary>Human-readable label of the preset.</summary>
        public string Label { get; set; }

        // --- General AI Difficulty ---

        /// <summary>Overall PMC difficulty multiplier (0.0 to 2.0).</summary>
        public double pmcDifficulty { get; set; }

        /// <summary>Overall Scav difficulty multiplier (0.0 to 2.0).</summary>
        public double scavDifficulty { get; set; }

        // --- Scav Spawning ---

        public double scavWaveDistribution { get; set; }
        public double scavWaveQuantity { get; set; }

        // --- Zombie Behavior ---

        public bool zombiesEnabled { get; set; }
        public double zombieWaveDistribution { get; set; }
        public double zombieWaveQuantity { get; set; }
        public double zombieHealth { get; set; }

        // --- PMC Spawning ---

        public bool startingPmcs { get; set; }
        public bool spawnSmoothing { get; set; }
        public bool randomSpawns { get; set; }

        public double pmcWaveDistribution { get; set; }
        public double pmcWaveQuantity { get; set; }

        // --- Bot Limits ---

        public int maxBotCap { get; set; }
        public int maxBotPerZone { get; set; }

        // --- Grouping Probabilities ---

        public double scavGroupChance { get; set; }
        public double pmcGroupChance { get; set; }
        public double sniperGroupChance { get; set; }

        // --- Max Group Sizes ---

        public int pmcMaxGroupSize { get; set; }
        public int scavMaxGroupSize { get; set; }
        public double sniperMaxGroupSize { get; set; }

        // --- Boss Behavior ---

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

        // --- Debug Mode ---

        public bool debug { get; set; }
    }
}
