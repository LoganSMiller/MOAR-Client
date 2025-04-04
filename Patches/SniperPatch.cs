using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Patch for properly assigning sniper spawn points to valid bot zones during spawn point initialization.
    /// Ensures custom sniper zones are respected or fallback logic is used.
    /// </summary>
    public class SniperPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(SpawnPointManagerClass), "smethod_1");

        /// <summary>
        /// Attempts to find a fallback zone from the list of valid zones.
        /// </summary>
        private static BotZone GetNearestZone(List<BotZone> zones, string fallbackName)
        {
            return zones.FirstOrDefault(z => z.NameZone == fallbackName)
                ?? zones[UnityEngine.Random.Range(0, zones.Count)];
        }

        /// <summary>
        /// Returns true if the zone name suggests a sniper zone.
        /// </summary>
        private static bool IsSnipeZoneName(string name) =>
            !string.IsNullOrEmpty(name) && name.ToLowerInvariant().Contains("custom_snipe");

        /// <summary>
        /// Finds the bot zone name for a given spawn point ID.
        /// </summary>
        private static string GetBotZoneNameById(SpawnPointParams[] spawnPoints, string id)
        {
            foreach (var point in spawnPoints)
            {
                if (point.Id == id)
                    return point.BotZoneName;
            }
            return string.Empty;
        }

        /// <summary>
        /// Reassigns the bot zone name of a specific spawn point.
        /// </summary>
        private static void SetBotZoneName(SpawnPointParams[] spawnPoints, string id, string newName)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].Id == id)
                {
                    var updated = spawnPoints[i];
                    updated.BotZoneName = newName;
                    spawnPoints[i] = updated; // structs must be reassigned
                    return;
                }
            }
        }

        /// <summary>
        /// Postfix that reassigns sniper points to valid BotZones after spawn points are built.
        /// </summary>
        [PatchPostfix]
        private static void Postfix(ref SpawnPointMarker[] __result, SpawnPointParams[] parameters)
        {
            if (__result == null || parameters == null || __result.Length == 0 || parameters.Length == 0)
            {
                Plugin.LogSource.LogInfo("[SniperPatch] No spawn points to process.");
                return;
            }

            var snipeZones = new List<BotZone>();
            var regularZones = new List<BotZone>();

            foreach (var marker in __result)
            {
                if (marker?.BotZone == null || marker.BotZone.IsNullOrDestroyed())
                    continue;

                if (marker.BotZone.SnipeZone)
                    snipeZones.Add(marker.BotZone);
                else
                    regularZones.Add(marker.BotZone);
            }

            if (snipeZones.Count == 0 || regularZones.Count == 0)
            {
                Plugin.LogSource.LogInfo("[SniperPatch] No valid snipe or regular zones found.");
                return;
            }

            foreach (var marker in __result)
            {
                if (marker == null ||
                    marker.SpawnPoint.Categories == ESpawnCategoryMask.None ||
                    marker.SpawnPoint.Categories.ContainPlayerCategory())
                    continue;

                if (marker.BotZone != null && !marker.BotZone.IsNullOrDestroyed())
                    continue;

                string zoneName = GetBotZoneNameById(parameters, marker.Id);

                if (!snipeZones.Any(z => z.NameZone == zoneName) && !IsSnipeZoneName(zoneName))
                {
                    // Fallback to regular zone
                    var fallback = GetNearestZone(regularZones, zoneName);
                    AccessTools.Field(typeof(BotZone), "_maxPersons").SetValue(fallback, -1);
                    marker.BotZone = fallback;
                }
                else
                {
                    // Assign to sniper zone
                    if (IsSnipeZoneName(zoneName))
                        SetBotZoneName(parameters, marker.Id, string.Empty);

                    var sniperZone = GetNearestZone(snipeZones, zoneName);
                    int newMax = sniperZone.MaxPersons > 0 ? sniperZone.MaxPersons + 1 : 5;
                    AccessTools.Field(typeof(BotZone), "_maxPersons").SetValue(sniperZone, newMax);
                    marker.BotZone = sniperZone;
                }
            }

            Plugin.LogSource.LogInfo("[SniperPatch] BotZone reassignment complete.");
        }
    }
}
