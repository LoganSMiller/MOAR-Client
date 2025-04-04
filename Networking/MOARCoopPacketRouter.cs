using System;
using Fika.Core.Networking;
using MOAR.Packets;
using MOAR.Helpers;
using Fika.Core;

namespace MOAR.Networking
{
    /// <summary>
    /// Registers MOAR-specific FIKA packets with the FIKA network manager.
    /// This class ensures packets are only registered when FikaPlugin is ready.
    /// </summary>
    internal static class MOARCoopPacketRouter
    {
        public static void Register()
        {
            if (FikaPlugin.Instance is IFikaNetworkManager networkManager)
            {
                networkManager.RegisterPacket<PresetSyncPacket>(MOARPresetSyncHandler.OnClientReceivedPresetPacket);
                Plugin.LogSource.LogInfo("[FIKA Sync] Registered PresetSyncPacket via FikaPlugin.");
            }
            else
            {
                Plugin.LogSource.LogError("[FIKA Sync] FikaPlugin.Instance not available — failed to register PresetSyncPacket.");
            }
        }
    }
}
