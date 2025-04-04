using System;
using EFT.Communications;
using Newtonsoft.Json;
using MOAR.Helpers;

#if FIKA
using Fika.Networking;
using Fika.Networking.Models;
#endif

namespace MOAR.Components.Notifications
{
    /// <summary>
    /// Custom debug notification used to display in-game messages during development.
    /// Multiplayer-safe and synchronized across FIKA sessions.
    /// </summary>
    public class DebugNotification : NotificationAbstractClass
    {
        [JsonProperty("notificationIcon")]
        public ENotificationIconType NotificationIcon;

        [JsonProperty("notification")]
        public string Notification;

        public override ENotificationIconType Icon => NotificationIcon;

        public override string Description => Notification;

        /// <summary>
        /// Displays this notification locally on the client.
        /// </summary>
        public void Display()
        {
            try
            {
                NotificationManagerClass.DisplayNotification(this);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DebugNotification.Display] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Broadcasts the notification to all FIKA clients.
        /// </summary>
        public void BroadcastToClients()
        {
#if FIKA
            try
            {
                if (!Settings.IsFika)
                    return;

                var payload = ToJson();
                FikaNetwork.SendToAll("MOAR:Notification", payload);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DebugNotification.BroadcastToClients] Error: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Converts this notification to a JSON string.
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        /// <summary>
        /// Creates a notification from a JSON string.
        /// </summary>
        public static DebugNotification FromJson(string json)
        {
            return JsonConvert.DeserializeObject<DebugNotification>(json);
        }

        /// <summary>
        /// Registers the packet handler for remote notification reception (FIKA only).
        /// </summary>
        public static void RegisterNetworkHandler()
        {
#if FIKA
            if (!Settings.IsFika)
                return;

            FikaNetwork.On("MOAR:Notification", (json) =>
            {
                try
                {
                    var notification = FromJson(json);
                    notification?.Display();
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError($"[DebugNotification.RegisterNetworkHandler] Error: {ex.Message}");
                }
            });
#endif
        }
    }
}
