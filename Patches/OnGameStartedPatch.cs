using System.Reflection;
using EFT;
using HarmonyLib;
using MOAR.Components;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Injects a BotZoneRenderer into the GameWorld on game start for runtime zone visualization and debug purposes.
    /// </summary>
    public sealed class OnGameStartedPatch : ModulePatch
    {
        /// <summary>
        /// Targets GameWorld.OnGameStarted method to attach BotZoneRenderer.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        /// <summary>
        /// Adds the BotZoneRenderer component if not already present.
        /// </summary>
        /// <param name="__instance">The current GameWorld instance.</param>
        [PatchPrefix]
        private static void Prefix(GameWorld __instance)
        {
            if (__instance == null || __instance.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[OnGameStartedPatch] GameWorld instance was null.");
                return;
            }

            if (!__instance.TryGetComponent<BotZoneRenderer>(out _))
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] BotZoneRenderer successfully added to GameWorld.");
            }
            else
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] GameWorld already contains BotZoneRenderer. Skipping.");
            }
        }
    }
}
