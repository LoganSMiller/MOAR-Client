using System;
using MOAR.Packets;
using MOAR.Components.Notifications;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;

namespace MOAR.Networking
{
    /// <summary>
    /// Handles reception of PresetSyncPacket from FIKA networking.
    /// Synchronizes current preset and optionally rebroadcasts in headless/server mode.
    /// </summary>
    internal static class MOARPresetSyncHandler
    {
        /// <summary>
        /// Processes an incoming preset sync packet from the server or host.
        /// </summary>
        /// <param name="packet">The preset sync packet received from the host.</param>
        public static void OnClientReceivedPresetPacket(PresetSyncPacket packet)
        {
            if (packet == null)
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Received null PresetSyncPacket.");
                return;
            }

            // Update preset configuration
            Settings.currentPreset.Value = packet.PresetName;
            Routers.SetHostPresetLabel(packet.PresetLabel);

            Plugin.LogSource.LogInfo($"[MOARPresetSyncHandler] Synced preset from host: {packet.PresetName} ({packet.PresetLabel})");

            // Create local notification
            var notification = new DebugNotification
            {
                Notification = $"Server preset: {packet.PresetLabel}",
                NotificationIcon = EFT.Communications.ENotificationIconType.EntryPoint
            };

            notification.Display();

            // Rebroadcast only if running as headless/server
            if (Settings.IsFika && FikaBackendUtils.IsServer)
            {
                Plugin.LogSource.LogDebug("[MOARPresetSyncHandler] Rebroadcasting preset notification to connected clients.");
                notification.BroadcastToClients();
            }
        }
    }
}
