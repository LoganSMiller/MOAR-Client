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
    /// Attaches a BotZoneRenderer to the GameWorld if enabled and not in headless FIKA mode.
    /// Ensures that the overlay only exists once and does not run in invalid environments.
    /// </summary>
    public sealed class OnGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));

        [PatchPrefix]
        private static void Prefix(GameWorld __instance)
        {
            if (!Singleton<GameWorld>.Instantiated || __instance == null)
            {
                Plugin.LogSource?.LogWarning($"[{nameof(OnGameStartedPatch)}] GameWorld not instantiated. Skipping.");
                return;
            }

            if (Settings.enablePointOverlay == null || !Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug($"[{nameof(OnGameStartedPatch)}] Point overlay disabled in config.");
                return;
            }

            if (Settings.IsFika && FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource?.LogDebug($"[{nameof(OnGameStartedPatch)}] Skipping overlay — FIKA headless mode.");
                return;
            }

            if (__instance.GetComponent<BotZoneRenderer>() != null)
            {
                Plugin.LogSource?.LogDebug($"[{nameof(OnGameStartedPatch)}] BotZoneRenderer already attached.");
                return;
            }

            try
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource?.LogInfo($"[{nameof(OnGameStartedPatch)}] BotZoneRenderer attached.");
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[{nameof(OnGameStartedPatch)}] Failed to attach BotZoneRenderer: {ex}");
            }
        }
    }
}
