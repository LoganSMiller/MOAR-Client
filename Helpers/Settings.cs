using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Communications;
using Newtonsoft.Json;
using UnityEngine;

namespace MOAR.Helpers
{
    internal static class Settings
    {
        private static ConfigFile _config;

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

        public static void Init(ConfigFile config)
        {
            _config = config;
            Log = Plugin.LogSource;
            IsFika = Chainloader.PluginInfos.ContainsKey("com.fika.core");

            ServerStoredDefaults = Routers.GetDefaultConfig();
            PresetList = Routers.GetPresetsList();
            ServerStoredValues = Routers.GetServerConfigWithOverrides();

            var fallbackPresetName = config.Bind("1. Main Settings", "Moar Preset Fallback", "live-like").Value?.Trim().ToLowerInvariant();
            var liveLabel = Routers.GetCurrentPresetLabel()?.Trim().ToLowerInvariant();

            var selectedPreset = PresetList.FirstOrDefault(p =>
                    p.Label?.Trim().ToLowerInvariant() == liveLabel ||
                    p.Name?.Trim().ToLowerInvariant() == liveLabel)
                ?? PresetList.FirstOrDefault(p => p.Name?.Trim().ToLowerInvariant() == fallbackPresetName)
                ?? PresetList.FirstOrDefault();

            currentPreset = config.Bind("1. Main Settings", "Moar Preset",
                selectedPreset?.Name ?? "live-like",
                new ConfigDescription("Preset to apply.", new AcceptableValueList<string>(PresetList.Select(p => p.Name).ToArray())));

            ShowPresetOnRaidStart = config.Bind("1. Main Settings", "Preset Announce On/Off", true);
            AnnounceKey = config.Bind("1. Main Settings", "Announce Key", new KeyboardShortcut(KeyCode.End));
            factionAggression = config.Bind("1. Main Settings", "Faction Based Aggression On/Off", false);

            startingPmcs = config.Bind("1. Main Settings", "Starting PMCS On/Off", ServerStoredDefaults.startingPmcs);
            spawnSmoothing = config.Bind("1. Main Settings", "Spawn Smoothing On/Off", ServerStoredDefaults.spawnSmoothing);
            randomSpawns = config.Bind("1. Main Settings", "Random Spawns On/Off", ServerStoredDefaults.randomSpawns);

            pmcDifficulty = config.Bind("1. Main Settings", "PMC Difficulty", ServerStoredDefaults.pmcDifficulty);
            scavDifficulty = config.Bind("1. Main Settings", "Scav Difficulty", ServerStoredDefaults.scavDifficulty);

            maxBotCap = config.Bind("2. Custom Game Settings", "Max Bot Cap", ServerStoredDefaults.maxBotCap);
            maxBotPerZone = config.Bind("2. Custom Game Settings", "Max Bot Per Zone", ServerStoredDefaults.maxBotPerZone);

            scavGroupChance = config.Bind("2. Custom Game Settings", "Scav Group Chance", ServerStoredDefaults.scavGroupChance);
            pmcGroupChance = config.Bind("2. Custom Game Settings", "PMC Group Chance", ServerStoredDefaults.pmcGroupChance);
            sniperGroupChance = config.Bind("2. Custom Game Settings", "Sniper Group Chance", ServerStoredDefaults.sniperGroupChance);

            scavMaxGroupSize = config.Bind("2. Custom Game Settings", "Scav Max Group Size", ServerStoredDefaults.scavMaxGroupSize);
            pmcMaxGroupSize = config.Bind("2. Custom Game Settings", "PMC Max Group Size", ServerStoredDefaults.pmcMaxGroupSize);
            sniperMaxGroupSize = config.Bind("2. Custom Game Settings", "Sniper Max Group Size", ServerStoredDefaults.sniperMaxGroupSize);

            zombiesEnabled = config.Bind("2. Custom Game Settings", "Zombies Enabled", ServerStoredDefaults.zombiesEnabled);
            zombieHealth = config.Bind("2. Custom Game Settings", "Zombie Health", ServerStoredDefaults.zombieHealth);
            zombieWaveQuantity = config.Bind("2. Custom Game Settings", "Zombie Wave Quantity", ServerStoredDefaults.zombieWaveQuantity);
            zombieWaveDistribution = config.Bind("2. Custom Game Settings", "Zombie Wave Distribution", ServerStoredDefaults.zombieWaveDistribution);

            scavWaveQuantity = config.Bind("2. Custom Game Settings", "Scav Wave Quantity", ServerStoredDefaults.scavWaveQuantity);
            scavWaveDistribution = config.Bind("2. Custom Game Settings", "Scav Wave Distribution", ServerStoredDefaults.scavWaveDistribution);
            pmcWaveQuantity = config.Bind("2. Custom Game Settings", "PMC Wave Quantity", ServerStoredDefaults.pmcWaveQuantity);
            pmcWaveDistribution = config.Bind("2. Custom Game Settings", "PMC Wave Distribution", ServerStoredDefaults.pmcWaveDistribution);

            debug = config.Bind("3. Debug", "Enable Debug Mode", ServerStoredDefaults.debug);
            AddBotSpawn = config.Bind("4. Advanced", "Add Bot Spawn", default(KeyboardShortcut));
            AddSniperSpawn = config.Bind("4. Advanced", "Add Sniper Spawn", default(KeyboardShortcut));
            DeleteBotSpawn = config.Bind("4. Advanced", "Delete Bot Spawn", default(KeyboardShortcut));
            AddPlayerSpawn = config.Bind("4. Advanced", "Add Player Spawn", default(KeyboardShortcut));
            enablePointOverlay = config.Bind("4. Advanced", "Enable Point Overlay", false);

            currentPreset.SettingChanged += (_, _) => OnPresetChange();
            spawnSmoothing.SettingChanged += (_, _) => OnStartingPmcsChanged();
            randomSpawns.SettingChanged += (_, _) => OnStartingPmcsChanged();
            startingPmcs.SettingChanged += (_, _) => OnStartingPmcsChanged();

            if (!IsFika && ShowPresetOnRaidStart.Value && selectedPreset != null)
                Methods.DisplayMessage($"Live preset: {selectedPreset.Label}", ENotificationIconType.Quest);
        }

