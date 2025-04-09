using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Communications;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;
using Newtonsoft.Json;

namespace MOAR
{
    internal static class Settings
    {
        private static ConfigFile _config = null!;
        private static bool _applyingPreset = false;

        // === Preset + UI ===
        public static ConfigEntry<string> currentPreset = null!;
        public static ConfigEntry<bool> ShowPresetOnRaidStart = null!;
        public static ConfigEntry<KeyboardShortcut> AnnounceKey = null!;

        // === Debug & Overlay ===
        public static ConfigEntry<bool> debug = null!;
        public static ConfigEntry<bool> factionAggression = null!;
        public static ConfigEntry<bool> enablePointOverlay = null!;
        public static ConfigEntry<bool> showConfigDebug = null!;
        public static ConfigEntry<bool> allowDebugSpawns = null!;
        public static ConfigEntry<bool> logPresetOnStart = null!;
        public static ConfigEntry<bool> botConfigDebug = null!;

        // === AI Difficulty ===
        public static ConfigEntry<double> pmcDifficulty = null!;
        public static ConfigEntry<double> scavDifficulty = null!;

        // === Zombie Settings ===
        public static ConfigEntry<bool> zombiesEnabled = null!;
        public static ConfigEntry<double> zombieWaveDistribution = null!;
        public static ConfigEntry<double> zombieWaveQuantity = null!;
        public static ConfigEntry<double> zombieHealth = null!;

        // === Wave Settings ===
        public static ConfigEntry<double> scavWaveDistribution = null!;
        public static ConfigEntry<double> scavWaveQuantity = null!;
        public static ConfigEntry<double> pmcWaveDistribution = null!;
        public static ConfigEntry<double> pmcWaveQuantity = null!;

        // === Spawn Logic ===
        public static ConfigEntry<bool> startingPmcs = null!;
        public static ConfigEntry<bool> spawnSmoothing = null!;
        public static ConfigEntry<bool> randomSpawns = null!;
        public static ConfigEntry<int> maxBotCap = null!;
        public static ConfigEntry<int> maxBotPerZone = null!;
        public static ConfigEntry<int> spawnRadius = null!;
        public static ConfigEntry<int> spawnDelay = null!;
        public static ConfigEntry<bool> forceHotzonesOnly = null!;

        // === Grouping ===
        public static ConfigEntry<double> scavGroupChance = null!;
        public static ConfigEntry<double> pmcGroupChance = null!;
        public static ConfigEntry<double> sniperGroupChance = null!;
        public static ConfigEntry<int> pmcMaxGroupSize = null!;
        public static ConfigEntry<int> scavMaxGroupSize = null!;
        public static ConfigEntry<double> sniperMaxGroupSize = null!;

        // === Boss Logic ===
        public static ConfigEntry<bool> bossOpenZones = null!;
        public static ConfigEntry<bool> randomRaiderGroup = null!;
        public static ConfigEntry<int> randomRaiderGroupChance = null!;
        public static ConfigEntry<bool> randomRogueGroup = null!;
        public static ConfigEntry<int> randomRogueGroupChance = null!;
        public static ConfigEntry<bool> disableBosses = null!;
        public static ConfigEntry<int> mainBossChanceBuff = null!;
        public static ConfigEntry<bool> bossInvasion = null!;
        public static ConfigEntry<int> bossInvasionSpawnChance = null!;
        public static ConfigEntry<bool> gradualBossInvasion = null!;
        public static ConfigEntry<bool> enableBossOverrides = null!;

        // === Hotkeys ===
        public static ConfigEntry<KeyboardShortcut> DeleteBotSpawn = null!;
        public static ConfigEntry<KeyboardShortcut> AddBotSpawn = null!;
        public static ConfigEntry<KeyboardShortcut> AddSniperSpawn = null!;
        public static ConfigEntry<KeyboardShortcut> AddPlayerSpawn = null!;

        public static bool IsFika { get; private set; }
        public static ManualLogSource Log { get; private set; } = null!;

