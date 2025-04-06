using System;
using System.Linq;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;
using EFT.Communications;
using MOAR.Packets;
using MOAR.Data;
using SPT.Reflection.Utils;
using UnityEngine;
using MOAR.Components.Notifications;
using MOAR.Helpers;

namespace MOAR.Helpers
{
    /// <summary>
    /// Utility methods used across MOAR for player tracking, debug notifications,
    /// and sync-safe network-aware message display.
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// Displays a local notification and optionally broadcasts across FIKA if server.
        /// Always safe for use in singleplayer, host, client, or headless.
        /// </summary>
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
                    Notification = message.Trim(),
                    NotificationIcon = icon
                };

                // Always show locally
                notification.Display();

                // If FIKA server or host, propagate to clients
                if (Settings.IsFika && Fika.Core.Coop.Utils.FikaBackendUtils.IsServer)
                {
                    notification.BroadcastToClients();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DisplayMessage] Exception: {ex}");
            }
        }

        /// <summary>
        /// Refreshes the location backend info for the current session (safe async).
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
                    Plugin.LogSource.LogWarning("[RefreshLocationInfo] Backend session unavailable.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[RefreshLocationInfo] Failed to refresh: {ex}");
            }
        }

        /// <summary>
        /// Gets the player's coordinates and location for use with bot spawn placement tools.
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
                Plugin.LogSource.LogError($"[GetPlayersCoordinatesAndLevel] Exception: {ex}");
                return new AddSpawnRequest { Map = "Unknown", Position = new Ixyz() };
            }
        }

        /// <summary>
        /// Checks for manual announce key and displays current preset if pressed.
        /// Safe in any runtime state.
        /// </summary>
        public static void CheckAnnounceKey()
        {
            try
            {
                if (MOAR.Helpers.ConfigEntryExtensions.BetterIsDown(Settings.AnnounceKey.Value))
                {
                    Settings.AnnounceManually();
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[CheckAnnounceKey] Failed to announce: {ex}");
            }
        }
    }
}
