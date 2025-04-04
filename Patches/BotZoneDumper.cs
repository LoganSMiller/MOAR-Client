using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using Fika.Core.Coop.Utils;

namespace MOAR.Patches
{
    /// <summary>
    /// Dumps all BotZone data to the log when a map loads.
    /// Useful for debugging spawn zone IDs and placement across all clients/hosts.
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
                Plugin.LogSource.LogDebug("[BotZoneDumper] Skipped zone dump (debug mode is off).");
                return;
            }

            if (__instance?.BotZones == null || __instance.BotZones.Length == 0)
            {
                Plugin.LogSource.LogWarning("[BotZoneDumper] No BotZones found in LocationScene.");
                return;
            }

            string context = FikaBackendUtils.IsServer ? "[Headless Host]" : "[Client]";
            Plugin.LogSource.LogInfo($"{context} [BotZoneDumper] Dumping {__instance.BotZones.Length} BotZones:");

            foreach (var botZone in __instance.BotZones)
            {
                if (botZone == null)
                    continue;

                string zoneName = !string.IsNullOrWhiteSpace(botZone.NameZone) ? botZone.NameZone : "Unnamed";
                Plugin.LogSource.LogInfo($"{context} [BotZoneDumper] BotZone: '{zoneName}' | ID: {botZone.Id}");
            }
        }
    }
}
