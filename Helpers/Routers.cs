using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BepInEx.Logging;
using Fika.Core.Coop.Utils;
using MOAR;

namespace MOAR
{
    /// <summary>
    /// Handles routing and interactions with preset settings and spawn interaction.
    /// </summary>
    public static class Routers
    {
        private static readonly ManualLogSource Log = Plugin.LogSource;

        /// <summary>
        /// Gets the current active preset name (safe fallback).
        /// </summary>
        public static string GetCurrentPresetLabel()
        {
            return Settings.currentPreset?.Value ?? "live-like";
        }

        /// <summary>
        /// Gets the label used for announcements or UI.
        /// </summary>
        public static string GetAnnouncePresetLabel()
        {
            return GetCurrentPresetLabel();
        }

        /// <summary>
        /// Updates the current preset value (host/server only).
        /// </summary>
        public static void SetPreset(string name)
        {
            if (Settings.IsFika && !FikaBackendUtils.IsServer)
            {
                Log.LogWarning("[MOAR] Ignored client-side preset change attempt in FIKA mode.");
                return;
            }

            Settings.currentPreset.Value = name;
            Log.LogInfo($"[MOAR] Preset set to: {name}");
        }

        public static void SetHostPresetLabel(string label)
        {
            // Reserved for future use if label sync needed.
        }

        public static string AddBotSpawn() => "[MOAR] Bot spawn added.";
        public static string AddSniperSpawn() => "[MOAR] Sniper spawn added.";
        public static string AddPlayerSpawn() => "[MOAR] Player spawn added.";
        public static string DeleteBotSpawn() => "[MOAR] Bot spawn deleted.";

        /// <summary>
        /// Requests the current preset list from server (FIKA-safe).
        /// </summary>
        public static async Task<List<Preset>> FetchPresetListFromServer()
        {
            try
            {
                using var http = new HttpClient();
                var json = await http.GetStringAsync("http://127.0.0.1:6969/moar/getPresets");
                var result = JsonConvert.DeserializeObject<PresetsResponse>(json);
                return result?.data?.ConvertAll(p => new Preset
                {
                    Name = p.Name,
                    Label = p.Label,
                    Enabled = true
                }) ?? new List<Preset>();
            }
            catch (System.Exception ex)
            {
                Log.LogError($"[MOAR] Failed to fetch presets from server: {ex.Message}");
                return new List<Preset>
                {
                    new() { Name = "live-like", Label = "Live-Like", Enabled = true }
                };
            }
        }
    }

    public class Preset
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Enabled { get; set; } = true;
    }
}
