using System.Collections;
using UnityEngine;
using Fika.Core;
using Fika.Core.Networking;
using MOAR.Helpers;
using MOAR.Packets;

namespace MOAR.Networking
{
    /// <summary>
    /// Handles safe registration of MOAR packet handlers with FIKA networking.
    /// Supports headless and traditional multiplayer sessions.
    /// </summary>
    internal static class MOARCoopPacketRouter
    {
        private static bool _registered = false;

        /// <summary>
        /// Initiates the coroutine-based registration of packet handlers.
        /// Safe to call repeatedly. Runs only if FIKA is installed and valid.
        /// </summary>
        public static void TryRegister()
        {
            if (_registered || !Settings.IsFika || Plugin.Instance == null)
                return;

            Plugin.Instance.StartCoroutine(WaitAndRegister());
        }

        private static IEnumerator WaitAndRegister()
        {
            Plugin.LogSource.LogDebug("[MOARCoopPacketRouter] Waiting for IFikaNetworkManager...");

            // Wait until FikaPlugin.Instance is available and valid
            while (!(FikaPlugin.Instance is IFikaNetworkManager))
            {
                yield return null;
            }

            var networkManager = (IFikaNetworkManager)FikaPlugin.Instance;

            try
            {
                networkManager.RegisterPacket<PresetSyncPacket>(packet =>
                {
                    MOARPresetSyncHandler.OnClientReceivedPresetPacket(packet);
                });

                _registered = true;
                Plugin.LogSource.LogInfo("[MOARCoopPacketRouter] PresetSyncPacket registered successfully.");
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource.LogError($"[MOARCoopPacketRouter] Registration error: {ex.Message}");
            }

            // Optional: yield briefly to ensure sync stability in early raid phases
            yield return null;
        }
    }
}
