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
    /// Skips execution in headless mode or when overlay is disabled.
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
                Plugin.LogSource?.LogWarning("[BotZoneAwakePatch] BotZone or its GameObject is null — skipping.");
                return;
            }

            if (FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource?.LogDebug("[BotZoneAwakePatch] Skipped — FIKA headless mode active.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[BotZoneAwakePatch] Skipped — Point overlay disabled in config.");
                return;
            }

            try
            {
                string zoneName = "[Unknown]";
                try { zoneName = __instance.NameZone ?? "[Unnamed]"; } catch { }

                if (!__instance.TryGetComponent<BotZoneRenderer>(out _))
                {
                    __instance.gameObject.AddComponent<BotZoneRenderer>();
                    Plugin.LogSource?.LogInfo($"[BotZoneAwakePatch] Attached BotZoneRenderer to zone: \"{zoneName}\"");
                }
                else
                {
                    Plugin.LogSource?.LogDebug($"[BotZoneAwakePatch] BotZoneRenderer already exists on zone: \"{zoneName}\"");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[BotZoneAwakePatch] Exception while attaching BotZoneRenderer: {ex}");
            }
        }
    }
}
