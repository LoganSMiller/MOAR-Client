using System;
using Fika.Core.Networking;
using MOAR.Packets;
using MOAR.Helpers;

namespace MOAR.Networking
{
    /// <summary>
    /// Registers packet handler for preset sync in Coop mode, supporting client and headless instances.
    /// </summary>
    internal static class MOARPresetSyncHandler
    {
        public static void Register()
        {
            if (!Settings.IsFika)
            {
                Plugin.LogSource.LogDebug("[MOARPresetSyncHandler] Not using FIKA — skipping preset sync registration.");
                return;
            }

            if (Fika.Core.FikaPlugin.Instance is not IFikaNetworkManager networkManager)
            {
                Plugin.LogSource.LogError("[MOARPresetSyncHandler] Could not access IFikaNetworkManager via FikaPlugin.Instance.");
                return;
            }

            networkManager.RegisterPacket<PresetSyncPacket>(packet =>
            {
                if (packet == null)
                {
                    Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Received null PresetSyncPacket.");
                    return;
                }

                Plugin.LogSource.LogInfo($"[MOARPresetSyncHandler] Received PresetSyncPacket: {packet.PresetName} / {packet.PresetLabel}");

                var match = Settings.PresetList?.Find(p =>
                    string.Equals(p.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(p.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    Settings.currentPreset.Value = match.Name;
                    var label = match.Label ?? match.Name;
                    Plugin.LogSource.LogInfo($"[MOARPresetSyncHandler] Applied preset: {label} ({match.Name})");
                }
                else
                {
                    Plugin.LogSource.LogWarning($"[MOARPresetSyncHandler] No matching preset found for: {packet.PresetName} / {packet.PresetLabel}");
                }
            });

            Plugin.LogSource.LogInfo("[MOARPresetSyncHandler] Preset sync packet handler registered via IFikaNetworkManager.");
        }
    }
}
