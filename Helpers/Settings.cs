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

            var fallbackPresetName = _config.Bind("1. Main Settings", "Moar Preset Fallback", "live-like").Value?.Trim().ToLowerInvariant();
            var liveLabel = Routers.GetCurrentPresetLabel()?.Trim().ToLowerInvariant();

            var selectedPreset = PresetList.FirstOrDefault(p =>
                    p.Label?.Trim().ToLowerInvariant() == liveLabel ||
                    p.Name?.Trim().ToLowerInvariant() == liveLabel)
                ?? PresetList.FirstOrDefault(p => p.Name?.Trim().ToLowerInvariant() == fallbackPresetName)
                ?? PresetList.FirstOrDefault();

            currentPreset = _config.Bind("1. Main Settings", "Moar Preset",
                selectedPreset?.Name ?? "live-like",
                new ConfigDescription("Preset to apply.", new AcceptableValueList<string>(PresetList.Select(p => p.Name).ToArray())));

            ShowPresetOnRaidStart = _config.Bind("1. Main Settings", "Preset Announce On/Off", true);
            AnnounceKey = _config.Bind("1. Main Settings", "Announce Key", new KeyboardShortcut(KeyCode.End));
            factionAggression = _config.Bind("1. Main Settings", "Faction Based Aggression On/Off", false);

            startingPmcs = _config.Bind("1. Main Settings", "Starting PMCS On/Off", ServerStoredDefaults.startingPmcs);
            spawnSmoothing = _config.Bind("1. Main Settings", "Spawn Smoothing On/Off", ServerStoredDefaults.spawnSmoothing);
            randomSpawns = _config.Bind("1. Main Settings", "Random Spawns On/Off", ServerStoredDefaults.randomSpawns);

            pmcDifficulty = _config.Bind("1. Main Settings", "PMC Difficulty", ServerStoredDefaults.pmcDifficulty);
            scavDifficulty = _config.Bind("1. Main Settings", "Scav Difficulty", ServerStoredDefaults.scavDifficulty);

            maxBotCap = _config.Bind("2. Custom game Settings", "MaxBotCap", ServerStoredDefaults.maxBotCap);
            maxBotPerZone = _config.Bind("2. Custom game Settings", "MaxBotPerZone", ServerStoredDefaults.maxBotPerZone);
            spawnRadius = _config.Bind("2. Custom game Settings", "SpawnRadius", ServerStoredDefaults.spawnRadius);
            spawnDelay = _config.Bind("2. Custom game Settings", "SpawnDelay", ServerStoredDefaults.spawnDelay);

            scavGroupChance = _config.Bind("2. Custom game Settings", "scavGroupChance", ServerStoredDefaults.scavGroupChance);
            pmcGroupChance = _config.Bind("2. Custom game Settings", "pmcGroupChance", ServerStoredDefaults.pmcGroupChance);
            sniperGroupChance = _config.Bind("2. Custom game Settings", "sniperGroupChance", ServerStoredDefaults.sniperGroupChance);

            pmcMaxGroupSize = _config.Bind("2. Custom game Settings", "pmcMaxGroupSize", ServerStoredDefaults.pmcMaxGroupSize);
            scavMaxGroupSize = _config.Bind("2. Custom game Settings", "scavMaxGroupSize", ServerStoredDefaults.scavMaxGroupSize);
            sniperMaxGroupSize = _config.Bind("2. Custom game Settings", "sniperMaxGroupSize", ServerStoredDefaults.sniperMaxGroupSize);

            zombiesEnabled = _config.Bind("2. Custom game Settings", "zombiesEnabled", ServerStoredDefaults.zombiesEnabled);
            zombieHealth = _config.Bind("2. Custom game Settings", "zombieHealth", ServerStoredDefaults.zombieHealth);
            zombieWaveQuantity = _config.Bind("2. Custom game Settings", "zombieWaveQuantity", ServerStoredDefaults.zombieWaveQuantity);
            zombieWaveDistribution = _config.Bind("2. Custom game Settings", "zombieWaveDistribution", ServerStoredDefaults.zombieWaveDistribution);
            scavWaveQuantity = _config.Bind("2. Custom game Settings", "scavWaveQuantity", ServerStoredDefaults.scavWaveQuantity);
            scavWaveDistribution = _config.Bind("2. Custom game Settings", "scavWaveDistribution", ServerStoredDefaults.scavWaveDistribution);
            pmcWaveQuantity = _config.Bind("2. Custom game Settings", "pmcWaveQuantity", ServerStoredDefaults.pmcWaveQuantity);
            pmcWaveDistribution = _config.Bind("2. Custom game Settings", "pmcWaveDistribution", ServerStoredDefaults.pmcWaveDistribution);

            bossOpenZones = _config.Bind("3. Bosses", "bossOpenZones", ServerStoredDefaults.bossOpenZones);
            disableBosses = _config.Bind("3. Bosses", "disableBosses", ServerStoredDefaults.disableBosses);
            mainBossChanceBuff = _config.Bind("3. Bosses", "mainBossChanceBuff", ServerStoredDefaults.mainBossChanceBuff);
            bossInvasion = _config.Bind("3. Bosses", "bossInvasion", ServerStoredDefaults.bossInvasion);
            bossInvasionSpawnChance = _config.Bind("3. Bosses", "bossInvasionSpawnChance", ServerStoredDefaults.bossInvasionSpawnChance);
            gradualBossInvasion = _config.Bind("3. Bosses", "gradualBossInvasion", ServerStoredDefaults.gradualBossInvasion);
            randomRaiderGroup = _config.Bind("3. Bosses", "randomRaiderGroup", ServerStoredDefaults.randomRaiderGroup);
            randomRaiderGroupChance = _config.Bind("3. Bosses", "randomRaiderGroupChance", ServerStoredDefaults.randomRaiderGroupChance);
            randomRogueGroup = _config.Bind("3. Bosses", "randomRogueGroup", ServerStoredDefaults.randomRogueGroup);
            randomRogueGroupChance = _config.Bind("3. Bosses", "randomRogueGroupChance", ServerStoredDefaults.randomRogueGroupChance);
            enableBossOverrides = _config.Bind("3. Bosses", "enableBossOverrides", ServerStoredDefaults.enableBossOverrides);
            forceHotzonesOnly = _config.Bind("3. Bosses", "forceHotzonesOnly", ServerStoredDefaults.forceHotzonesOnly);

            debug = _config.Bind("4. Debug", "Debug Mode", ServerStoredDefaults.debug);

            AddBotSpawn = _config.Bind("5. Advanced", "Add Bot Spawn", default(KeyboardShortcut));
            AddSniperSpawn = _config.Bind("5. Advanced", "Add Sniper Spawn", default(KeyboardShortcut));
            DeleteBotSpawn = _config.Bind("5. Advanced", "Delete Bot Spawn", default(KeyboardShortcut));
            AddPlayerSpawn = _config.Bind("5. Advanced", "Add Player Spawn", default(KeyboardShortcut));
            enablePointOverlay = _config.Bind("5. Advanced", "Enable Spawnpoint Overlay", false);

            currentPreset.SettingChanged += (_, _) => OnPresetChange();
            spawnSmoothing.SettingChanged += (_, _) => OnStartingPmcsChanged();
            randomSpawns.SettingChanged += (_, _) => OnStartingPmcsChanged();
            startingPmcs.SettingChanged += (_, _) => OnStartingPmcsChanged();

            if (!IsFika && !FikaBackendUtils.IsHeadless && ShowPresetOnRaidStart.Value && selectedPreset != null)
                Methods.DisplayMessage($"Live preset: {selectedPreset.Label}", ENotificationIconType.Quest);
        }

        private static void OnPresetChange()
        {
            var selected = PresetList.FirstOrDefault(p => p.Name == currentPreset.Value);
            if (selected != null)
            {
                ApplyPresetSettings(selected);
                Methods.DisplayMessage($"Current preset: {selected.Label ?? selected.Name}", ENotificationIconType.Quest);
                Routers.SetPreset(selected.Name);
            }
            else
            {
                Methods.DisplayMessage("Unknown preset selected", ENotificationIconType.Alert);
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

        private static void ApplyPresetSettings(Preset preset)
        {
            if (preset?.Settings == null) return;

            try
            {
                var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(preset.Settings));

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
                TrySet("scavGroupChance", (double v) => scavGroupChance.Value = v);
                TrySet("scavMaxGroupSize", (int v) => scavMaxGroupSize.Value = v);
                TrySet("pmcGroupChance", (double v) => pmcGroupChance.Value = v);
                TrySet("pmcMaxGroupSize", (int v) => pmcMaxGroupSize.Value = v);
                TrySet("pmcWaveQuantity", (double v) => pmcWaveQuantity.Value = v);
                TrySet("pmcWaveDistribution", (double v) => pmcWaveDistribution.Value = v);
            }
            catch (Exception ex)
            {
                Log.LogError($"[ApplyPresetSettings] Failed to apply preset: {ex.Message}");
            }
        }

        public static void AnnounceManually()
        {
            if (IsFika && FikaBackendUtils.IsHeadless)
                return;

            var selected = PresetList.FirstOrDefault(p => p.Name == currentPreset.Value);
            if (selected != null)
                Methods.DisplayMessage($"Current preset: {selected.Label}", ENotificationIconType.Quest);
            else
                Methods.DisplayMessage("Unknown preset selected", ENotificationIconType.Alert);
        }
    }
}
