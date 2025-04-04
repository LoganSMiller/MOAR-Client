using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx.Configuration;
using Newtonsoft.Json;
using SPT.Common.Http;
using MOAR.Helpers;
using MOAR.Packets;
using EFT;
using Comfort.Common;
using Fika.Core.Networking;
using Fika.Core;
using UnityEngine;
using MOAR.Networking;

namespace MOAR.Helpers
{
    /// <summary>
    /// Provides all client-side routing logic for HTTP and FIKA-based config/preset synchronization.
    /// Includes spawn tooling, preset management, and headless-safe fallback behavior.
    /// </summary>
    internal static class Routers
    {
        private static string _hostPresetLabel = "Unknown";
        private static bool _initialized;

        /// <summary>
        /// Initializes networking and registers FIKA packet listeners if necessary.
        /// Only safe to call once.
        /// </summary>
        public static void Init(ConfigFile config)
        {
            if (_initialized)
            {
                Plugin.LogSource.LogDebug("[Routers] Init skipped — already initialized.");
                return;
            }

            _initialized = true;

            if (Settings.IsFika)
            {
                Plugin.Instance.StartCoroutine(WaitAndRegisterClientPacket());
            }

            Plugin.LogSource.LogInfo("[Routers] Initialization complete.");
        }

        private static System.Collections.IEnumerator WaitAndRegisterClientPacket()
        {
            Plugin.LogSource.LogDebug("[Routers] Waiting for IFikaNetworkManager...");

            while (!(FikaPlugin.Instance is IFikaNetworkManager))
            {
                yield return null;
            }

            if (FikaPlugin.Instance is IFikaNetworkManager networkManager)
            {
                try
                {
                    networkManager.RegisterPacket<PresetSyncPacket>(packet =>
                    {
                        if (!string.IsNullOrWhiteSpace(packet.PresetName))
                        {
                            HandleFikaPresetSync(packet.PresetLabel, packet.PresetName);
                        }
                        else
                        {
                            Plugin.LogSource.LogWarning("[Routers] Received empty or invalid PresetSyncPacket.");
                        }
                    });

                    Plugin.LogSource.LogInfo("[Routers] Registered PresetSyncPacket handler (client).");
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError($"[Routers] Exception during packet registration: {ex.Message}");
                }
            }
        }

        // ─────────────────────────────────────────────────────────────
        // ─── PRESET ACCESSORS ────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────

        public static string GetCurrentPresetLabel()
        {
            try
            {
                return RequestHandler.GetJson("/moar/currentPreset")?.Trim() ?? "live-like";
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[GetCurrentPresetLabel] Fallback: {ex.Message}");
                return Settings.currentPreset?.Value ?? Settings.ServerStoredDefaults?.Name ?? "live-like";
            }
        }

        public static string GetAnnouncePresetLabel()
        {
            try
            {
                return RequestHandler.GetJson("/moar/announcePreset")?.Trim() ?? "live-like";
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogWarning($"[GetAnnouncePresetLabel] Fallback: {ex.Message}");
                return Settings.currentPreset?.Value ?? Settings.ServerStoredDefaults?.Name ?? "live-like";
            }
        }

        public static string GetCurrentPresetName()
        {
            var label = GetCurrentPresetLabel();
            var preset = FindPresetByLabel(label);
            return preset?.Name ?? label ?? "Unknown";
        }

        public static string GetAnnouncePresetName() => _hostPresetLabel;
        public static void SetHostPresetLabel(string label) => _hostPresetLabel = label;

        private static Preset? FindPresetByLabel(string label)
        {
            if (Settings.PresetList == null)
            {
                Plugin.LogSource.LogError("[Routers] PresetList is null. Cannot match preset.");
                return null;
            }

            return Settings.PresetList.FirstOrDefault(p =>
                string.Equals(p.Label?.Trim(), label, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Name?.Trim(), label, StringComparison.OrdinalIgnoreCase));
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
                Plugin.LogSource.LogError($"[SetPreset] Failed: {ex.Message}");
                return "Failed to set preset.";
            }
        }

        public static List<Preset> GetPresetsList()
        {
            try
            {
                var json = RequestHandler.GetJson("/moar/getPresets");
                var response = JsonConvert.DeserializeObject<GetPresetsListResponse>(json);
                return response?.Data?.ToList() ?? new();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetPresetsList] Failed: {ex.Message}");
                return new();
            }
        }

        // ─────────────────────────────────────────────────────────────
        // ─── CONFIG MANAGEMENT ───────────────────────────────────────
        // ─────────────────────────────────────────────────────────────

        public static ConfigSettings GetServerConfigWithOverrides()
        {
            try
            {
                var json = RequestHandler.GetJson("/moar/getServerConfigWithOverrides");
                return JsonConvert.DeserializeObject<ConfigSettings>(json) ?? new();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetServerConfigWithOverrides] Failed: {ex.Message}");
                return new();
            }
        }

        public static ConfigSettings GetDefaultConfig()
        {
            try
            {
                var json = RequestHandler.GetJson("/moar/getDefaultConfig");
                return JsonConvert.DeserializeObject<ConfigSettings>(json) ?? new();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetDefaultConfig] Failed: {ex.Message}");
                return new();
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
                Plugin.LogSource.LogError($"[SetOverrideConfig] Failed: {ex.Message}");
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
                Plugin.LogSource.LogError($"[PostPlayerLocationTo] Failed on {endpoint}: {ex.Message}");
                return "Error submitting player position.";
            }
        }

        // ─────────────────────────────────────────────────────────────
        // ─── FIKA SYNC (CLIENT & HEADLESS) ───────────────────────────
        // ─────────────────────────────────────────────────────────────

        public static void HandleFikaPresetSync(string presetLabel, string presetName)
        {
            Plugin.LogSource.LogInfo($"[FIKA Sync] Received preset: {presetLabel} ({presetName})");

            if (Settings.PresetList == null)
            {
                Plugin.LogSource.LogError("[FIKA Sync] PresetList is null.");
                return;
            }

            if (Settings.currentPreset == null)
            {
                Plugin.LogSource.LogError("[FIKA Sync] currentPreset config entry is null.");
                return;
            }

            var match = Settings.PresetList.FirstOrDefault(p =>
                string.Equals(p.Name?.Trim(), presetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label?.Trim(), presetLabel, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                if (Settings.currentPreset.Value != match.Name)
                {
                    Settings.currentPreset.Value = match.Name;
                    _hostPresetLabel = match.Label;
                    Plugin.LogSource.LogInfo($"[FIKA Sync] Applied: {match.Label} ({match.Name})");
                }
                else
                {
                    Plugin.LogSource.LogDebug($"[FIKA Sync] Already current: {match.Label}");
                }
            }
            else
            {
                Plugin.LogSource.LogWarning($"[FIKA Sync] No match for: {presetLabel} / {presetName}");
            }
        }
    }
}
