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
    /// Utility methods used across MOAR for player tracking, debug notifications,
    /// and sync-safe network-aware message display.
    /// </summary>
    public static class Methods
    {
        /// <summary>
        /// Displays a client-side notification and optionally broadcasts to other players if in FIKA multiplayer.
        /// Safe for single-player, host, client, and headless environments.
        /// </summary>
        /// <param name="message">The message text to display.</param>
        /// <param name="icon">Optional notification icon (defaults to Quest icon).</param>
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

                // Optional network broadcast in FIKA Coop/Headless
                if (Settings.IsFika)
                    notification.BroadcastToClients();
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[DisplayMessage] Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously refreshes backend location info (e.g. after map changes).
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
                    Plugin.LogSource.LogWarning("[RefreshLocationInfo] Backend session unavailable.");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[RefreshLocationInfo] Failed to refresh: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the player's current world position and map for use in spawn-related tools.
        /// Returns a fallback response if player or game state is invalid.
        /// </summary>
        public static AddSpawnRequest GetPlayersCoordinatesAndLevel()
        {
            try
            {
                var player = Singleton<GameWorld>.Instance?.MainPlayer;

                if (player == null)
                {
                    Plugin.LogSource.LogWarning("[GetPlayersCoordinatesAndLevel] MainPlayer is null.");
                    return new AddSpawnRequest { Map = "Unknown", Position = new Ixyz() };
                }

                var pos = player.Position;

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
                Plugin.LogSource.LogError($"[GetPlayersCoordinatesAndLevel] Exception: {ex.Message}");
                return new AddSpawnRequest { Map = "Unknown", Position = new Ixyz() };
            }
        }

        /// <summary>
        /// Detects when the user presses the AnnounceKey and shows the current preset.
        /// Used during runtime for debug or multiplayer awareness.
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
