using System;

namespace MOAR
{
    /// <summary>
    /// Represents server-synced configuration settings for spawn and AI control.
    /// Used to apply behavior changes via presets and real-time overrides.
    /// </summary>
    public class ConfigSettings
    {
        public string Name { get; set; }
        public string Label { get; set; }

        public double pmcDifficulty { get; set; }
        public double scavDifficulty { get; set; }

        public double scavWaveDistribution { get; set; }
        public double scavWaveQuantity { get; set; }

        public bool zombiesEnabled { get; set; }
        public double zombieWaveDistribution { get; set; }
        public double zombieWaveQuantity { get; set; }
        public double zombieHealth { get; set; }

        public bool startingPmcs { get; set; }
        public bool spawnSmoothing { get; set; }
        public bool randomSpawns { get; set; }

        public double PmcGroupChance { get; set; }
        public int PmcMaxGroupSize { get; set; }

        public int maxBotCap { get; set; }
        public int maxBotPerZone { get; set; }

        public double scavGroupChance { get; set; }
        public double pmcGroupChance { get; set; }
        public double sniperGroupChance { get; set; }

        public int pmcMaxGroupSize { get; set; }
        public int scavMaxGroupSize { get; set; }
        public double sniperMaxGroupSize { get; set; }

        public bool bossOpenZones { get; set; }

        public bool RandomRogueGroup { get; set; }
        public int RandomRogueGroupChance { get; set; }

        public bool DisableBosses { get; set; }
        public int MainBossChanceBuff { get; set; }

        public bool BossInvasion { get; set; }
        public int BossInvasionSpawnChance { get; set; }
        public bool GradualBossInvasion { get; set; }

        // --- Debug Mode ---

        public bool debug { get; set; }
    }
}
