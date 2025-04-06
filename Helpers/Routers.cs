using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Newtonsoft.Json;
using SPT.Common.Http;
using MOAR.Helpers;
using MOAR.Packets;
using EFT;
using Comfort.Common;
using UnityEngine;

namespace MOAR.Helpers
{
    /// <summary>
    /// Provides all client-side routing logic for HTTP and server interaction.
    /// Includes spawn tooling, preset management, and local fallback behavior.
    /// </summary>
    internal static class Routers
    {
        private static string _hostPresetLabel = "Unknown";
        private static bool _initialized;

        public static void Init(ConfigFile config)
        {
            if (_initialized)
            {
                Plugin.LogSource.LogDebug("[Routers] Init skipped — already initialized.");
                return;
            }

            _initialized = true;
            Plugin.LogSource.LogInfo("[Routers] Initialization complete.");
        }

        public static string GetCurrentPresetLabel()
        {
            try
            {
                Plugin.LogSource.LogDebug("[Routers] Requesting /moar/currentPreset...");
                return RequestHandler.GetJson("/moar/currentPreset")?.Trim() ?? "live-like";
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[Routers] [GetCurrentPresetLabel] Fallback: {ex.Message}");
                return Settings.currentPreset?.Value
                    ?? Settings.ServerStoredDefaults?.Label
                    ?? "live-like";
            }
        }

        public static string GetAnnouncePresetLabel()
        {
            try
            {
                Plugin.LogSource.LogDebug("[Routers] Requesting /moar/announcePreset...");
                return RequestHandler.GetJson("/moar/announcePreset")?.Trim() ?? "live-like";
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[Routers] [GetAnnouncePresetLabel] Fallback: {ex.Message}");
                return Settings.currentPreset?.Value
                    ?? Settings.ServerStoredDefaults?.Label
                    ?? "live-like";
            }
        }

        public static string GetCurrentPresetName()
        {
            var label = GetCurrentPresetLabel();
            var preset = FindPresetByLabel(label);
            return preset?.Name ?? label ?? "Unknown";
        }

        public static string GetAnnouncePresetName() => _hostPresetLabel;

        public static void SetHostPresetLabel(string label)
        {
            _hostPresetLabel = label ?? "Unknown";
            Plugin.LogSource.LogInfo($"[Routers] Host preset label set: {_hostPresetLabel}");
        }

        private static Preset? FindPresetByLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                Plugin.LogSource.LogWarning("[Routers] FindPresetByLabel received null or empty label.");
                return null;
            }

            if (Settings.PresetList == null)
            {
                Plugin.LogSource.LogError("[Routers] PresetList is null. Cannot match preset.");
                return null;
            }

            var preset = Settings.PresetList.FirstOrDefault(p =>
                string.Equals(p.Label?.Trim(), label, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name?.Trim(), label, StringComparison.OrdinalIgnoreCase));

            if (preset == null)
                Plugin.LogSource.LogWarning($"[Routers] No preset matched label: {label}");

            return preset;
        }

        public static string SetPreset(string label)
        {
            try
            {
                Plugin.LogSource.LogInfo($"[Routers] Sending SetPreset request: {label}");
                var request = new SetPresetRequest { Preset = label };
                var json = JsonConvert.SerializeObject(request);
                return RequestHandler.PostJson("/moar/setPreset", json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Routers] [SetPreset] Failed: {ex.Message}");
                return "Failed to set preset.";
            }
        }

        public static List<Preset> GetPresetsList()
        {
            try
            {
                Plugin.LogSource.LogInfo("[Routers] Requesting /moar/getPresets...");
                var json = RequestHandler.GetJson("/moar/getPresets");
                var response = JsonConvert.DeserializeObject<GetPresetsListResponse>(json);
                return response?.Data?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Routers] [GetPresetsList] Failed: {ex.Message}");
                return new();
            }
        }

        public static ConfigSettings GetServerConfigWithOverrides()
        {
            try
            {
                Plugin.LogSource.LogInfo("[Routers] Requesting /moar/getServerConfigWithOverrides...");
                var json = RequestHandler.GetJson("/moar/getServerConfigWithOverrides");
                return JsonConvert.DeserializeObject<ConfigSettings>(json) ?? new();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Routers] [GetServerConfigWithOverrides] Failed: {ex.Message}");
                return new();
            }
        }

        public static ConfigSettings GetDefaultConfig()
        {
            try
            {
                Plugin.LogSource.LogInfo("[Routers] Requesting /moar/getDefaultConfig...");
                var json = RequestHandler.GetJson("/moar/getDefaultConfig");
                return JsonConvert.DeserializeObject<ConfigSettings>(json) ?? new();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Routers] [GetDefaultConfig] Failed: {ex.Message}");
                return new();
            }
        }

        public static void SetOverrideConfig(ConfigSettings settings)
        {
            try
            {
                Plugin.LogSource.LogInfo("[Routers] Posting override config...");
                var json = JsonConvert.SerializeObject(settings);
                RequestHandler.PostJson("/moar/setOverrideConfig", json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Routers] [SetOverrideConfig] Failed: {ex.Message}");
            }
        }

        public static string AddBotSpawn() => PostPlayerLocationTo("/moar/addBotSpawn");
        public static string AddSniperSpawn() => PostPlayerLocationTo("/moar/addSniperSpawn");
        public static string DeleteBotSpawn() => PostPlayerLocationTo("/moar/deleteBotSpawn");
        public static string AddPlayerSpawn() => PostPlayerLocationTo("/moar/addPlayerSpawn");

        private static string PostPlayerLocationTo(string endpoint)
        {
            try
            {
                Plugin.LogSource.LogInfo($"[Routers] Posting player position to: {endpoint}");
                var request = Methods.GetPlayersCoordinatesAndLevel();
                var json = JsonConvert.SerializeObject(request);
                return RequestHandler.PostJson(endpoint, json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[Routers] [PostPlayerLocationTo] Failed on {endpoint}: {ex.Message}");
                return "Error submitting player position.";
            }
        }
    }
}
