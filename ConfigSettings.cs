using System;

namespace MOAR
{
    /// <summary>
    /// Represents all server-configurable parameters for AI, spawns, bosses, and debugging.
    /// This structure is used for syncing presets between client and server.
    /// </summary>
    [Serializable]
    public class ConfigSettings
    {
        #region Preset Metadata

        /// <summary>Unique internal name of this preset.</summary>
        public string Name { get; set; } = "live-like";

        /// <summary>Friendly label displayed to the player.</summary>
        public string Label { get; set; } = "Live-Like";

        /// <summary>Fallback preset name used by the server.</summary>
        public string defaultPreset { get; set; } = "live-like";

        /// <summary>Enables or disables bot spawning system-wide.</summary>
        public bool enableBotSpawning { get; set; } = true;

        #endregion

        #region Difficulty

        /// <summary>PMC AI difficulty multiplier (0.0 to 1.0+).</summary>
        public double pmcDifficulty { get; set; } = 0.6;

        /// <summary>Scav AI difficulty multiplier (0.0 to 1.0+).</summary>
        public double scavDifficulty { get; set; } = 0.4;

        #endregion

        #region PMC Spawning

        /// <summary>Relative distribution of PMCs over raid time.</summary>
        public double pmcWaveDistribution { get; set; } = 0.6;

        /// <summary>Multiplier for PMC total spawn count.</summary>
        public double pmcWaveQuantity { get; set; } = 1;

        #endregion

        #region Scav Spawning

        /// <summary>Relative distribution of Scavs over raid time.</summary>
        public double scavWaveDistribution { get; set; } = 1;

        /// <summary>Multiplier for Scav total spawn count.</summary>
        public double scavWaveQuantity { get; set; } = 1;

        #endregion

        #region Zombie Settings

        /// <summary>Enables zombie waves instead of standard bots.</summary>
        public bool zombiesEnabled { get; set; } = false;

        /// <summary>Distribution of zombie waves across raid time.</summary>
        public double zombieWaveDistribution { get; set; } = 0.8;

        /// <summary>Multiplier for total zombie wave count.</summary>
        public double zombieWaveQuantity { get; set; } = 1;

        /// <summary>Global zombie health multiplier.</summary>
        public double zombieHealth { get; set; } = 1;

        #endregion

        #region Spawn Behavior

        /// <summary>Whether PMC waves can spawn right at the start.</summary>
        public bool startingPmcs { get; set; } = false;

        /// <summary>Whether to apply spawn smoothing to waves.</summary>
        public bool spawnSmoothing { get; set; } = true;

        /// <summary>Whether bot spawn points are randomized.</summary>
        public bool randomSpawns { get; set; } = false;

        /// <summary>Radius in meters around the player to spawn bots.</summary>
        public int spawnRadius { get; set; } = 20;

        /// <summary>Delay in seconds before spawn can occur.</summary>
        public int spawnDelay { get; set; } = 4;

        /// <summary>Restrict all bot spawns to hotzones only.</summary>
        public bool forceHotzonesOnly { get; set; } = false;

        #endregion

        #region Bot Limits

        /// <summary>Total simultaneous bots allowed on the map.</summary>
        public int maxBotCap { get; set; } = 20;

        /// <summary>Maximum bots allowed per BotZone at once.</summary>
        public int maxBotPerZone { get; set; } = 6;

        #endregion

        #region Grouping Chances

        /// <summary>Chance of Scavs spawning in a group (0–1).</summary>
        public double scavGroupChance { get; set; } = 0.2;

        /// <summary>Chance of PMCs spawning in a group (0–1).</summary>
        public double pmcGroupChance { get; set; } = 0.2;

        /// <summary>Chance of Sniper bots spawning in a group (0–1).</summary>
        public double sniperGroupChance { get; set; } = 0.1;

        #endregion

        #region Group Sizes

        /// <summary>Maximum group size for PMCs.</summary>
        public int pmcMaxGroupSize { get; set; } = 4;

        /// <summary>Maximum group size for Scavs.</summary>
        public int scavMaxGroupSize { get; set; } = 3;

        /// <summary>Maximum group size for Snipers.</summary>
        public double sniperMaxGroupSize { get; set; } = 1;

        #endregion

        #region Boss Spawning

        /// <summary>Allows bosses to spawn outside of hotzones.</summary>
        public bool bossOpenZones { get; set; } = false;

        /// <summary>Enable random Raider group injection.</summary>
        public bool randomRaiderGroup { get; set; } = false;

        /// <summary>Chance (%) of random Raider group appearing.</summary>
        public int randomRaiderGroupChance { get; set; } = 10;

        /// <summary>Enable random Rogue group injection.</summary>
        public bool randomRogueGroup { get; set; } = false;

        /// <summary>Chance (%) of random Rogue group appearing.</summary>
        public int randomRogueGroupChance { get; set; } = 10;

        /// <summary>Globally disable all bosses and guards.</summary>
        public bool disableBosses { get; set; } = false;

        /// <summary>Boost applied to main boss appearance rate.</summary>
        public int mainBossChanceBuff { get; set; } = 0;

        /// <summary>Allow boss groups to invade other zones.</summary>
        public bool bossInvasion { get; set; } = false;

        /// <summary>Chance (%) of a boss invasion wave appearing.</summary>
        public int bossInvasionSpawnChance { get; set; } = 5;

        /// <summary>If true, boss invasions become more likely over time.</summary>
        public bool gradualBossInvasion { get; set; } = true;

        /// <summary>Allow map-specific overrides for boss settings.</summary>
        public bool enableBossOverrides { get; set; } = true;

        #endregion

        #region Debugging

        /// <summary>Enable verbose debug mode for diagnostics.</summary>
        public bool debug { get; set; } = false;

        /// <summary>Enable logging of all bot spawn events.</summary>
        public bool logSpawnData { get; set; } = false;

        /// <summary>Log override decisions for boss behavior.</summary>
        public bool logBossOverrides { get; set; } = false;

        #endregion

        public override string ToString()
        {
            return $"[ConfigSettings] Preset: {Label} | Bots Enabled: {enableBotSpawning} | PMCDiff: {pmcDifficulty}, ScavDiff: {scavDifficulty}";
        }
    }
}
