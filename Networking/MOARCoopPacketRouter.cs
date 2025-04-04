using System;
using Comfort.Common; 
using Fika.Core.Networking;
using Fika.Core.Coop.Utils;
using MOAR.Helpers;
using MOAR.Packets;

namespace MOAR.Networking
{
    internal static class MOARCoopPacketRouter
    {
        private static bool _registered = false;
        private static bool _warnedOnce = false;

        public static void TryRegister()
        {
            if (_registered || !Settings.IsFika)
                return;

            if (!FikaBackendUtils.IsServer)
            {
                if (!_warnedOnce)
                {
                    Plugin.LogSource.LogDebug("[MOARCoopPacketRouter] Skipped packet registration — not server or headless host.");
                    _warnedOnce = true;
                }
                return;
            }

            
            if (Singleton<IFikaNetworkManager>.Instantiated)
            {
                var networkManager = Singleton<IFikaNetworkManager>.Instance;

                try
                {
                    networkManager.RegisterPacket<PresetSyncPacket>(MOARPresetSyncHandler.OnClientReceivedPresetPacket);
                    Plugin.LogSource.LogInfo("[MOARCoopPacketRouter] Registered PresetSyncPacket via IFikaNetworkManager.");
                    _registered = true;
                }
                catch (Exception ex)
                {
                    Plugin.LogSource.LogError($"[MOARCoopPacketRouter] Packet registration failed: {ex.Message}");
                }
            }
            else if (!_warnedOnce)
            {
                Plugin.LogSource.LogWarning("[MOARCoopPacketRouter] IFikaNetworkManager is not yet instantiated.");
                _warnedOnce = true;
            }
        }
    }
}
