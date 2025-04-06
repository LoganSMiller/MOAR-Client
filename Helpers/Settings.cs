using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Communications;
using Newtonsoft.Json;
using UnityEngine;
using MOAR.Helpers;
using Fika.Core.Coop.Utils;
using MOAR.Packets;

namespace MOAR.Helpers
{
    internal static class Settings
    {
        private static ConfigFile _config;
        private static bool _applyingPreset = false;

        // ConfigEntry fields
        public static ConfigEntry<bool> ShowPresetOnRaidStart;
        public static ConfigEntry<KeyboardShortcut> AnnounceKey;
        public static ConfigEntry<string> currentPreset;

        public static ConfigEntry<bool> debug;
        public static ConfigEntry<bool> factionAggression;
        public static ConfigEntry<bool> enablePointOverlay;

        public static ConfigEntry<double> pmcDifficulty;
        public static ConfigEntry<double> scavDifficulty;

        public static ConfigEntry<bool> zombiesEnabled;
        public static ConfigEntry<double> zombieWaveDistribution;
        public static ConfigEntry<double> zombieWaveQuantity;
        public static ConfigEntry<double> zombieHealth;

        public static ConfigEntry<double> scavWaveDistribution;
        public static ConfigEntry<double> scavWaveQuantity;
        public static ConfigEntry<double> pmcWaveDistribution;
        public static ConfigEntry<double> pmcWaveQuantity;

        public static ConfigEntry<bool> startingPmcs;
        public static ConfigEntry<bool> spawnSmoothing;
        public static ConfigEntry<bool> randomSpawns;

        public static ConfigEntry<int> maxBotCap;
        public static ConfigEntry<int> maxBotPerZone;
        public static ConfigEntry<int> spawnRadius;
        public static ConfigEntry<int> spawnDelay;

        public static ConfigEntry<double> scavGroupChance;
        public static ConfigEntry<double> pmcGroupChance;
        public static ConfigEntry<double> sniperGroupChance;

        public static ConfigEntry<int> pmcMaxGroupSize;
        public static ConfigEntry<int> scavMaxGroupSize;
        public static ConfigEntry<double> sniperMaxGroupSize;

        public static ConfigEntry<bool> bossOpenZones;
        public static ConfigEntry<bool> randomRaiderGroup;
        public static ConfigEntry<int> randomRaiderGroupChance;
        public static ConfigEntry<bool> randomRogueGroup;
        public static ConfigEntry<int> randomRogueGroupChance;
        public static ConfigEntry<bool> disableBosses;
        public static ConfigEntry<int> mainBossChanceBuff;
        public static ConfigEntry<bool> bossInvasion;
        public static ConfigEntry<int> bossInvasionSpawnChance;
        public static ConfigEntry<bool> gradualBossInvasion;
        public static ConfigEntry<bool> enableBossOverrides;
        public static ConfigEntry<bool> forceHotzonesOnly;

        // Bot spawn settings
        public static ConfigEntry<KeyboardShortcut> DeleteBotSpawn;
        public static ConfigEntry<KeyboardShortcut> AddBotSpawn;
        public static ConfigEntry<KeyboardShortcut> AddSniperSpawn;
        public static ConfigEntry<KeyboardShortcut> AddPlayerSpawn;

        public static bool IsFika;
        public static ConfigSettings ServerStoredValues;
        public static ConfigSettings ServerStoredDefaults;
        public static ManualLogSource Log;
        public static List<Preset> PresetList;
        public static double LastUpdatedServer;

