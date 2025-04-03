using System.Reflection;
using EFT;
using HarmonyLib;
using MOAR.Components;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Patch that ensures a BotZoneRenderer is attached to each BotZone when it awakens.
    /// Used for debug visualization of bot spawn zones.
    /// </summary>
    public class BotZoneAwakePatch : ModulePatch
    {
        /// <summary>
        /// Targets the BotZone.Awake method for injection.
        /// </summary>
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(BotZone), nameof(BotZone.Awake));

        /// <summary>
        /// Postfix logic that adds BotZoneRenderer if not already present.
        /// </summary>
        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BotZone __instance)
        {
            try
            {
                if (__instance?.gameObject?.GetComponent<BotZoneRenderer>() == null)
                {
                    __instance.gameObject.AddComponent<BotZoneRenderer>();
#if DEBUG
                    Plugin.LogSource?.LogDebug($"[BotZoneAwakePatch] Renderer added to BotZone: {__instance.NameZone}");
#endif
                }
            }
            catch (System.Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[BotZoneAwakePatch] Failed to add renderer: {ex.Message}");
            }
        }
    }
}
