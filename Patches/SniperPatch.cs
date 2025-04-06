using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;
using MOAR.Helpers;

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
        /// Returns the closest or randomly selected valid bot zone.
        /// </summary>
        private static BotZone GetNearestZone(List<BotZone> zones, string fallbackName)
        {
            return zones.FirstOrDefault(z => z?.NameZone == fallbackName && !z.IsNullOrDestroyed()) ??
                   zones.Where(z => z != null && !z.IsNullOrDestroyed())
                        .OrderBy(_ => Guid.NewGuid()) // Random fallback if no named match
                        .FirstOrDefault();
        }

        /// <summary>
        /// Determines if a zone name matches a sniper zone naming convention.
        /// </summary>
        private static bool IsSnipeZoneName(string name) =>
            !string.IsNullOrWhiteSpace(name) && name.ToLowerInvariant().Contains("custom_snipe");

        /// <summary>
        /// Retrieves the original bot zone name for a spawn point by ID.
        /// </summary>
        private static string GetBotZoneNameById(SpawnPointParams[] spawnPoints, string id)
        {
            foreach (var point in spawnPoints)
            {
                if (point.Id == id)
                    return point.BotZoneName ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Updates the bot zone name for a specific spawn point ID.
        /// </summary>
        private static void SetBotZoneName(SpawnPointParams[] spawnPoints, string id, string newName)
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i].Id == id)
                {
                    var updated = spawnPoints[i];
                    updated.BotZoneName = newName;
                    spawnPoints[i] = updated; // Struct reassignment required
                    return;
                }
            }
        }

        /// <summary>
        /// Reassigns sniper points to valid BotZones after spawn points are built.
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

            if (snipeZones.Count == 0 && regularZones.Count == 0)
            {
                Plugin.LogSource.LogWarning("[SniperPatch] No valid bot zones found. Skipping reassignment.");
                return;
            }

            foreach (var marker in __result)
            {
                if (marker == null ||
                    marker.SpawnPoint.Categories == ESpawnCategoryMask.None ||
                    marker.SpawnPoint.Categories.ContainPlayerCategory())
                    continue;

                // Skip if marker already has a valid zone
                if (marker.BotZone != null && !marker.BotZone.IsNullOrDestroyed())
                    continue;

                string zoneName = GetBotZoneNameById(parameters, marker.Id);
                bool isSnipeZone = IsSnipeZoneName(zoneName);

                if (!isSnipeZone || !snipeZones.Any(z => z.NameZone == zoneName))
                {
                    var fallback = GetNearestZone(regularZones, zoneName);
                    if (fallback != null)
                    {
                        AccessTools.Field(typeof(BotZone), "_maxPersons").SetValue(fallback, -1);
                        marker.BotZone = fallback;

                        Plugin.LogSource.LogDebug($"[SniperPatch] Assigned fallback regular zone '{fallback.NameZone}' to spawn point '{marker.Id}'");
                    }
                }
                else
                {
                    SetBotZoneName(parameters, marker.Id, string.Empty); // Clear old name if invalid

                    var sniperZone = GetNearestZone(snipeZones, zoneName);
                    if (sniperZone != null)
                    {
                        int newMax = sniperZone.MaxPersons > 0 ? sniperZone.MaxPersons + 1 : 5;
                        AccessTools.Field(typeof(BotZone), "_maxPersons").SetValue(sniperZone, newMax);
                        marker.BotZone = sniperZone;

                        Plugin.LogSource.LogDebug($"[SniperPatch] Assigned sniper zone '{sniperZone.NameZone}' to spawn point '{marker.Id}'");
                    }
                }
            }

            Plugin.LogSource.LogInfo("[SniperPatch] BotZone reassignment complete.");
        }
    }
}
