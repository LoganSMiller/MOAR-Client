using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;
using MOAR.AI;

namespace MOAR.Patches
{
    /// <summary>
    /// Reassigns sniper spawn points to valid BotOwnerZones after initialization.
    /// Ensures sniper zones are respected or falls back to usable zones if needed.
    /// </summary>
    public sealed class SniperPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(SpawnPointManagerClass), "smethod_1");

        private static BotOwnerZone? GetNearestZone(List<BotOwnerZone> zones, string fallbackName)
        {
            return zones.FirstOrDefault(z => z?.NameZone == fallbackName && !z.IsNullOrDestroyed()) ??
                   zones.Where(z => z != null && !z.IsNullOrDestroyed())
                        .OrderBy(_ => Guid.NewGuid())
                        .FirstOrDefault();
        }

        private static bool IsSnipeZoneName(string name) =>
            !string.IsNullOrWhiteSpace(name) && name.ToLowerInvariant().Contains("custom_snipe");

        private static string GetBotOwnerZoneNameById(SpawnPointParams[] points, string id)
        {
            foreach (var point in points)
            {
                if (point.Id == id)
                {
                    return point.BotOwnerZoneName ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static void SetBotOwnerZoneName(SpawnPointParams[] points, string id, string newName)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Id == id)
                {
                    var updated = points[i];
                    updated.BotOwnerZoneName = newName;
                    points[i] = updated;
                    return;
                }
            }
        }

        [PatchPostfix]
        private static void Postfix(ref SpawnPointMarker[] __result, SpawnPointParams[] parameters)
        {
            if (__result == null || parameters == null || __result.Length == 0 || parameters.Length == 0)
            {
                Plugin.LogSource.LogInfo("[SniperPatch] No spawn points to process.");
                return;
            }

            var snipeZones = new List<BotOwnerZone>();
            var regularZones = new List<BotOwnerZone>();

            foreach (var marker in __result)
            {
                if (marker?.BotOwnerZone == null || marker.BotOwnerZone.IsNullOrDestroyed())
                    continue;

                if (marker.BotOwnerZone.SnipeZone)
                    snipeZones.Add(marker.BotOwnerZone);
                else
                    regularZones.Add(marker.BotOwnerZone);
            }

            if (snipeZones.Count == 0 && regularZones.Count == 0)
            {
                Plugin.LogSource.LogWarning("[SniperPatch] No valid BotOwner zones found. Skipping reassignment.");
                return;
            }

            foreach (var marker in __result)
            {
                if (marker == null ||
                    marker.SpawnPoint.Categories == ESpawnCategoryMask.None ||
                    marker.SpawnPoint.Categories.ContainPlayerCategory())
                    continue;

                // Skip valid zones
                if (marker.BotOwnerZone != null && !marker.BotOwnerZone.IsNullOrDestroyed())
                    continue;

                var zoneName = GetBotOwnerZoneNameById(parameters, marker.Id);
                bool isSniper = IsSnipeZoneName(zoneName);

                if (!isSniper || !snipeZones.Any(z => z.NameZone == zoneName))
                {
                    var fallback = GetNearestZone(regularZones, zoneName);
                    if (fallback != null)
                    {
                        AccessTools.Field(typeof(BotOwnerZone), "_maxPersons").SetValue(fallback, -1);
                        marker.BotOwnerZone = fallback;
                        Plugin.LogSource.LogDebug($"[SniperPatch] Fallback regular zone '{fallback.NameZone}' assigned to '{marker.Id}'");
                    }
                }
                else
                {
                    SetBotOwnerZoneName(parameters, marker.Id, string.Empty);

                    var sniperZone = GetNearestZone(snipeZones, zoneName);
                    if (sniperZone != null)
                    {
                        int newMax = sniperZone.MaxPersons > 0 ? sniperZone.MaxPersons + 1 : 5;
                        AccessTools.Field(typeof(BotOwnerZone), "_maxPersons").SetValue(sniperZone, newMax);
                        marker.BotOwnerZone = sniperZone;
                        Plugin.LogSource.LogDebug($"[SniperPatch] Sniper zone '{sniperZone.NameZone}' assigned to '{marker.Id}'");
                    }
                }
            }

            Plugin.LogSource.LogInfo("[SniperPatch] Sniper zone assignment complete.");
        }
    }
}