        private static void OnPresetChange()
        {
            var selected = PresetList.FirstOrDefault(p => p.Name == currentPreset.Value);
            if (selected != null)
            {
                Methods.DisplayMessage($"Current preset: {selected.Label ?? selected.Name}", ENotificationIconType.Quest);
                ApplyPresetSettings(selected);
                Routers.SetPreset(selected.Name);
            }
            else
            {
                Methods.DisplayMessage("Unknown preset selected", ENotificationIconType.Alert);
            }
        }

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

                TrySet<bool>("randomSpawns", (bool v) => randomSpawns.Value = v);
                TrySet<bool>("spawnSmoothing", (bool v) => spawnSmoothing.Value = v);
                TrySet<bool>("startingPmcs", (bool v) => startingPmcs.Value = v);

                TrySet<double>("scavGroupChance", (double v) => scavGroupChance.Value = v);
                TrySet<int>("scavMaxGroupSize", (int v) => scavMaxGroupSize.Value = v);
                TrySet<double>("pmcGroupChance", (double v) => pmcGroupChance.Value = v);
                TrySet<int>("pmcMaxGroupSize", (int v) => pmcMaxGroupSize.Value = v);
            }
            catch (Exception ex)
            {
                Log.LogError($"[ApplyPresetSettings] Failed: {ex.Message}");
            }
        }


        private static void OnStartingPmcsChanged()
        {
            if (startingPmcs.Value)
            {
                randomSpawns.Value = true;
                spawnSmoothing.Value = false;
            }
        }

        public static void AnnounceManually()
        {
            if (IsFika) return;
            var selected = PresetList.FirstOrDefault(p => p.Name == currentPreset.Value);
            Methods.DisplayMessage(selected != null ? $"Current preset: {selected.Label}" : "Unknown preset selected", ENotificationIconType.Quest);
        }
    }
}
