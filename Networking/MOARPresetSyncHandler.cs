using MOAR.Helpers;
using System;
using System.Linq;

namespace MOAR.Networking
{
    public static class MOARPresetSyncHandler
    {
        public static void OnClientReceivedPresetPacket(PresetSyncPacket packet)
        {
            if (string.IsNullOrWhiteSpace(packet.PresetName))
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Received PresetSyncPacket with empty name.");
                return;
            }

            var match = Settings.PresetList?.FirstOrDefault(p =>
                string.Equals(p.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase));

            if (match != null)
            {
                Settings.currentPreset.Value = match.Name;
                Routers.SetHostPresetLabel(match.Label);
                Plugin.LogSource.LogInfo($"[MOAR] Applied preset: {match.Label} ({match.Name})");
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOAR] No matching preset found for: {packet.PresetLabel} / {packet.PresetName}");
            }
        }
    }
}
