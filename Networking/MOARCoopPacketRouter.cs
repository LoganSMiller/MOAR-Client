using System;
using System.Linq;
using Comfort.Common;
using Fika.Core.Networking;
using MOAR.Helpers;
using MOAR.Packets;

namespace MOAR.Networking
{
    /// <summary>
    /// Handles incoming preset sync packets via FIKA's client networking system.
    /// Ensures synced preset is applied locally when received from server.
    /// </summary>
    internal static class MOARCoopPacketRouter
    {
        /// <summary>
        /// Invoked after a preset has been successfully synced and applied.
        /// </summary>
        public static event Action<string>? OnPresetSynced;

        /// <summary>
        /// Registers the handler for PresetSyncPacket received from FIKA networking.
        /// </summary>
        public static void Register()
        {
            var client = Singleton<FikaClient>.Instance;
            if (client == null)
            {
                Plugin.LogSource.LogError("[MOARCoopPacketRouter] FikaClient instance not available. Cannot register packet handler.");
                return;
            }

            client.RegisterPacket<PresetSyncPacket>(OnPresetPacketReceived);
            Plugin.LogSource.LogInfo("[MOARCoopPacketRouter] Handler registered for PresetSyncPacket via FikaClient.");
        }

        /// <summary>
        /// Handles the incoming PresetSyncPacket and applies the appropriate preset.
        /// </summary>
        private static void OnPresetPacketReceived(PresetSyncPacket packet)
        {
            if (packet is not { PresetName: not null, PresetLabel: not null })
            {
                Plugin.LogSource.LogWarning("[MOARCoopPacketRouter] Received null or incomplete PresetSyncPacket.");
                return;
            }

            Plugin.LogSource.LogInfo($"[MOARCoopPacketRouter] Received PresetSyncPacket: Name = {packet.PresetName}, Label = {packet.PresetLabel}");

            var matchingPreset = Settings.PresetList?.Find(preset =>
                string.Equals(preset.Name, packet.PresetName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(preset.Label, packet.PresetLabel, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingPreset != null)
            {
                Settings.currentPreset.Value = matchingPreset.Name;
                Plugin.LogSource.LogInfo($"[MOARCoopPacketRouter] Applied synced preset: {matchingPreset.Label} ({matchingPreset.Name})");

                OnPresetSynced?.Invoke(matchingPreset.Name);
                LogPresetDetails(matchingPreset.Name);
            }
            else
            {
                Plugin.LogSource.LogWarning($"[MOARCoopPacketRouter] No matching preset found for: {packet.PresetName} / {packet.PresetLabel}");

                var fallback = Settings.PresetList?.FirstOrDefault(p =>
                    p.Name.Equals("live-like", StringComparison.OrdinalIgnoreCase))
                    ?? Settings.PresetList?.FirstOrDefault();

                if (fallback != null)
                {
                    Settings.currentPreset.Value = fallback.Name;
                    Plugin.LogSource.LogWarning($"[MOARCoopPacketRouter] Falling back to preset: {fallback.Label} ({fallback.Name})");

                    OnPresetSynced?.Invoke(fallback.Name);
                    LogPresetDetails(fallback.Name);
                }
                else
                {
                    Plugin.LogSource.LogError("[MOARCoopPacketRouter] No presets available to fall back to!");
                }
            }
        }

        /// <summary>
        /// Logs extended preset details for debugging.
        /// </summary>
        private static void LogPresetDetails(string presetName)
        {
            var preset = Settings.PresetList?.Find(p => p.Name.Equals(presetName, StringComparison.OrdinalIgnoreCase));
            if (preset is null)
            {
                Plugin.LogSource.LogWarning($"[MOARCoopPacketRouter] Unable to retrieve details for preset: {presetName}");
                return;
            }

            Plugin.LogSource.LogInfo($"[MOARCoopPacketRouter] Preset Details — Name: {preset.Name}, Label: {preset.Label}");
        }
    }
}
