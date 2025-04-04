using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Components;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;
using Fika.Core.Coop.Utils;

namespace MOAR.Patches
{
    /// <summary>
    /// Ensures every BotZone has a BotZoneRenderer component when it awakens.
    /// Used to render spawn zone outlines for debugging, even during headless sessions.
    /// </summary>
    public class BotZoneAwakePatch : ModulePatch
    {
        /// <summary>
        /// Targets the BotZone.Awake method.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(BotZone), nameof(BotZone.Awake));

        /// <summary>
        /// Postfix logic that attaches BotZoneRenderer to each BotZone if debugging is enabled.
        /// </summary>
        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BotZone __instance)
        {
            if (__instance == null || __instance.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[BotZoneAwakePatch] BotZone or GameObject is null. Skipping renderer attach.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[BotZoneAwakePatch] Overlay disabled in config. Skipping renderer attach.");
                return;
            }

            if (!__instance.TryGetComponent<BotZoneRenderer>(out _))
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
                Plugin.LogSource?.LogDebug($"[BotZoneAwakePatch] BotZoneRenderer added to '{__instance.NameZone}' (headless: {FikaBackendUtils.IsServer})");
            }
            else
            {
                Plugin.LogSource?.LogDebug($"[BotZoneAwakePatch] BotZoneRenderer already exists on '{__instance.NameZone}'.");
            }
        }
    }
}
