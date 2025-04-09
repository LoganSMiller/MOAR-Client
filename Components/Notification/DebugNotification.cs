#nullable enable
using System;
using Comfort.Common;
using EFT.Communications;
using Fika.Core.Coop;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LiteNetLib;
using MOAR.Packets;

namespace MOAR.Components.Notifications
{
    public sealed class DebugNotification
    {
        public string Notification { get; set; } = string.Empty;
        public ENotificationIconType NotificationIcon { get; set; } = ENotificationIconType.Default;

        private static FikaServer? _server;
        private static bool _registered;

        public void Display()
        {
            if (string.IsNullOrWhiteSpace(Notification))
            {
                Plugin.LogSource.LogWarning($"[{nameof(DebugNotification)}] Tried to display empty message.");
                return;
            }

            if (Fika.Core.Coop.Utils.FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] Skipped display in headless mode.");
                return;
            }

            if (!Singleton<NotificationManagerClass>.Instantiated || Singleton<NotificationManagerClass>.Instance == null)
            {
                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] NotificationManager not ready.");
                return;
            }

            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    Notification,
                    ENotificationDurationType.Default,
                    NotificationIcon
                );

                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] Displayed locally: {Notification}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[{nameof(DebugNotification)}] Display failed: {ex.Message}");
            }
        }

        public void BroadcastToClients()
        {
            if (string.IsNullOrWhiteSpace(Notification))
            {
                Plugin.LogSource.LogWarning($"[{nameof(DebugNotification)}] Tried to broadcast empty message.");
                return;
            }

            if (!Fika.Core.Coop.Utils.FikaBackendUtils.IsServer || _server == null)
            {
                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] Skipped broadcast — not host or missing server ref.");
                return;
            }

            try
            {
                var packet = new DebugNotificationPacket(Notification.Trim(), NotificationIcon);
                _server.SendDataToAll(ref packet, DeliveryMethod.ReliableUnordered);
                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] Broadcasted to clients: {Notification}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[{nameof(DebugNotification)}] Broadcast failed: {ex.Message}");
            }
        }

        public static void RegisterNetworkHandler()
        {
            if (_registered)
            {
                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] Already registered.");
                return;
            }

            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkReady);
            _registered = true;

            Plugin.LogSource.LogInfo($"[{nameof(DebugNotification)}] Subscribed to FIKA network manager event.");
        }

        private static void OnNetworkReady(FikaNetworkManagerCreatedEvent ev)
        {
            if (ev.Manager is FikaClient client)
            {
                client.RegisterPacket<DebugNotificationPacket>(HandleNotificationPacket);
                Plugin.LogSource.LogInfo($"[{nameof(DebugNotification)}] Registered client packet handler.");
            }
            else if (ev.Manager is FikaServer server)
            {
                _server = server;
                Plugin.LogSource.LogInfo($"[{nameof(DebugNotification)}] Server reference stored.");
            }
        }

        private static void HandleNotificationPacket(DebugNotificationPacket packet)
        {
            if (string.IsNullOrWhiteSpace(packet.Message))
            {
                Plugin.LogSource.LogWarning($"[{nameof(DebugNotification)}] Received empty notification.");
                return;
            }

            if (!Singleton<NotificationManagerClass>.Instantiated || Singleton<NotificationManagerClass>.Instance == null)
            {
                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] NotificationManager not ready — skipping remote display.");
                return;
            }

            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    packet.Message,
                    ENotificationDurationType.Default,
                    packet.Icon
                );

                Plugin.LogSource.LogDebug($"[{nameof(DebugNotification)}] Displayed remote message: {packet.Message}");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[{nameof(DebugNotification)}] Remote display failed: {ex.Message}");
            }
        }
    }
}
