using System.Reflection;
using EFT.Game.Spawning;
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
        /// Adds a BotZoneRenderer component to the BotZone if not already present.
        /// </summary>
        [PatchPostfix]
        [HarmonyPriority(Priority.Last)]
        private static void Postfix(BotZone __instance)
        {
            if (__instance != null && __instance.GetComponent<BotZoneRenderer>() == null)
            {
                __instance.gameObject.AddComponent<BotZoneRenderer>();
            }
        }
    }
}
