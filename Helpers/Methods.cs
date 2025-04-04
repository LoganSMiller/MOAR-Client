using System;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Communications;
using MOAR.Components.Notifications;
using MOAR.Data;
using SPT.Reflection.Utils;
using UnityEngine;

namespace MOAR.Helpers
{
    /// <summary>
    /// Common utility methods for UI messaging, location sync, and player coordinate capture.
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// Displays an in-game notification and safely broadcasts it in multiplayer if FIKA is present.
        /// This method is FIKA-safe and does nothing multiplayer-wise if FIKA is not loaded.
        /// </summary>
        /// <param name="message">Message text to show</param>
        /// <param name="icon">Optional icon type (default = Quest)</param>
        public static void DisplayMessage(string message, ENotificationIconType icon = ENotificationIconType.Quest)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Plugin.LogSource.LogWarning("[DisplayMessage] Tried to show null or empty message.");
                return;
            }

            try
            {
                var notification = new DebugNotification
                {
                    Notification = message,
                    NotificationIcon = icon
                };

                notification.Display();

                // Only broadcast if FIKA is active
                if (Settings.IsFika)
                    notification.BroadcastToClients();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DisplayMessage] Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Triggers a location info refresh by querying the backend session.
        /// Used to refresh available zone and raid metadata.
        /// </summary>
        public static async Task RefreshLocationInfo()
        {
            try
            {
                if (PatchConstants.BackEndSession != null)
                {
                    await PatchConstants.BackEndSession.GetLevelSettings();
                }
                else
                {
                    Plugin.LogSource.LogWarning("[RefreshLocationInfo] BackEndSession is null");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[RefreshLocationInfo] Error refreshing: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns current player world position and location string for spawn tracking and server sync.
        /// </summary>
        /// <returns>AddSpawnRequest with position and map</returns>
        public static AddSpawnRequest GetPlayersCoordinatesAndLevel()
        {
            try
            {
                var player = Singleton<GameWorld>.Instance?.MainPlayer;

                if (player == null)
                {
                    Plugin.LogSource.LogWarning("[GetPlayersCoordinatesAndLevel] MainPlayer is null");
                    return new AddSpawnRequest { Map = "Unknown", Position = new Ixyz() };
                }

                Vector3 pos = player.Position;

                return new AddSpawnRequest
                {
                    Map = player.Location ?? "Unknown",
                    Position = new Ixyz
                    {
                        X = pos.x,
                        Y = pos.y,
                        Z = pos.z
                    }
                };
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[GetPlayersCoordinatesAndLevel] Error: {ex.Message}");
                return new AddSpawnRequest { Map = "Unknown", Position = new Ixyz() };
            }
        }

        /// <summary>
        /// Polls for the announce key each frame and triggers manual preset display if pressed.
        /// </summary>
        public static void CheckAnnounceKey()
        {
            try
            {
                if (Settings.AnnounceKey?.Value.BetterIsDown() == true)
                {
                    Settings.AnnounceManually();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[CheckAnnounceKey] Failed to announce: {ex.Message}");
            }
        }
    }
}
