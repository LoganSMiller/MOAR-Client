#nullable enable
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
using UnityEngine;

namespace MOAR
{
    internal static class Settings
    {
        private static ConfigFile _config = null!;
        private static bool _applyingPreset = false;
        public static bool IsInitialized { get; private set; } = false;
        public static bool IsFika { get; private set; } = false;
        public static ManualLogSource Log { get; private set; } = null!;

        // All config entries
        public static ConfigEntry<string>? currentPreset;
        public static ConfigEntry<bool>? ShowPresetOnRaidStart;
        public static ConfigEntry<KeyboardShortcut>? AnnounceKey;

        public static ConfigEntry<bool>? debug, factionAggression, enablePointOverlay, showConfigDebug,
            allowDebugSpawns, logPresetOnStart, BotOwnerConfigDebug;

        public static ConfigEntry<double>? pmcDifficulty, scavDifficulty;
        public static ConfigEntry<bool>? zombiesEnabled;
        public static ConfigEntry<double>? zombieWaveDistribution, zombieWaveQuantity, zombieHealth;

        public static ConfigEntry<double>? scavWaveDistribution, scavWaveQuantity, pmcWaveDistribution, pmcWaveQuantity;

        public static ConfigEntry<bool>? startingPmcs, spawnSmoothing, randomSpawns;
        public static ConfigEntry<int>? maxBotOwnerCap, maxBotOwnerPerZone, spawnRadius, spawnDelay;
        public static ConfigEntry<bool>? forceHotzonesOnly;

        public static ConfigEntry<double>? scavGroupChance, pmcGroupChance, sniperGroupChance;
        public static ConfigEntry<int>? pmcMaxGroupSize, scavMaxGroupSize;
        public static ConfigEntry<double>? sniperMaxGroupSize;

        public static ConfigEntry<bool>? bossOpenZones, randomRaiderGroup, randomRogueGroup, disableBosses,
            bossInvasion, gradualBossInvasion, enableBossOverrides;

        public static ConfigEntry<int>? randomRaiderGroupChance, randomRogueGroupChance, mainBossChanceBuff, bossInvasionSpawnChance;

        public static ConfigEntry<KeyboardShortcut>? DeleteBotOwnerSpawn, AddBotOwnerSpawn, AddSniperSpawn, AddPlayerSpawn;

        public static async Task InitAsync(ConfigFile config)
        {
            _config = config;
            Log = Plugin.LogSource;
            IsFika = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.fika.core");

            List<string> presetLabels = await FetchPresetsFromServer();

            currentPreset = config.Bind("1. Main Settings", "MOAR Preset", "live-like",
                new ConfigDescription("Preset to apply.", new AcceptableValueList<string>(presetLabels.ToArray())));

            ShowPresetOnRaidStart = config.Bind("1. Main Settings", "Preset Announce On/Off", true);
            AnnounceKey = config.Bind("1. Main Settings", "Announce Key", new KeyboardShortcut(KeyCode.End));
            factionAggression = config.Bind("1. Main Settings", "Faction-Based Aggression", false);
            debug = config.Bind("1. Main Settings", "Debug Mode", false);
            enablePointOverlay = config.Bind("1. Main Settings", "Enable Spawn Overlay", true);
            showConfigDebug = config.Bind("1. Main Settings", "Show Config Debug", false);
            allowDebugSpawns = config.Bind("1. Main Settings", "Allow Debug Spawns", false);
            logPresetOnStart = config.Bind("1. Main Settings", "Log Preset On Start", false);
            BotOwnerConfigDebug = config.Bind("1. Main Settings", "BotOwner Config Debug", false);

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
            maxBotOwnerCap = config.Bind("Spawns", "Max BotOwner Cap", 20);
            maxBotOwnerPerZone = config.Bind("Spawns", "Max BotOwners Per Zone", 6);
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

            DeleteBotOwnerSpawn = config.Bind("Hotkeys", "Delete BotOwner Spawn", new KeyboardShortcut(KeyCode.Delete));
            AddBotOwnerSpawn = config.Bind("Hotkeys", "Add BotOwner Spawn", new KeyboardShortcut(KeyCode.Insert));
            AddSniperSpawn = config.Bind("Hotkeys", "Add Sniper Spawn", new KeyboardShortcut(KeyCode.F1));
            AddPlayerSpawn = config.Bind("Hotkeys", "Add Player Spawn", new KeyboardShortcut(KeyCode.F2));

            IsInitialized = true;

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
                string host = "127.0.0.1:6969"; // fallback-safe, since this is local-only
                using var http = new HttpClient();
                string json = await http.GetStringAsync($"http://{host}/moar/getPresets");
                var parsed = JsonConvert.DeserializeObject<PresetsResponse>(json);
                return parsed?.data?.Select(p => p.Label).ToList() ?? new List<string> { "live-like" };
            }
            catch (Exception ex)
            {
                Log.LogError($"[Settings] Failed to fetch presets: {ex.Message}");
                return new List<string> { "live-like" };
            }
        }

        public static string GetCurrentPresetName() => currentPreset?.Value ?? "live-like";
        public static string GetCurrentPresetLabel() => currentPreset?.Value ?? "live-like";

        public static void AnnounceManually()
        {
            if (!IsInitialized || (IsFika && FikaBackendUtils.IsHeadless))
                return;

            Methods.DisplayMessage($"Current preset: {GetCurrentPresetLabel()}", ENotificationIconType.Quest);
        }

        public static bool AreHotkeysReady()
        {
            return IsInitialized &&
                   DeleteBotOwnerSpawn?.Value != null &&
                   AddBotOwnerSpawn?.Value != null &&
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
