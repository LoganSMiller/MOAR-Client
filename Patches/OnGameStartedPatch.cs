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
    /// Allows visualization of spawn zones for debugging or live adjustment.
    /// </summary>
    public sealed class OnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPrefix]
        private static void Prefix(GameWorld __instance)
        {
            try
            {
                if (__instance == null || __instance.gameObject == null)
                {
                    Plugin.LogSource?.LogWarning("[OnGameStartedPatch] GameWorld or GameObject is null. Skipping overlay attach.");
                    return;
                }

                if (!Settings.enablePointOverlay.Value)
                {
                    Plugin.LogSource?.LogDebug("[OnGameStartedPatch] Point overlay disabled in config.");
                    return;
                }

                if (Settings.IsFika && FikaBackendUtils.IsHeadless)
                {
                    Plugin.LogSource?.LogDebug("[OnGameStartedPatch] Skipping overlay in FIKA headless mode.");
                    return;
                }

                if (__instance.GetComponent<BotZoneRenderer>() != null)
                {
                    Plugin.LogSource?.LogDebug("[OnGameStartedPatch] BotZoneRenderer already attached. No action taken.");
                    return;
                }

                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource?.LogInfo("[OnGameStartedPatch] BotZoneRenderer successfully attached to GameWorld.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[OnGameStartedPatch] Exception while attaching overlay: {ex}");
            }
        }
    }
}
