using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Logs all BotZones on map load. Useful for debugging spawn zone placement and IDs.
    /// </summary>
    public class BotZoneDumper : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(LocationScene), nameof(LocationScene.Awake));

        [PatchPostfix]
        public static void Postfix(LocationScene __instance)
        {
            if (__instance?.BotZones == null || __instance.BotZones.Length == 0)
            {
                if (Settings.debug.Value)
                    Plugin.LogSource.LogWarning("[BotZoneDumper] No BotZones found on this map.");
                return;
            }

            if (!Settings.debug.Value)
            {
                Plugin.LogSource.LogDebug("[BotZoneDumper] Skipped zone dump (debug disabled).");
                return;
            }

            foreach (var botZone in __instance.BotZones)
            {
                if (botZone == null)
                    continue;

                Plugin.LogSource.LogInfo($"[BotZoneDumper] BotZone: '{botZone.NameZone}' | ID: {botZone.Id}");
            }
        }
    }
}
