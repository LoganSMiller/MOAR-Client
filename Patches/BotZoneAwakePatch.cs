using System;
using System.Reflection;
using EFT.Game.Spawning;
using Fika.Core.Coop.Utils;
using HarmonyLib;
using MOAR.Components;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Ensures each BotZone attaches a BotZoneRenderer for visual debugging.
    /// Skips in headless mode or if overlay is disabled in config.
    /// Safe for FIKA host, client, headless, and solo SPT.
    /// </summary>
    public class BotZoneAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(BotZone), nameof(BotZone.Awake));

        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BotZone __instance)
        {
            if (__instance?.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[BotZoneAwakePatch] BotZone or GameObject is null — skipping.");
                return;
            }

            if (FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource?.LogDebug("[BotZoneAwakePatch] Skipped in FIKA headless mode.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[BotZoneAwakePatch] Point overlay disabled via config.");
                return;
            }

            try
            {
                string zoneName = __instance.NameZone ?? "[Unnamed]";

                if (!__instance.TryGetComponent<BotZoneRenderer>(out _))
                {
                    __instance.gameObject.AddComponent<BotZoneRenderer>();
                    Plugin.LogSource?.LogInfo($"[BotZoneAwakePatch] BotZoneRenderer attached to zone: \"{zoneName}\"");
                }
                else
                {
                    Plugin.LogSource?.LogDebug($"[BotZoneAwakePatch] Renderer already exists on zone: \"{zoneName}\"");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[BotZoneAwakePatch] Exception while attaching overlay:\n{ex}");
            }
        }
    }
}
