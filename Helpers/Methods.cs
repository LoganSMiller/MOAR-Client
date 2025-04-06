using System;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Communications;
using MOAR.Data;
using MOAR.Helpers;
using MOAR.Components.Notifications;
using SPT.Reflection.Utils;
using UnityEngine;

namespace MOAR.Helpers
{
    /// <summary>
    /// Utility methods used across MOAR for player tracking, debug notifications,
    /// and sync-safe network-aware message display.
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// Displays a local notification and optionally broadcasts across FIKA if on the server.
        /// Always safe for singleplayer, host, client, or headless modes.
        /// </summary>
        public static void DisplayMessage(string message, ENotificationIconType icon = ENotificationIconType.Quest)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                Plugin.LogSource.LogWarning("[DisplayMessage] Tried to display null or empty message.");
                return;
            }

            try
            {
                var notification = new DebugNotification
                {
                    Notification = message.Trim(),
                    NotificationIcon = icon
                };

                notification.Display();

                if (Settings.IsFika && Fika.Core.Coop.Utils.FikaBackendUtils.IsServer)
                {
                    notification.BroadcastToClients();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DisplayMessage] Exception during message broadcast: {ex}");
            }
        }

        /// <summary>
        /// Safely refreshes level settings from the backend session.
        /// Used for reinitializing spawn context or player state.
        /// </summary>
        public static async Task RefreshLocationInfo()
        {
            try
            {
                if (PatchConstants.BackEndSession != null)
                {
                    await PatchConstants.BackEndSession.GetLevelSettings().ConfigureAwait(false);
                }
                else
                {
                    Plugin.LogSource.LogWarning("[RefreshLocationInfo] Backend session is unavailable.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[RefreshLocationInfo] Exception while refreshing location info: {ex}");
            }
        }

        /// <summary>
        /// Retrieves the player's current position and map name, used for adding spawn points.
        /// </summary>
        public static AddSpawnRequest GetPlayersCoordinatesAndLevel()
        {
            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                var player = gameWorld?.MainPlayer;

                if (player == null)
                {
                    Plugin.LogSource.LogWarning("[GetPlayersCoordinatesAndLevel] MainPlayer is null.");
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
                Plugin.LogSource.LogError($"[GetPlayersCoordinatesAndLevel] Exception occurred: {ex}");
                return new AddSpawnRequest { Map = "Unknown", Position = new Ixyz() };
            }
        }

        /// <summary>
        /// Evaluates if the announce key was just pressed and manually shows the current preset.
        /// Safe in all runtime states and environments.
        /// </summary>
        public static void CheckAnnounceKey()
        {
            try
            {
                if (UIUtils.BetterIsDown(Settings.AnnounceKey.Value))
                {
                    Settings.AnnounceManually();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[CheckAnnounceKey] Failed to trigger manual preset announce: {ex}");
            }
        }
    }
}
