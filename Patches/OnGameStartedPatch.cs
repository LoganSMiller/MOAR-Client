using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using Fika.Core.Coop.Utils;
using HarmonyLib;
using MOAR.Components;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Attaches a BotZoneRenderer to the GameWorld if enabled and in a compatible environment.
    /// Prevents execution in FIKA headless mode and skips duplicate attachment.
    /// </summary>
    public sealed class OnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPrefix]
        private static void Prefix(GameWorld __instance)
        {
            if (__instance?.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[OnGameStartedPatch] GameWorld or GameObject was null. Skipping.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] Overlay disabled in config.");
                return;
            }

            if (Settings.IsFika && FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] Skipping overlay — FIKA headless mode.");
                return;
            }

            if (__instance.GetComponent<BotZoneRenderer>() != null)
            {
                Plugin.LogSource?.LogDebug("[OnGameStartedPatch] BotZoneRenderer already present.");
                return;
            }

            try
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource?.LogInfo("[OnGameStartedPatch] BotZoneRenderer attached successfully.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[OnGameStartedPatch] Failed to attach BotZoneRenderer: {ex}");
            }
        }
    }
}
