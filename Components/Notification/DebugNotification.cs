using System;
using Comfort.Common;
using EFT.Communications;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Coop;
using LiteNetLib;
using MOAR.Packets;

namespace MOAR.Components.Notifications
{
    /// <summary>
    /// Handles both local display and FIKA-based broadcast of debug notifications.
    /// </summary>
    public sealed class DebugNotification
    {
        public string Notification { get; set; } = string.Empty;
        public ENotificationIconType NotificationIcon { get; set; } = ENotificationIconType.Default;

        private static FikaServer? _server;
        private static bool _registered = false;

        /// <summary>
        /// Displays the message locally.
        /// </summary>
        public void Display()
        {
            if (string.IsNullOrWhiteSpace(Notification))
            {
                Plugin.LogSource.LogWarning("[DebugNotification] Tried to display an empty message.");
                return;
            }

            if (!Singleton<NotificationManagerClass>.Instantiated || Singleton<NotificationManagerClass>.Instance == null)
            {
                Plugin.LogSource.LogDebug("[DebugNotification] Skipped local display — NotificationManager not available.");
                return;
            }

            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    Notification,
                    ENotificationDurationType.Default,
                    NotificationIcon
                );

                Plugin.LogSource.LogDebug($"[DebugNotification] Displayed locally: {Notification}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DebugNotification] Display failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcasts the notification to all FIKA clients if on the server.
        /// </summary>
        public void BroadcastToClients()
        {
            if (!Fika.Core.Coop.Utils.FikaBackendUtils.IsServer || _server == null)
            {
                Plugin.LogSource.LogDebug("[DebugNotification] Skipped broadcast — not server or server ref missing.");
                return;
            }

            try
            {
                var packet = BuildPacket();
                _server.SendDataToAll(ref packet, DeliveryMethod.ReliableUnordered);
                Plugin.LogSource.LogDebug($"[DebugNotification] Broadcasted to all clients: {Notification}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DebugNotification] Broadcast failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Registers the network packet handler for DebugNotificationPacket.
        /// </summary>
        public static void RegisterNetworkHandler()
        {
            if (_registered)
            {
                Plugin.LogSource.LogDebug("[DebugNotification] Already registered.");
                return;
            }

            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkReady);
            _registered = true;
            Plugin.LogSource.LogInfo("[DebugNotification] Subscribed to FIKA network manager event.");
        }

        private static void OnNetworkReady(FikaNetworkManagerCreatedEvent ev)
        {
            if (ev.Manager is FikaClient client)
            {
                client.RegisterPacket<DebugNotificationPacket>(HandleNotificationPacket);
                Plugin.LogSource.LogInfo("[DebugNotification] Registered client handler for DebugNotificationPacket.");
            }
            else if (ev.Manager is FikaServer server)
            {
                _server = server;
                Plugin.LogSource.LogInfo("[DebugNotification] Server reference set.");
            }
        }

        private static void HandleNotificationPacket(DebugNotificationPacket packet)
        {
            if (string.IsNullOrWhiteSpace(packet.Message))
            {
                Plugin.LogSource.LogWarning("[DebugNotification] Received empty packet — skipping.");
                return;
            }

            if (!Singleton<NotificationManagerClass>.Instantiated || Singleton<NotificationManagerClass>.Instance == null)
            {
                Plugin.LogSource.LogDebug("[DebugNotification] Skipped remote display — NotificationManager not available.");
                return;
            }

            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    packet.Message,
                    ENotificationDurationType.Default,
                    packet.Icon
                );

                Plugin.LogSource.LogDebug($"[DebugNotification] Displayed remote message: {packet.Message}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DebugNotification] Remote display failed: {ex.Message}");
            }
        }

        private DebugNotificationPacket BuildPacket()
        {
            return new DebugNotificationPacket
            {
                Message = Notification?.Trim() ?? string.Empty,
                Icon = NotificationIcon
            };
        }
    }
}
