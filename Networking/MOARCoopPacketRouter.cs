using System;
using Fika.Core.Networking;
using MOAR.Packets;
using MOAR.Helpers;
using Fika.Core;

namespace MOAR.Networking
{
    /// <summary>
    /// Registers MOAR packets with FIKA when ready. Safe to call repeatedly.
    /// </summary>
    internal static class MOARCoopPacketRouter
    {
        private static bool _registered = false;
        private static bool _warnedOnce = false;

        public static void TryRegister()
        {
            if (_registered)
                return;

            if (FikaPlugin.Instance is IFikaNetworkManager networkManager)
            {
                networkManager.RegisterPacket<PresetSyncPacket>(MOARPresetSyncHandler.OnClientReceivedPresetPacket);
                Plugin.LogSource.LogInfo("[FIKA Sync] Registered PresetSyncPacket via FikaPlugin.");
                _registered = true;
            }
            else if (!_warnedOnce)
            {
                Plugin.LogSource.LogWarning("[FIKA Sync] FikaPlugin.Instance not ready — will retry...");
                _warnedOnce = true;
            }
        }
    }
}
