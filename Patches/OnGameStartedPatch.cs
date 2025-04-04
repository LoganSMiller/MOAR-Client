using System.Reflection;
using EFT;
using HarmonyLib;
using MOAR.Components;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;
using Fika.Core.Coop.Utils;

namespace MOAR.Patches
{
    /// <summary>
    /// Ensures a BotZoneRenderer is attached to the GameWorld root object when a raid begins.
    /// Enables visualization of bot spawn zones, useful for both local and headless debugging.
    /// </summary>
    public sealed class OnGameStartedPatch : ModulePatch
    {
        /// <summary>
        /// Targets GameWorld.OnGameStarted to inject component setup.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        /// <summary>
        /// Adds BotZoneRenderer to the GameWorld root object if overlay is enabled.
        /// Avoids redundant attachment and respects headless/server environment.
        /// </summary>
        [PatchPrefix]
        private static void Prefix(GameWorld __instance)
        {
            if (__instance == null || __instance.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[OnGameStartedPatch] GameWorld is null. Cannot attach BotZoneRenderer.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] Point overlay disabled in config. Renderer will not be attached.");
                return;
            }

            if (__instance.TryGetComponent<BotZoneRenderer>(out _))
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] BotZoneRenderer already exists. Skipping.");
                return;
            }

            __instance.gameObject.AddComponent<BotZoneRenderer>();
            Plugin.LogSource?.LogInfo($"[OnGameStartedPatch] BotZoneRenderer attached to GameWorld " +
                $"(headless: {FikaBackendUtils.IsServer}).");
        }
    }
}
