using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;

namespace MOAR.Patches
{
    /// <summary>
    /// Dumps all BotZone data to the log when a map loads.
    /// Useful for debugging spawn zone IDs and placement.
    /// </summary>
    public class BotZoneDumper : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(LocationScene), nameof(LocationScene.Awake));

        [PatchPostfix]
        private static void Postfix(LocationScene __instance)
        {
            if (!Settings.debug.Value)
            {
                Plugin.LogSource.LogDebug("[BotZoneDumper] Skipped zone dump (debug disabled).");
                return;
            }

            if (__instance?.BotZones == null || __instance.BotZones.Length == 0)
            {
                Plugin.LogSource.LogWarning("[BotZoneDumper] No BotZones found in current LocationScene.");
                return;
            }

            Plugin.LogSource.LogInfo($"[BotZoneDumper] Dumping {__instance.BotZones.Length} BotZones...");

            foreach (var botZone in __instance.BotZones)
            {
                if (botZone == null) continue;

                string zoneName = !string.IsNullOrWhiteSpace(botZone.NameZone) ? botZone.NameZone : "Unnamed";
                Plugin.LogSource.LogInfo($"[BotZoneDumper] BotZone: '{zoneName}' | ID: {botZone.Id}");
            }
        }
    }
}