        // Init method for Settings
        public static void Init(ConfigFile config)
        {
            _config = config;
            Log = Plugin.LogSource;
            IsFika = Chainloader.PluginInfos.ContainsKey("com.fika.core");

            Log.LogInfo("[Settings] Fetching config and presets from server...");

            try
            {
                ServerStoredDefaults = Routers.GetDefaultConfig() ?? new ConfigSettings();
                ServerStoredValues = Routers.GetServerConfigWithOverrides() ?? new ConfigSettings();
                PresetList = Routers.GetPresetsList() ?? new List<Preset>();
            }
            catch (Exception ex)
            {
                Log.LogError($"[Settings] Failed to fetch remote config: {ex.Message}");
                ServerStoredDefaults = new ConfigSettings();
                ServerStoredValues = new ConfigSettings();
                PresetList = new List<Preset>();
            }

            if (PresetList.Count == 0)
            {
                Log.LogWarning("[Settings] No presets found — using fallback.");
                PresetList.Add(new Preset { Name = "live-like", Label = "Live Like" });
            }

            string fallbackPresetName = config.Bind("1. Main Settings", "Moar Preset Fallback", "live-like").Value?.Trim();
            string liveLabel = Routers.GetCurrentPresetLabel()?.Trim();

            var selectedPreset = PresetList.FirstOrDefault(p =>
                string.Equals(p.Label?.Trim(), liveLabel, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name?.Trim(), liveLabel, StringComparison.OrdinalIgnoreCase)) ??
                PresetList.FirstOrDefault(p => string.Equals(p.Name, fallbackPresetName, StringComparison.OrdinalIgnoreCase)) ??
                PresetList.First();

            currentPreset = config.Bind("1. Main Settings", "Moar Preset",
                selectedPreset?.Name ?? "live-like",
                new ConfigDescription("Preset to apply.", new AcceptableValueList<string>(PresetList.Select(p => p.Name).ToArray())));

            // Bind ConfigEntries for missing settings
            ShowPresetOnRaidStart = config.Bind("1. Main Settings", "Preset Announce On/Off", true);
            AnnounceKey = config.Bind("1. Main Settings", "Announce Key", new KeyboardShortcut(KeyCode.End));
            factionAggression = config.Bind("1. Main Settings", "Faction Based Aggression On/Off", false);

            // Bind spawn-related settings
            DeleteBotSpawn = config.Bind("Bot Spawn Settings", "Delete Bot Spawn Key", new KeyboardShortcut(KeyCode.Delete));
            AddBotSpawn = config.Bind("Bot Spawn Settings", "Add Bot Spawn Key", new KeyboardShortcut(KeyCode.Insert));
            AddSniperSpawn = config.Bind("Bot Spawn Settings", "Add Sniper Spawn Key", new KeyboardShortcut(KeyCode.F1));
            AddPlayerSpawn = config.Bind("Bot Spawn Settings", "Add Player Spawn Key", new KeyboardShortcut(KeyCode.F2));

            // Bind additional settings
            debug = config.Bind("1. Main Settings", "Debug Mode", false);
            enablePointOverlay = config.Bind("1. Main Settings", "Enable Point Overlay", true);
            startingPmcs = config.Bind("1. Main Settings", "Starting PMCs On/Off", ServerStoredDefaults.startingPmcs);
            spawnSmoothing = config.Bind("1. Main Settings", "Spawn Smoothing On/Off", ServerStoredDefaults.spawnSmoothing);
            randomSpawns = config.Bind("1. Main Settings", "Random Spawns On/Off", ServerStoredDefaults.randomSpawns);
            pmcDifficulty = config.Bind("1. Main Settings", "PMC Difficulty", ServerStoredDefaults.pmcDifficulty);
            scavDifficulty = config.Bind("1. Main Settings", "Scav Difficulty", ServerStoredDefaults.scavDifficulty);

            BindWaveSettings();
            BindBossSettings();
            BindDebugAndOverlay();
            BindKeys();

            currentPreset.SettingChanged += (_, _) => OnPresetChange();
            startingPmcs.SettingChanged += (_, _) => OnStartingPmcsChanged();
            spawnSmoothing.SettingChanged += (_, _) => OnStartingPmcsChanged();
            randomSpawns.SettingChanged += (_, _) => OnStartingPmcsChanged();


            // Display the preset if not in headless mode
            if (!IsFika && ShowPresetOnRaidStart.Value && !FikaBackendUtils.IsHeadless)
            {
                Methods.DisplayMessage($"Live preset: {selectedPreset?.Label ?? selectedPreset?.Name}", ENotificationIconType.Quest);
            }

            Log.LogInfo($"[Settings] Initialization complete. Selected preset: {selectedPreset?.Name}");
        }

