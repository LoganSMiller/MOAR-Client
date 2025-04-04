using System;
using MOAR.Packets;
using MOAR.Components.Notifications;
using Fika.Core.Coop.Utils; 
using MOAR.Helpers;

namespace MOAR.Networking
{
    /// <summary>
    /// Provides handler for preset sync packets from FIKA networking.
    /// Registration is now handled via MOARCoopPacketRouter using IFikaNetworkManager.
    /// </summary>
    internal static class MOARPresetSyncHandler
    {
        public static void OnClientReceivedPresetPacket(PresetSyncPacket packet)
        {
            if (packet == null)
            {
                Plugin.LogSource.LogWarning("[MOARPresetSyncHandler] Received null PresetSyncPacket.");
                return;
            }

            Settings.currentPreset.Value = packet.PresetName;
            Routers.SetHostPresetLabel(packet.PresetLabel);

            Plugin.LogSource.LogInfo($"[MOARPresetSyncHandler] Synced preset from server: {packet.PresetName} / {packet.PresetLabel}");

            var notification = new DebugNotification
            {
                Notification = $"Server preset: {packet.PresetLabel}",
                NotificationIcon = EFT.Communications.ENotificationIconType.EntryPoint
            };
            notification.Display();

            if (Settings.IsFika && FikaBackendUtils.IsServer) 
                notification.BroadcastToClients();
        }
    }
}
