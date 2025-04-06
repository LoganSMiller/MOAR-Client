using MOAR.Helpers;
using MOAR.Packets;
using System;
using System.Linq;

namespace MOAR.Networking
{
    /// <summary>
    /// Handles application and reset of preset sync packets received via FIKA.
    /// </summary>
    public static class MOARPresetSyncHandler
    {
        private static string _lastReceivedVersion = "unknown";

        /// <summary>
        /// Applies a preset sync packet received from the FIKA host.
        /// </summary>
        public static void OnClientReceivedPresetPacket(PresetSyncPacket packet)
        {
            if (packet == null || string.IsNullOrWhiteSpace(packet.PresetName) || Settings.PresetList == null)
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Received invalid or empty PresetSyncPacket.");
                return;
            }

            _lastReceivedVersion = packet.Version ?? "unknown";

            if (_lastReceivedVersion != Plugin.Version)
            {
                Plugin.LogSource.LogWarning($"[MOARPresetSyncHandler] Version mismatch — Host: {_lastReceivedVersion}, Local: {Plugin.Version}");
            }

            var match = Settings.PresetList.FirstOrDefault(p =>
                string.Equals(p.Name?.Trim(), packet.PresetName?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label?.Trim(), packet.PresetLabel?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                Settings.currentPreset.Value = match.Name;
                Routers.SetHostPresetLabel(match.Label);
                Plugin.LogSource.LogInfo($"[MOARPresetSyncHandler] Preset sync applied: {match.Label} ({match.Name})");
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOARPresetSyncHandler] No matching preset found for: {packet.PresetLabel} / {packet.PresetName} — attempting fallback...");

                var fallback = Settings.PresetList.FirstOrDefault(p =>
                    string.Equals(p.Name?.Trim(), "live-like", StringComparison.OrdinalIgnoreCase));

                if (fallback != null)
                {
                    Settings.currentPreset.Value = fallback.Name;
                    Routers.SetHostPresetLabel(fallback.Label);
                    Plugin.LogSource.LogWarning($"[MOARPresetSyncHandler] Fallback applied: {fallback.Label} ({fallback.Name})");
                }
                else
                {
                    Plugin.LogSource.LogError("[MOARPresetSyncHandler] No fallback preset ('live-like') found in PresetList.");
                }
            }
        }

        /// <summary>
        /// Resets sync state on raid end or disconnect.
        /// </summary>
        public static void ResetSync()
        {
            _lastReceivedVersion = "unknown";
            Routers.SetHostPresetLabel("Unknown");
            Plugin.LogSource.LogDebug("[MOARPresetSyncHandler] FIKA sync state reset.");
        }
    }
}
