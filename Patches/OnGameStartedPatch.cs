using System.Reflection;
using EFT;
using HarmonyLib;
using MOAR.Components;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Ensures the BotZoneRenderer component is attached to the GameWorld root object when a raid starts.
    /// Used for visualizing bot spawn zones during debugging.
    /// </summary>
    public sealed class OnGameStartedPatch : ModulePatch
    {
        /// <summary>
        /// Targets GameWorld.OnGameStarted method to attach BotZoneRenderer.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        /// <summary>
        /// Adds the BotZoneRenderer component if not already present and overlay is enabled.
        /// </summary>
        [PatchPrefix]
        private static void Prefix(GameWorld __instance)
        {
            if (__instance == null || __instance.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[OnGameStartedPatch] GameWorld instance is null.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] Point overlay is disabled. Skipping renderer attach.");
                return;
            }

            if (!__instance.TryGetComponent<BotZoneRenderer>(out _))
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] BotZoneRenderer added to GameWorld.");
            }
            else
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] BotZoneRenderer already present. Skipping.");
            }
        }
    }
}