        public static async Task InitAsync(ConfigFile config)
        {
            _config = config;
            Log = Plugin.LogSource;
            IsFika = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core");

            List<string> presetLabels = await FetchPresetsFromServer();

            currentPreset = config.Bind("1. Main Settings", "MOAR Preset", "live-like",
                new ConfigDescription("Preset to apply.", new AcceptableValueList<string>(presetLabels.ToArray())));

            ShowPresetOnRaidStart = config.Bind("1. Main Settings", "Preset Announce On/Off", true);
            AnnounceKey = config.Bind("1. Main Settings", "Announce Key", new KeyboardShortcut(UnityEngine.KeyCode.End));
            factionAggression = config.Bind("1. Main Settings", "Faction-Based Aggression", false);
            debug = config.Bind("1. Main Settings", "Debug Mode", false);
            enablePointOverlay = config.Bind("1. Main Settings", "Enable Spawn Overlay", true);
            showConfigDebug = config.Bind("1. Main Settings", "Show Config Debug", false);
            allowDebugSpawns = config.Bind("1. Main Settings", "Allow Debug Spawns", false);
            logPresetOnStart = config.Bind("1. Main Settings", "Log Preset On Start", false);
            botConfigDebug = config.Bind("1. Main Settings", "Bot Config Debug", false);

            pmcDifficulty = config.Bind("AI", "PMC Difficulty", 0.6);
            scavDifficulty = config.Bind("AI", "Scav Difficulty", 0.4);

            zombiesEnabled = config.Bind("Zombies", "Zombies Enabled", false);
            zombieWaveDistribution = config.Bind("Zombies", "Zombie Wave Distribution", 0.8);
            zombieWaveQuantity = config.Bind("Zombies", "Zombie Wave Quantity", 1.0);
            zombieHealth = config.Bind("Zombies", "Zombie Health", 1.0);

            scavWaveDistribution = config.Bind("Spawns", "Scav Wave Distribution", 1.0);
            scavWaveQuantity = config.Bind("Spawns", "Scav Wave Quantity", 1.0);
            pmcWaveDistribution = config.Bind("Spawns", "PMC Wave Distribution", 0.6);
            pmcWaveQuantity = config.Bind("Spawns", "PMC Wave Quantity", 1.0);

            startingPmcs = config.Bind("Spawns", "Starting PMCs Enabled", false);
            spawnSmoothing = config.Bind("Spawns", "Enable Smoothing", true);
            randomSpawns = config.Bind("Spawns", "Random Spawns Enabled", false);

            maxBotCap = config.Bind("Spawns", "Max Bot Cap", 20);
            maxBotPerZone = config.Bind("Spawns", "Max Bots Per Zone", 6);
            spawnRadius = config.Bind("Spawns", "Spawn Radius", 20);
            spawnDelay = config.Bind("Spawns", "Spawn Delay", 4);
            forceHotzonesOnly = config.Bind("Spawns", "Force Hotzones Only", false);

            scavGroupChance = config.Bind("Groups", "Scav Group Chance", 0.2);
            pmcGroupChance = config.Bind("Groups", "PMC Group Chance", 0.2);
            sniperGroupChance = config.Bind("Groups", "Sniper Group Chance", 0.1);
            pmcMaxGroupSize = config.Bind("Groups", "PMC Max Group Size", 4);
            scavMaxGroupSize = config.Bind("Groups", "Scav Max Group Size", 3);
            sniperMaxGroupSize = config.Bind("Groups", "Sniper Max Group Size", 1.0);

            bossOpenZones = config.Bind("Bosses", "Bosses Use Open Zones", false);
            randomRaiderGroup = config.Bind("Bosses", "Inject Random Raider Group", false);
            randomRaiderGroupChance = config.Bind("Bosses", "Raider Group Chance", 10);
            randomRogueGroup = config.Bind("Bosses", "Inject Random Rogue Group", false);
            randomRogueGroupChance = config.Bind("Bosses", "Rogue Group Chance", 10);
            disableBosses = config.Bind("Bosses", "Disable All Bosses", false);
            mainBossChanceBuff = config.Bind("Bosses", "Main Boss Chance Buff", 0);
            bossInvasion = config.Bind("Bosses", "Boss Invasion Mode", false);
            bossInvasionSpawnChance = config.Bind("Bosses", "Boss Invasion Spawn Chance", 5);
            gradualBossInvasion = config.Bind("Bosses", "Gradual Invasion", true);
            enableBossOverrides = config.Bind("Bosses", "Enable Boss Config Overrides", true);

            DeleteBotSpawn = config.Bind("Hotkeys", "Delete Bot Spawn", new KeyboardShortcut(UnityEngine.KeyCode.Delete));
            AddBotSpawn = config.Bind("Hotkeys", "Add Bot Spawn", new KeyboardShortcut(UnityEngine.KeyCode.Insert));
            AddSniperSpawn = config.Bind("Hotkeys", "Add Sniper Spawn", new KeyboardShortcut(UnityEngine.KeyCode.F1));
            AddPlayerSpawn = config.Bind("Hotkeys", "Add Player Spawn", new KeyboardShortcut(UnityEngine.KeyCode.F2));

            if (ShowPresetOnRaidStart.Value && (!IsFika || FikaBackendUtils.IsServer))
            {
                Methods.DisplayMessage($"Live preset: {currentPreset.Value}", ENotificationIconType.Quest);
            }

            Log.LogInfo($"[Settings] Initialization complete. Selected preset: {currentPreset.Value}");
        }

        private static async Task<List<string>> FetchPresetsFromServer()
        {
            try
            {
                using var http = new HttpClient();
                var res = await http.GetStringAsync("http://127.0.0.1:6969/moar/getPresets");
                var parsed = JsonConvert.DeserializeObject<PresetsResponse>(res);
                return parsed?.data?.Select(p => p.Label).ToList() ?? new List<string> { "live-like" };
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Settings] Failed to fetch presets: {ex.Message}");
                return new List<string> { "live-like" };
            }
        }

        public static string GetCurrentPresetName() => currentPreset?.Value ?? "live-like";
        public static string GetCurrentPresetLabel() => currentPreset?.Value ?? "live-like";

        public static void AnnounceManually()
        {
            if (IsFika && FikaBackendUtils.IsHeadless)
                return;

            Methods.DisplayMessage($"Current preset: {GetCurrentPresetLabel()}", ENotificationIconType.Quest);
        }

        public static bool AreHotkeysReady()
        {
            return DeleteBotSpawn?.Value != null &&
                   AddBotSpawn?.Value != null &&
                   AddSniperSpawn?.Value != null &&
                   AddPlayerSpawn?.Value != null &&
                   AnnounceKey?.Value != null;
        }
    }

    public class PresetsResponse
    {
        public List<PresetLabel> data { get; set; } = new();
    }

    public class PresetLabel
    {
        public string Name { get; set; } = "Unknown";
        public string Label { get; set; } = "unknown";
    }

    public static class ConfigEntryExtensions
    {
        public static bool BetterIsDown(this KeyboardShortcut shortcut) => shortcut.IsDown();
    }
}

