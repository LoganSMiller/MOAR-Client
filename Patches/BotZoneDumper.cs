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
    /// Dumps all BotOwnerZone data to the log during LocationScene.Awake.
    /// Useful for spawn zone debugging across host, client, and headless modes.
    /// </summary>
    public class BotOwnerZoneDumper : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(LocationScene), nameof(LocationScene.Awake));

        [PatchPostfix]
        private static void Postfix(LocationScene __instance)
        {
            if (!Settings.debug.Value)
            {
                Plugin.LogSource.LogDebug("[BotOwnerZoneDumper] Debug disabled — skipping zone dump.");
                return;
            }

            try
            {
                var zones = __instance?.BotOwnerZones;
                if (zones == null || zones.Length == 0)
                {
                    Plugin.LogSource.LogWarning("[BotOwnerZoneDumper] No BotOwnerZones found in current scene.");
                    return;
                }

                string ctx = FikaBackendUtils.IsHeadless ? "[Headless Host]" :
                             FikaBackendUtils.IsServer ? "[FIKA Server]" :
                             FikaBackendUtils.IsClient ? "[FIKA Client]" :
                             "[SPT Offline]";

                Plugin.LogSource.LogInfo($"{ctx} [BotOwnerZoneDumper] Dumping {zones.Length} BotOwnerZones...");

                foreach (var zone in zones)
                {
                    if (zone == null)
                    {
                        Plugin.LogSource.LogWarning($"{ctx} [BotOwnerZoneDumper] Skipped null BotOwnerZone.");
                        continue;
                    }

                    string name = zone.NameZone ?? "[Unnamed]";
                    string id = zone.Id != 0 ? zone.Id.ToString() : "[No ID]";
                    string pos = zone.transform != null
                        ? $"[{zone.transform.position.x:F1}, {zone.transform.position.y:F1}, {zone.transform.position.z:F1}]"
                        : "[No Position]";

                    Plugin.LogSource.LogInfo($"{ctx} [BotOwnerZoneDumper] Zone: \"{name}\" | ID: {id} | Pos: {pos}");
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[BotOwnerZoneDumper] Exception while dumping zones:\n{ex}");
            }
        }
    }
}
