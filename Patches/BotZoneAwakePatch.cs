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
    /// Ensures each BotOwnerZone attaches a BotOwnerZoneRenderer for visual debugging.
    /// Skips in headless mode or if overlay is disabled in config.
    /// Safe for FIKA host, client, headless, and solo SPT.
    /// </summary>
    public class BotOwnerZoneAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(BotOwnerZone), nameof(BotOwnerZone.Awake));

        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BotOwnerZone __instance)
        {
            if (__instance?.gameObject == null)
            {
                Plugin.LogSource?.LogWarning("[BotOwnerZoneAwakePatch] BotOwnerZone or GameObject is null — skipping.");
                return;
            }

            if (FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource?.LogDebug("[BotOwnerZoneAwakePatch] Skipped in FIKA headless mode.");
                return;
            }

            if (!Settings.enablePointOverlay.Value)
            {
                Plugin.LogSource?.LogDebug("[BotOwnerZoneAwakePatch] Point overlay disabled via config.");
                return;
            }

            try
            {
                string zoneName = __instance.NameZone ?? "[Unnamed]";

                if (!__instance.TryGetComponent<BotOwnerZoneRenderer>(out _))
                {
                    __instance.gameObject.AddComponent<BotOwnerZoneRenderer>();
                    Plugin.LogSource?.LogInfo($"[BotOwnerZoneAwakePatch] BotOwnerZoneRenderer attached to zone: \"{zoneName}\"");
                }
                else
                {
                    Plugin.LogSource?.LogDebug($"[BotOwnerZoneAwakePatch] Renderer already exists on zone: \"{zoneName}\"");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[BotOwnerZoneAwakePatch] Exception while attaching overlay:\n{ex}");
            }
        }
    }
}
