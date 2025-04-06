using System;
using System.Reflection;
using EFT.Game.Spawning;
using Fika.Core.Coop.Utils;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Dumps all BotZone data to the log during LocationScene.Awake.
    /// Useful for spawn zone debugging across hosts, clients, and headless.
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
                Plugin.LogSource.LogDebug("[BotZoneDumper] Debug is disabled — skipping BotZone dump.");
                return;
            }

            try
            {
                if (__instance?.BotZones == null || __instance.BotZones.Length == 0)
                {
                    Plugin.LogSource.LogWarning("[BotZoneDumper] No BotZones found in scene.");
                    return;
                }

                string ctx = FikaBackendUtils.IsHeadless ? "[Headless Host]" :
                             FikaBackendUtils.IsServer ? "[FIKA Server]" :
                             FikaBackendUtils.IsClient ? "[FIKA Client]" :
                             "[SPT Offline]";

                Plugin.LogSource.LogInfo($"{ctx} [BotZoneDumper] Dumping {__instance.BotZones.Length} BotZones...");

                foreach (var zone in __instance.BotZones)
                {
                    if (zone == null)
                    {
                        Plugin.LogSource.LogWarning($"{ctx} [BotZoneDumper] Skipped null BotZone.");
                        continue;
                    }

                    string name = "[Unnamed]";
                    string id = "[No ID]";
                    string pos = "[No Position]";

                    try { name = string.IsNullOrWhiteSpace(zone.NameZone) ? "[Unnamed]" : zone.NameZone; } catch { }
                    try {
                        id = zone.Id.ToString();
                    }
                    catch { }
                    try
                    {
                        if (zone.transform != null)
                        {
                            Vector3 p = zone.transform.position;
                            pos = $"[{p.x:F1}, {p.y:F1}, {p.z:F1}]";
                        }
                    }
                    catch { }

                    Plugin.LogSource.LogInfo($"{ctx} [BotZoneDumper] Zone: \"{name}\" | ID: {id} | Pos: {pos}");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[BotZoneDumper] Exception during zone dump: {ex}");
            }
        }
    }
}
