using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using EFT.Communications;
using UnityEngine;
using Fika.Core.Coop.Utils;

namespace MOAR.Helpers
{
    internal static class Settings
    {
        private static ConfigFile _config;
        private static bool _applyingPreset = false;

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

        public static bool IsFika { get; private set; }
        public static ManualLogSource Log { get; private set; }

        public static void Init(ConfigFile config)
        {
            _config = config;
            Log = Plugin.LogSource;
            IsFika = Chainloader.PluginInfos.ContainsKey("com.fika.core");

            string[] availablePresets = Routers.GetPresetsList()?.Select(p => p.Name).ToArray()
                                        ?? new[] { "live-like" };

            currentPreset = config.Bind("1. Main Settings", "Moar Preset", "live-like",
                new ConfigDescription("Preset to apply.", new AcceptableValueList<string>(availablePresets)));

            ShowPresetOnRaidStart = config.Bind("1. Main Settings", "Preset Announce On/Off", true);
            AnnounceKey = config.Bind("1. Main Settings", "Announce Key", new KeyboardShortcut(KeyCode.End));
            factionAggression = config.Bind("1. Main Settings", "Faction Based Aggression On/Off", false);

            DeleteBotSpawn = config.Bind("Bot Spawn Settings", "Delete Bot Spawn Key", new KeyboardShortcut(KeyCode.Delete));
            AddBotSpawn = config.Bind("Bot Spawn Settings", "Add Bot Spawn Key", new KeyboardShortcut(KeyCode.Insert));
            AddSniperSpawn = config.Bind("Bot Spawn Settings", "Add Sniper Spawn Key", new KeyboardShortcut(KeyCode.F1));
            AddPlayerSpawn = config.Bind("Bot Spawn Settings", "Add Player Spawn Key", new KeyboardShortcut(KeyCode.F2));

            debug = config.Bind("1. Main Settings", "Debug Mode", false);
            enablePointOverlay = config.Bind("1. Main Settings", "Enable Point Overlay", true);

            startingPmcs = config.Bind("1. Main Settings", "Starting PMCs On/Off", false);
            spawnSmoothing = config.Bind("1. Main Settings", "Spawn Smoothing On/Off", true);
            randomSpawns = config.Bind("1. Main Settings", "Random Spawns On/Off", false);
            pmcDifficulty = config.Bind("1. Main Settings", "PMC Difficulty", 0.6);
            scavDifficulty = config.Bind("1. Main Settings", "Scav Difficulty", 0.4);

            // Reactive behavior
            currentPreset.SettingChanged += (_, _) => OnPresetChange();
            startingPmcs.SettingChanged += (_, _) => OnStartingPmcsChanged();
            spawnSmoothing.SettingChanged += (_, _) => OnStartingPmcsChanged();
            randomSpawns.SettingChanged += (_, _) => OnStartingPmcsChanged();

            if (ShowPresetOnRaidStart.Value && (!IsFika || FikaBackendUtils.IsServer))
            {
                Methods.DisplayMessage($"Live preset: {currentPreset.Value}", ENotificationIconType.Quest);
            }

            Log.LogInfo($"[Settings] Initialization complete. Selected preset: {currentPreset.Value}");
        }

        private static void OnStartingPmcsChanged()
        {
            if (startingPmcs.Value)
            {
                randomSpawns.Value = true;
                spawnSmoothing.Value = false;
            }
        }

        private static void OnPresetChange()
        {
            if (_applyingPreset)
                return;

            _applyingPreset = true;

            try
            {
                Methods.DisplayMessage($"Current preset: {currentPreset.Value}", ENotificationIconType.Quest);
            }
            finally
            {
                _applyingPreset = false;
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

    public static class ConfigEntryExtensions
    {
        public static bool BetterIsDown(this KeyboardShortcut shortcut) => shortcut.IsDown();
    }
}
