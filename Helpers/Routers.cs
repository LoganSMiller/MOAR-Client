using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using Newtonsoft.Json;
using SPT.Common.Http;
using MOAR.Helpers;
using MOAR.Packets;
using EFT;
using Fika.Core.Networking;
using Fika.Core.Coop.Components;
using Comfort.Common;

namespace MOAR.Helpers
{
    /// <summary>
    /// Handles all client-side server API routing logic for config, presets, and spawn tools.
    /// </summary>
    internal static class Routers
    {
        public static void Init(ConfigFile config) { }

        // ─────────────────────────────────────────────────────────────
        // ─── PRESET ACCESSORS ────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────

        public static string GetCurrentPresetLabel()
        {
            try
            {
                return RequestHandler.GetJson("/moar/currentPreset")?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[GetCurrentPresetLabel] Fallback to client config: {ex.Message}");
                return Settings.currentPreset?.Value ?? Settings.ServerStoredDefaults?.Name ?? "live-like";
            }
        }

        public static string GetAnnouncePresetLabel()
        {
            try
            {
                return RequestHandler.GetJson("/moar/announcePreset")?.Trim() ?? string.Empty;
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[GetAnnouncePresetLabel] Fallback to client config: {ex.Message}");
                return Settings.currentPreset?.Value ?? Settings.ServerStoredDefaults?.Name ?? "live-like";
            }
        }

        public static string GetCurrentPresetName()
        {
            var label = GetCurrentPresetLabel();
            var preset = FindPresetByLabel(label);
            return preset?.Name ?? label ?? "Unknown";
        }

        public static string GetAnnouncePresetName()
        {
            return _hostPresetLabel; // <-- Use stored label instead of resolving it again
        }

        public static void SetHostPresetLabel(string label)
        {
            _hostPresetLabel = label;
        }

        private static Preset FindPresetByLabel(string label)
        {
            return Settings.PresetList?.FirstOrDefault(p =>
                string.Equals(p.Label, label, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name, label, StringComparison.OrdinalIgnoreCase));
        }

        public static string SetPreset(string label)
        {
            try
            {
                var request = new SetPresetRequest { Preset = label };
                var json = JsonConvert.SerializeObject(request);
                return RequestHandler.PostJson("/moar/setPreset", json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[SetPreset] Failed to set preset '{label}': {ex.Message}");
                return "Failed to set preset.";
            }
        }

        public static List<Preset> GetPresetsList()
        {
            try
            {
                var json = RequestHandler.GetJson("/moar/getPresets");
                var response = JsonConvert.DeserializeObject<GetPresetsListResponse>(json);
                return response?.Data?.ToList() ?? new List<Preset>();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetPresetsList] Failed to fetch presets: {ex.Message}");
                return new List<Preset>();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // ─── CONFIG MANAGEMENT ───────────────────────────────────────
        // ─────────────────────────────────────────────────────────────

        public static ConfigSettings GetServerConfigWithOverrides()
        {
            try
            {
                var json = RequestHandler.GetJson("/moar/getServerConfig");
                return JsonConvert.DeserializeObject<ConfigSettings>(json) ?? new ConfigSettings();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetServerConfigWithOverrides] Error: {ex.Message}");
                return new ConfigSettings();
            }
        }

        public static ConfigSettings GetDefaultConfig()
        {
            try
            {
                var json = RequestHandler.GetJson("/moar/getDefaultConfig");
                return JsonConvert.DeserializeObject<ConfigSettings>(json) ?? new ConfigSettings();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetDefaultConfig] Error: {ex.Message}");
                return new ConfigSettings();
            }
        }

        public static void SetOverrideConfig(ConfigSettings settings)
        {
            try
            {
                var json = JsonConvert.SerializeObject(settings);
                RequestHandler.PostJson("/moar/setOverrideConfig", json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[SetOverrideConfig] Failed to push config: {ex.Message}");
            }
        }

        // ─────────────────────────────────────────────────────────────
        // ─── SPAWN TOOLING ───────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────

        public static string AddBotSpawn() => PostPlayerLocationTo("/moar/addBotSpawn");
        public static string AddSniperSpawn() => PostPlayerLocationTo("/moar/addSniperSpawn");
        public static string DeleteBotSpawn() => PostPlayerLocationTo("/moar/deleteBotSpawn");
        public static string AddPlayerSpawn() => PostPlayerLocationTo("/moar/addPlayerSpawn");

        private static string PostPlayerLocationTo(string endpoint)
        {
            try
            {
                var request = Methods.GetPlayersCoordinatesAndLevel();
                var json = JsonConvert.SerializeObject(request);
                return RequestHandler.PostJson(endpoint, json);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[PostPlayerLocationTo] Failed to post to {endpoint}: {ex.Message}");
                return "Error submitting player position.";
            }
        }

        // --- FIKA Sync Debug ---

        public static void HandleFikaPresetSync(string presetLabel, string presetName)
        {
            Plugin.LogSource.LogInfo($"[FIKA Sync] Received preset from FIKA: {presetLabel} ({presetName})");

            var match = Settings.PresetList?.Find(p =>
                string.Equals(p.Name, presetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label, presetLabel, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                Settings.currentPreset.Value = match.Name;
                _hostPresetLabel = match.Label; // <-- Update stored label here
                Plugin.LogSource.LogInfo($"[FIKA Sync] Applied synced preset: {match.Label} ({match.Name})");
            }
            else
            {
                Plugin.LogSource.LogWarning($"[FIKA Sync] No matching preset found for: {presetLabel} / {presetName}");
            }
        }
    }
}
