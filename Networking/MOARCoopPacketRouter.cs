using System;
using Fika.Core;
using Fika.Core.Networking;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;
using MOAR.Packets;

namespace MOAR.Networking
{
    /// <summary>
    /// Handles safe registration of MOAR packet handlers with FIKA.
    /// Automatically avoids duplicate registration or invalid instances (client vs host).
    /// </summary>
    internal static class MOARCoopPacketRouter
    {
        private static bool _registered = false;
        private static bool _warnedOnce = false;

        /// <summary>
        /// Attempts to register the MOAR networking packets with FIKA.
        /// Safe to call repeatedly. Registers only on server or headless host.
        /// </summary>
        public static void TryRegister()
        {
            // Exit if already registered or FIKA isn't installed
            if (_registered || !Settings.IsFika)
                return;

            // Ensure we are the server or headless host before registering
            if (!FikaBackendUtils.IsServer)
            {
                if (!_warnedOnce)
                {
                    Plugin.LogSource.LogDebug("[MOARCoopPacketRouter] Skipped packet registration — not server or headless host.");
                    _warnedOnce = true;
                }
                return;
            }

            // Attempt to register via the FIKA network manager
            if (FikaPlugin.Instance is IFikaNetworkManager networkManager)
            {
                try
                {
                    networkManager.RegisterPacket<PresetSyncPacket>(MOARPresetSyncHandler.OnClientReceivedPresetPacket);
                    Plugin.LogSource.LogInfo("[MOARCoopPacketRouter] Successfully registered PresetSyncPacket.");
                    _registered = true;
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError($"[MOARCoopPacketRouter] Failed to register PresetSyncPacket: {ex.Message}");
                }
            }
            else if (!_warnedOnce)
            {
                Plugin.LogSource.LogWarning("[MOARCoopPacketRouter] FikaPlugin.Instance does not implement IFikaNetworkManager.");
                _warnedOnce = true;
            }
        }
    }
}
