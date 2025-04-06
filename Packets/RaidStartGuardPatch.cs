using System.Reflection;
using EFT;
using EFT.Communications;
using Fika.Core.Coop.Utils;
using HarmonyLib;
using MOAR.Networking;
using MOAR.Helpers;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Ensures MOAR config was synced before raid start, warning clients if not received.
    /// Matches Tyfon-style lifecycle guard to prevent headless desyncs.
    /// </summary>
    public sealed class RaidStartGuardPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted), BindingFlags.Instance | BindingFlags.Public);

        [PatchPostfix]
        [HarmonyPriority(Priority.Last)] // Ensure this runs at the very end of OnGameStarted
        private static void Postfix()
        {
            if (!Settings.IsFika)
            {
                Plugin.LogSource.LogDebug("[RaidStartGuardPatch] FIKA not active — skipping sync check.");
                return;
            }

            if (FikaBackendUtils.IsServer)
            {
                Plugin.LogSource.LogDebug("[RaidStartGuardPatch] Running on server — no need to verify client-side sync.");
                return;
            }

            if (MOARSync.ConfigReceived)
            {
                Plugin.LogSource.LogDebug("[RaidStartGuardPatch] Preset config was received before raid start — OK.");
                return;
            }

            // ❌ Sync failed — warn player
            Plugin.LogSource.LogError("[RaidStartGuardPatch] Host failed to sync MOAR config — warning player.");
            NotificationManagerClass.DisplayWarningNotification(
                "MOAR sync failed! Host may not have the MOAR mod installed.",
                ENotificationDurationType.Long
            );
        }
    }
}
