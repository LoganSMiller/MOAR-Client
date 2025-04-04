using System.Reflection;
using EFT;
using HarmonyLib;
using MOAR.Components;
using MOAR.Helpers; // ✅ Reference to static Settings class
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Ensures that BotZoneRenderer is added to the GameWorld on raid start.
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
            if (__instance == null)
                return;
            }

            if (!__instance.TryGetComponent<BotZoneRenderer>(out _))
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource.LogDebug("[OnGameStartedPatch] BotZoneRenderer added to GameWorld.");
            }
        }
    }
}
