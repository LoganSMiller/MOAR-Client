using System;
using System.Linq;
using System.Reflection;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace MOAR.Helpers
{
    internal class Settings
    {
        private static ConfigFile _config;

        public static ConfigEntry<bool> ShowPresetOnRaidStart;
        public static bool IsFika = false;
        public static ConfigSettings ServerStoredValues;
        public static ConfigSettings ServerStoredDefaults;

        public static ConfigEntry<double> pmcDifficulty;
        public static ConfigEntry<double> scavDifficulty;
        public static ConfigEntry<double> defaultScavStartWaveRatio;

        public static ConfigEntry<double> defaultScavWaveMultiplier;

        // public static ConfigEntry<bool> startingPmcs;
        public static ConfigEntry<double> defaultPmcStartWaveRatio;
        public static ConfigEntry<double> defaultPmcWaveMultiplier;

        // ============================================================


        public static ConfigEntry<int> defaultMaxBotCap;
        public static ConfigEntry<int> defaultMaxBotPerZone;
        public static ConfigEntry<bool> moreScavGroups;
        public static ConfigEntry<bool> morePmcGroups;
        public static ConfigEntry<int> defaultGroupMaxPMC;
        public static ConfigEntry<int> defaultGroupMaxScav;
        public static ConfigEntry<bool> sniperBuddies;
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

        // ============================================================

        public static Preset[] PresetList;
        public static double LastUpdatedServer = 0;
        public static ConfigEntry<string> currentPreset;

        public static ManualLogSource Log;

        public static void Init(ConfigFile Config)
        {
            _config = Config;
            Log = Plugin.LogSource;
            IsFika = Chainloader.PluginInfos.ContainsKey("com.fika.core");
            ServerStoredDefaults = Routers.GetDefaultConfig();
            PresetList = Routers.GetPresetsList();
            UpdateServerStoredValues();

            // Main SETTINGS =====================================

            pmcDifficulty = Config.Bind(
                "1. Main Settings",
                "Pmc difficulty",
                ServerStoredDefaults.pmcDifficulty,
                new ConfigDescription(
                    "Randomly rolls for easy/medium/hard/impossible",
                    new AcceptableValueRange<double>(0, 1.5),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = true,
                        DefaultValue = ServerStoredDefaults.pmcDifficulty,
                    }
                )
            );

            scavDifficulty = Config.Bind(
                "1. Main Settings",
                "Scav difficulty",
                ServerStoredDefaults.scavDifficulty,
                new ConfigDescription(
                    "Randomly rolls for easy/medium/hard/impossible",
                    new AcceptableValueRange<double>(0, 1.5),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = true,
                        DefaultValue = ServerStoredDefaults.scavDifficulty,
                        Order = 96,
                    }
                )
            );

            currentPreset = Config.Bind(
                "1. Main Settings",
                "Moar Preset",
                "Random",
                new ConfigDescription(
                    "Preset to be used, default pulls a random weighted preset from the config.",
                    new AcceptableValueList<string>(PresetList.Select(item => item.Name).ToArray()),
                    new ConfigurationManagerAttributes
                    {
                        DefaultValue = "Random",
                        ShowRangeAsPercent = false,
                        Order = 98,
                    }
                )
            );

            ShowPresetOnRaidStart = Config.Bind(
                "1. Main Settings",
                "Preset Announce On/Off",
                true,
                new ConfigDescription(
                    "Enable/Disable preset announce on raid start",
                    null,
                    new ConfigurationManagerAttributes { IsAdvanced = false, Order = 99 }
                )
            );

            if (IsFika)
            {
                CreateSimpleButton(
                    "1. Main Settings",
                    "FIKA DETECTED: ALWAYS PRESS THIS FIRST BEFORE MAKING CHANGES!!",
                    "Pull settings from server",
                    "Pulls all server settings from server",
                    () =>
                    {
                        UpdateServerStoredValues();
                        UpdateValuesFromServerStoredValues();
                        Methods.DisplayMessage("Pulled latest settings from servers");
                        return "";
                    },
                    100
                );
            }

            // Main SETTINGS =====================================

            CreateSimpleButton(
                "2. Custom game Settings",
                "Reset",
                "Reset all settings to defaults",
                "Resets all settings to defaults",
                () =>
                {
                    currentPreset.Value = "Random";
                    UpdateValuesFromDefaults(true);
                    Routers.SetOverrideConfig(ServerStoredDefaults);
                    Methods.DisplayMessage("Reset all settings");
                    return "";
                },
                95
            );

            CreateSimpleButton(
                "2. Custom game Settings",
                "Save the above changes",
                "Save",
                "Pushes settings to server",
                () =>
                {
                    if (!CustomUnchanged())
                    {
                        currentPreset.Value = "Custom";
                    }
                    OverwriteServerStoredValuesAndSubmit();
                    Methods.DisplayMessage("Pushed latest settings to servers");
                    return "";
                },
                96
            );

            defaultScavStartWaveRatio = Config.Bind(
                "2. Custom game Settings",
                "ScavStartWaveRatio",
                ServerStoredDefaults.defaultScavStartWaveRatio,
                new ConfigDescription(
                    "Determines the weighting of spawns at the beginning (0) spread evenly throughout (0.5) or at the end(1) of the raid",
                    new AcceptableValueRange<double>(0, 1),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.defaultScavStartWaveRatio,
                        Order = 98,
                    }
                )
            );

            defaultPmcStartWaveRatio = Config.Bind(
                "2. Custom game Settings",
                "PmcStartWaveRatio",
                ServerStoredDefaults.defaultPmcStartWaveRatio,
                new ConfigDescription(
                    "Determines the weighting of spawns at the beginning (0) spread evenly throughout (0.5) or at the end(1) of the raid",
                    new AcceptableValueRange<double>(0, 1),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.defaultPmcStartWaveRatio,
                        Order = 97,
                    }
                )
            );

            defaultScavWaveMultiplier = Config.Bind(
                "2. Custom game Settings",
                "ScavWaveMultiplier",
                ServerStoredDefaults.defaultScavWaveMultiplier,
                new ConfigDescription(
                    "Multiplies base waves by this number",
                    new AcceptableValueRange<double>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.defaultScavWaveMultiplier,
                        Order = 100,
                    }
                )
            );

            defaultPmcWaveMultiplier = Config.Bind(
                "2. Custom game Settings",
                "PmcWaveMultiplier",
                ServerStoredDefaults.defaultPmcWaveMultiplier,
                new ConfigDescription(
                    "Multiplies base waves by this number",
                    new AcceptableValueRange<double>(0, 10),
                    new ConfigurationManagerAttributes
                    {
                        IsAdvanced = false,
                        ShowRangeAsPercent = false,
                        DefaultValue = ServerStoredDefaults.defaultPmcWaveMultiplier,
                        Order = 99,
                    }
                )
            );

            currentPreset.SettingChanged += OnPresetChange;
            if (IsFika)
            {
                currentPreset.Value = Routers.GetCurrentPresetName();
                UpdateServerStoredValues();
                UpdateValuesFromServerStoredValues();
            }
            else
            {
                if (!CustomUnchanged())
                {
                    currentPreset.Value = "Custom";
                }
                OverwriteServerStoredValuesAndSubmit();
                Methods.DisplayMessage("Pushed latest settings to servers");
            }
        }

        private static void OverwriteServerStoredValuesAndSubmit()
        {
            if (currentPreset.Value == "Custom")
            {
                ServerStoredValues.scavDifficulty = Math.Round(scavDifficulty.Value, 2);
                ServerStoredValues.pmcDifficulty = Math.Round(pmcDifficulty.Value, 2);

                ServerStoredValues.defaultScavStartWaveRatio = Math.Round(
                    defaultScavStartWaveRatio.Value,
                    1
                );

                ServerStoredValues.defaultPmcStartWaveRatio = Math.Round(
                    defaultPmcStartWaveRatio.Value,
                    1
                );
                ServerStoredValues.defaultPmcWaveMultiplier = Math.Round(
                    defaultPmcWaveMultiplier.Value,
                    1
                );
                ServerStoredValues.defaultScavWaveMultiplier = Math.Round(
                    defaultScavWaveMultiplier.Value,
                    1
                );

                scavDifficulty.Value = ServerStoredValues.scavDifficulty;
                pmcDifficulty.Value = ServerStoredValues.pmcDifficulty;
                defaultScavStartWaveRatio.Value = ServerStoredValues.defaultScavStartWaveRatio;
                defaultPmcStartWaveRatio.Value = ServerStoredValues.defaultPmcStartWaveRatio;
                defaultScavWaveMultiplier.Value = ServerStoredValues.defaultScavWaveMultiplier;
                defaultPmcWaveMultiplier.Value = ServerStoredValues.defaultPmcWaveMultiplier;
                defaultMaxBotCap.Value = ServerStoredValues.defaultMaxBotCap;
                defaultMaxBotPerZone.Value = ServerStoredValues.defaultMaxBotPerZone;
                moreScavGroups.Value = ServerStoredValues.moreScavGroups;
                morePmcGroups.Value = ServerStoredValues.morePmcGroups;
                defaultGroupMaxPMC.Value = ServerStoredValues.defaultGroupMaxPMC;
                defaultGroupMaxScav.Value = ServerStoredValues.defaultGroupMaxScav;
                sniperBuddies.Value = ServerStoredValues.sniperBuddies;
                bossOpenZones.Value = ServerStoredValues.bossOpenZones;
                randomRaiderGroup.Value = ServerStoredValues.randomRaiderGroup;
                randomRaiderGroupChance.Value = ServerStoredValues.randomRaiderGroupChance;
                randomRogueGroup.Value = ServerStoredValues.randomRogueGroup;
                randomRogueGroupChance.Value = ServerStoredValues.randomRogueGroupChance;
                disableBosses.Value = ServerStoredValues.disableBosses;
                mainBossChanceBuff.Value = ServerStoredValues.mainBossChanceBuff;
                bossInvasion.Value = ServerStoredValues.bossInvasion;
                bossInvasionSpawnChance.Value = ServerStoredValues.bossInvasionSpawnChance;
                gradualBossInvasion.Value = ServerStoredValues.gradualBossInvasion;
            }
            else
            {
                UpdateValuesFromDefaults(false);

                ServerStoredValues.scavDifficulty = Math.Round(scavDifficulty.Value, 2);
                ServerStoredValues.pmcDifficulty = Math.Round(pmcDifficulty.Value, 2);
            }

            Routers.SetOverrideConfig(ServerStoredValues);
        }

        private static bool CustomUnchanged()
        {
            return defaultScavStartWaveRatio.Value == ServerStoredValues.defaultScavStartWaveRatio
                && defaultPmcStartWaveRatio.Value == ServerStoredValues.defaultPmcStartWaveRatio
                && defaultScavWaveMultiplier.Value == ServerStoredValues.defaultScavWaveMultiplier
                && defaultPmcWaveMultiplier.Value == ServerStoredValues.defaultPmcWaveMultiplier
                && defaultMaxBotCap.Value == ServerStoredValues.defaultMaxBotCap
                && defaultMaxBotPerZone.Value == ServerStoredValues.defaultMaxBotPerZone
                && moreScavGroups.Value == ServerStoredValues.moreScavGroups
                && morePmcGroups.Value == ServerStoredValues.morePmcGroups
                && defaultGroupMaxPMC.Value == ServerStoredValues.defaultGroupMaxPMC
                && defaultGroupMaxScav.Value == ServerStoredValues.defaultGroupMaxScav
                && sniperBuddies.Value == ServerStoredValues.sniperBuddies
                && bossOpenZones.Value == ServerStoredValues.bossOpenZones
                && randomRaiderGroup.Value == ServerStoredValues.randomRaiderGroup
                && randomRaiderGroupChance.Value == ServerStoredValues.randomRaiderGroupChance
                && randomRogueGroup.Value == ServerStoredValues.randomRogueGroup
                && randomRogueGroupChance.Value == ServerStoredValues.randomRogueGroupChance
                && disableBosses.Value == ServerStoredValues.disableBosses
                && mainBossChanceBuff.Value == ServerStoredValues.mainBossChanceBuff
                && bossInvasion.Value == ServerStoredValues.bossInvasion
                && bossInvasionSpawnChance.Value == ServerStoredValues.bossInvasionSpawnChance
                && gradualBossInvasion.Value == ServerStoredValues.gradualBossInvasion;
        }

        private static void UpdateValuesFromDefaults(bool updateDifficulty = false)
        {
            if (updateDifficulty)
            {
                scavDifficulty.Value = ServerStoredDefaults.scavDifficulty;
                pmcDifficulty.Value = ServerStoredDefaults.pmcDifficulty;
            }

            defaultScavStartWaveRatio.Value = ServerStoredDefaults.defaultScavStartWaveRatio;
            defaultPmcStartWaveRatio.Value = ServerStoredDefaults.defaultPmcStartWaveRatio;
            defaultScavWaveMultiplier.Value = ServerStoredDefaults.defaultScavWaveMultiplier;
            defaultPmcWaveMultiplier.Value = ServerStoredDefaults.defaultPmcWaveMultiplier;
            defaultMaxBotCap.Value = ServerStoredDefaults.defaultMaxBotCap;
            defaultMaxBotPerZone.Value = ServerStoredDefaults.defaultMaxBotPerZone;
            moreScavGroups.Value = ServerStoredDefaults.moreScavGroups;
            morePmcGroups.Value = ServerStoredDefaults.morePmcGroups;
            defaultGroupMaxPMC.Value = ServerStoredDefaults.defaultGroupMaxPMC;
            defaultGroupMaxScav.Value = ServerStoredDefaults.defaultGroupMaxScav;
            sniperBuddies.Value = ServerStoredDefaults.sniperBuddies;
            bossOpenZones.Value = ServerStoredDefaults.bossOpenZones;
            randomRaiderGroup.Value = ServerStoredDefaults.randomRaiderGroup;
            randomRaiderGroupChance.Value = ServerStoredDefaults.randomRaiderGroupChance;
            randomRogueGroup.Value = ServerStoredDefaults.randomRogueGroup;
            randomRogueGroupChance.Value = ServerStoredDefaults.randomRogueGroupChance;
            disableBosses.Value = ServerStoredDefaults.disableBosses;
            mainBossChanceBuff.Value = ServerStoredDefaults.mainBossChanceBuff;
            bossInvasion.Value = ServerStoredDefaults.bossInvasion;
            bossInvasionSpawnChance.Value = ServerStoredDefaults.bossInvasionSpawnChance;
            gradualBossInvasion.Value = ServerStoredDefaults.gradualBossInvasion;
            ServerStoredValues = Routers.GetDefaultConfig();
        }

        private static void UpdateValuesFromServerStoredValues()
        {
            currentPreset.Value = Routers.GetCurrentPresetName();
            scavDifficulty.Value = ServerStoredValues.scavDifficulty;
            pmcDifficulty.Value = ServerStoredValues.pmcDifficulty;
            defaultScavStartWaveRatio.Value = ServerStoredValues.defaultScavStartWaveRatio;
            defaultPmcStartWaveRatio.Value = ServerStoredValues.defaultPmcStartWaveRatio;
            defaultScavWaveMultiplier.Value = ServerStoredValues.defaultScavWaveMultiplier;
            defaultPmcWaveMultiplier.Value = ServerStoredValues.defaultPmcWaveMultiplier;
            defaultMaxBotCap.Value = ServerStoredValues.defaultMaxBotCap;
            defaultMaxBotPerZone.Value = ServerStoredValues.defaultMaxBotPerZone;
            moreScavGroups.Value = ServerStoredValues.moreScavGroups;
            morePmcGroups.Value = ServerStoredValues.morePmcGroups;
            defaultGroupMaxPMC.Value = ServerStoredValues.defaultGroupMaxPMC;
            defaultGroupMaxScav.Value = ServerStoredValues.defaultGroupMaxScav;
            sniperBuddies.Value = ServerStoredValues.sniperBuddies;
            bossOpenZones.Value = ServerStoredValues.bossOpenZones;
            randomRaiderGroup.Value = ServerStoredValues.randomRaiderGroup;
            randomRaiderGroupChance.Value = ServerStoredValues.randomRaiderGroupChance;
            randomRogueGroup.Value = ServerStoredValues.randomRogueGroup;
            randomRogueGroupChance.Value = ServerStoredValues.randomRogueGroupChance;
            disableBosses.Value = ServerStoredValues.disableBosses;
            mainBossChanceBuff.Value = ServerStoredValues.mainBossChanceBuff;
            bossInvasion.Value = ServerStoredValues.bossInvasion;
            bossInvasionSpawnChance.Value = ServerStoredValues.bossInvasionSpawnChance;
            gradualBossInvasion.Value = ServerStoredValues.gradualBossInvasion;
        }

        private static void OnPresetChange(object sender, EventArgs e)
        {
            var current = Array.Find(PresetList, (item) => item.Name == currentPreset.Value).Label;
            var result = Routers.SetPreset(current);
            Methods.DisplayMessage("Updated, preset set to: " + result);
            if (current != "custom")
            {
                OverwriteServerStoredValuesAndSubmit();
                UpdateServerStoredValues();
            }
        }

        private static void UpdateServerStoredValues()
        {
            ServerStoredValues = Routers.GetServerConfigWithOverrides();
        }

        private static void CreateSimpleButton(
            string configSection,
            string configEntryName,
            string buttonName,
            string description,
            Func<string> doThings,
            int? order
        )
        {
            Action<ConfigEntryBase> drawer = (ConfigEntryBase entry) =>
            {
                if (GUILayout.Button(buttonName, GUILayout.ExpandWidth(true)))
                {
                    doThings();
                }
            };

            ConfigDescription configDescription =
                new(
                    description,
                    null,
                    new ConfigurationManagerAttributes { Order = order, CustomDrawer = drawer }
                );

            _config.Bind(configSection, configEntryName, "", configDescription);
        }
    }
}
