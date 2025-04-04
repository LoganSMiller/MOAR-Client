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
    /// A custom debug notification used to display messages in-game.
    /// Safe for local and FIKA multiplayer use. Supports network broadcast.
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
        /// Broadcasts this notification to all FIKA clients.
        /// Safe to call from host or headless.
        /// </summary>
        public void BroadcastToClients()
        {
#if FIKA
            if (!Settings.IsFika)
                return;

            try
            {
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
        /// Serializes this notification to JSON.
        /// </summary>
        public string ToJson() => JsonConvert.SerializeObject(this);

        /// <summary>
        /// Deserializes a DebugNotification from JSON.
        /// </summary>
        public static DebugNotification FromJson(string json) =>
            JsonConvert.DeserializeObject<DebugNotification>(json);

        /// <summary>
        /// Registers the FIKA packet listener for remote notification delivery.
        /// Safe to call repeatedly.
        /// </summary>
        public static void RegisterNetworkHandler()
        {
#if FIKA
            if (!Settings.IsFika)
                return;

            try
            {
                FikaNetwork.On("MOAR:Notification", (json) =>
                {
                    var notification = FromJson(json);
                    notification?.Display();
                });

                Plugin.LogSource.LogDebug("[DebugNotification] Registered FIKA notification handler.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DebugNotification.RegisterNetworkHandler] Error: {ex.Message}");
            }
#endif
        }
    }
}