        // Additional helper methods for settings binding and logic
        private static void BindWaveSettings() { /* Unchanged for now */ }
        private static void BindBossSettings() { /* Unchanged for now */ }
        private static void BindDebugAndOverlay() { /* Unchanged for now */ }
        private static void BindKeys() { /* Unchanged for now */ }

        // Preset change handling
        private static void OnPresetChange()
        {
            if (_applyingPreset) return;
            _applyingPreset = true;

            try
            {
                var selected = PresetList.FirstOrDefault(p => p.Name == currentPreset.Value);
                if (selected != null)
                {
                    ApplyPresetSettings(selected);
                    Methods.DisplayMessage($"Current preset: {selected.Label}", ENotificationIconType.Quest);
                    Routers.SetPreset(selected.Name);
                }
                else
                {
                    Log.LogWarning($"[Settings] Preset not found: {currentPreset.Value}");
                    currentPreset.Value = "live-like";
                    Methods.DisplayMessage("Unknown preset selected — fallback applied", ENotificationIconType.Alert);
                }
            }
            finally { _applyingPreset = false; }
        }

        private static void OnStartingPmcsChanged()
        {
            if (startingPmcs.Value)
            {
                randomSpawns.Value = true;
                spawnSmoothing.Value = false;
            }
        }

        // Apply settings for a specific preset
        private static void ApplyPresetSettings(Preset preset)
        {
            if (preset?.Settings == null) return;

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonConvert.SerializeObject(preset.Settings));

                void TrySet<T>(string key, Action<T> setter)
                {
                    if (dict.TryGetValue(key, out var val))
                    {
                        try { setter((T)Convert.ChangeType(val, typeof(T))); } catch { }
                    }
                }

                TrySet("randomSpawns", (bool v) => randomSpawns.Value = v);
                TrySet("spawnSmoothing", (bool v) => spawnSmoothing.Value = v);
                TrySet("startingPmcs", (bool v) => startingPmcs.Value = v);
                // More settings...

                Log.LogInfo($"[Settings] Applied preset: {preset.Name}");
            }
            catch (Exception ex)
            {
                Log.LogError($"[Settings] Failed to apply preset: {ex.Message}");
            }
        }

        // Get current preset name and label
        public static string GetCurrentPresetName() => currentPreset?.Value ?? "live-like";
        public static string GetCurrentPresetLabel() =>
            PresetList.FirstOrDefault(p => p.Name == GetCurrentPresetName())?.Label ?? GetCurrentPresetName();

        // Apply preset from a sync packet
        public static void ApplyPresetFromPacket(PresetSyncPacket packet)
        {
            if (packet == null || string.IsNullOrEmpty(packet.PresetName))
            {
                Log.LogWarning("[Settings] Received empty preset packet.");
                return;
            }

            var preset = PresetList.FirstOrDefault(p => p.Name == packet.PresetName);
            if (preset == null)
            {
                Log.LogWarning($"[Settings] No match for preset: {packet.PresetName}, using fallback.");
                preset = new Preset { Name = "live-like", Label = "Live Like" };
            }

            Log.LogInfo($"[Settings] Applying preset from sync: {preset.Name}");
            _applyingPreset = true;
            try
            {
                currentPreset.Value = preset.Name;
                ApplyPresetSettings(preset);
            }
            finally
            {
                _applyingPreset = false;
            }
        }

        // Manual announcement of the current preset
        public static void AnnounceManually()
        {
            if (IsFika && FikaBackendUtils.IsHeadless)
                return;

            var selected = PresetList.FirstOrDefault(p => p.Name == currentPreset.Value);
            Methods.DisplayMessage(
                selected != null ? $"Current preset: {selected.Label}" : "Unknown preset selected",
                selected != null ? ENotificationIconType.Quest : ENotificationIconType.Alert);
        }
        // Add the missing method OnStartingPmcsChanged
        private static void OnStartingPmcsChanged(object sender, EventArgs e)
        {
            // Implement the logic for handling the change in starting PMCs setting
            Log.LogInfo("[Settings] Starting PMCs setting changed.");
            // Add any additional logic needed for handling the change
        }
    }
}
