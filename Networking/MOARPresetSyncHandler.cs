using MOAR.Helpers;
using System;
using System.Linq;

namespace MOAR.Networking
{
    /// <summary>
    /// Handles incoming preset sync packets and applies the correct configuration.
    /// Invoked automatically during FIKA host → client synchronization.
    /// </summary>
    public static class MOARPresetSyncHandler
    {
        /// <summary>
        /// Applies the received preset sync packet client-side.
        /// </summary>
        public static void OnClientReceivedPresetPacket(PresetSyncPacket packet)
        {
            if (packet == null)
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Received null PresetSyncPacket.");
                return;
            }

            if (string.IsNullOrWhiteSpace(packet.PresetName))
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Preset name is empty.");
                return;
            }

            var presets = Settings.PresetList;
            if (presets == null || presets.Count == 0)
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Preset list is empty or null — cannot apply sync.");
                return;
            }

            var match = presets.FirstOrDefault(p =>
                string.Equals(p.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                if (Settings.currentPreset != null)
                {
                    Settings.currentPreset.Value = match.Name;
                }

                Routers.SetHostPresetLabel(match.Label);
                Plugin.LogSource.LogInfo($"[MOARPresetSyncHandler] Synchronized preset: {match.Label} ({match.Name})");
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOARPresetSyncHandler] No matching preset found for: {packet.PresetLabel} / {packet.PresetName}");
                Plugin.LogSource.LogDebug($"[MOARPresetSyncHandler] Available presets: {string.Join(", ", presets.Select(p => p.Label))}");
            }
        }
    }
}
