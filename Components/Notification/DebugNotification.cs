using System;
using Comfort.Common;
using EFT.Communications;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using Fika.Core.Coop;
using LiteNetLib;
using MOAR.Packets;

namespace MOAR.Components.Notifications;

/// <summary>
/// A lightweight wrapper for displaying notifications locally and broadcasting via FIKA.
/// </summary>
public sealed class DebugNotification
{
    public string Notification { get; set; } = string.Empty;
    public ENotificationIconType NotificationIcon { get; set; } = ENotificationIconType.Default;

    private static FikaServer? _server;
    private static bool _registered = false;

    /// <summary>
    /// Displays the notification locally on the current client.
    /// </summary>
    public void Display()
    {
        if (string.IsNullOrWhiteSpace(Notification))
        {
            Plugin.LogSource.LogWarning("[DebugNotification] Tried to display empty notification.");
            return;
        }

        NotificationManagerClass.DisplayMessageNotification(
            Notification,
            ENotificationDurationType.Default,
            NotificationIcon
        );

        Plugin.LogSource.LogDebug($"[DebugNotification] Displayed locally: {Notification}");
    }

    /// <summary>
    /// Sends this notification to all clients if running as FIKA server.
    /// </summary>
    public void BroadcastToClients()
    {
        if (!Fika.Core.Coop.Utils.FikaBackendUtils.IsServer || _server == null)
        {
            Plugin.LogSource.LogWarning("[DebugNotification] Broadcast skipped — not server or no server ref.");
            return;
        }

        try
        {
            var packet = BuildPacket();

            _server.SendDataToAll(ref packet, DeliveryMethod.ReliableUnordered);
            Plugin.LogSource.LogDebug($"[DebugNotification] Broadcasted to clients: {Notification}");
        }
        catch (Exception ex)
        {
            Plugin.LogSource.LogError($"[DebugNotification] Failed to broadcast: {ex.Message}");
        }
    }

    /// <summary>
    /// Registers the client handler for incoming DebugNotificationPackets.
    /// </summary>
    public static void RegisterNetworkHandler()
    {
        if (_registered)
        {
            Plugin.LogSource.LogWarning("[DebugNotification] Handler already registered — skipping.");
            return;
        }

        FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnNetworkReady);
        _registered = true;
        Plugin.LogSource.LogInfo("[DebugNotification] Registered FIKA network event listener.");
    }

    private static void OnNetworkReady(FikaNetworkManagerCreatedEvent ev)
    {
        if (ev.Manager is FikaClient client)
        {
            client.RegisterPacket<DebugNotificationPacket>(HandleNotificationPacket);
            Plugin.LogSource.LogInfo("[DebugNotification] Registered DebugNotificationPacket handler (client)");
        }
        else if (ev.Manager is FikaServer server)
        {
            _server = server;
            Plugin.LogSource.LogInfo("[DebugNotification] Stored FikaServer reference");
        }
    }

    private static void HandleNotificationPacket(DebugNotificationPacket packet)
    {
        if (string.IsNullOrWhiteSpace(packet.Message))
        {
            Plugin.LogSource.LogWarning("[DebugNotification] Received empty message — skipping.");
            return;
        }

        NotificationManagerClass.DisplayMessageNotification(
            packet.Message,
            ENotificationDurationType.Default,
            packet.Icon
        );

        Plugin.LogSource.LogDebug($"[DebugNotification] Displayed from host: {packet.Message}");
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
