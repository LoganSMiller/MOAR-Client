using System;
using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using Fika.Core.Coop.Utils;
using UnityEngine;

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
            try
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

                string context = FikaBackendUtils.IsHeadless ? "[Headless Host]" :
                                 FikaBackendUtils.IsServer ? "[FIKA Server]" :
                                 "[Client]";

                Plugin.LogSource.LogInfo($"{context} [BotZoneDumper] Dumping {__instance.BotZones.Length} BotZones:");

                foreach (var botZone in __instance.BotZones)
                {
                    if (botZone == null)
                    {
                        Plugin.LogSource.LogWarning($"{context} [BotZoneDumper] Skipping null BotZone.");
                        continue;
                    }

                    string zoneName = "Unnamed";
                    string id = "Unknown";
                    string pos = "N/A";

                    try
                    {
                        zoneName = string.IsNullOrWhiteSpace(botZone.NameZone) ? "Unnamed" : botZone.NameZone;
                    }
                    catch { }

                    try
                    {
                        id = botZone.Id.ToString();
                    }
                    catch { id = "Unknown"; }


                    try
                    {
                        if (botZone.transform != null)
                        {
                            var t = botZone.transform;
                            pos = $"[{t.position.x:F1}, {t.position.y:F1}, {t.position.z:F1}]";
                        }
                    }
                    catch { }

                    Plugin.LogSource.LogInfo($"{context} [BotZoneDumper] Zone: '{zoneName}' | ID: {id} | Pos: {pos}");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[BotZoneDumper] Exception during BotZone dump: {ex}");
            }
        }
    }
}
