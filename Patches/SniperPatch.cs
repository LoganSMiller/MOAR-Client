using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EFT.Game.Spawning;
using HarmonyLib;
using MOAR.Helpers;
using SPT.Reflection.Patching;
using UnityEngine;

namespace MOAR.Patches
{
    /// <summary>
    /// Reassigns sniper spawn points to valid BotZones after the spawn point manager initializes.
    /// Ensures custom sniper zones are respected or fallback regular zones are used.
    /// </summary>
    public sealed class SniperPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod() =>
            AccessTools.Method(typeof(SpawnPointManagerClass), "smethod_1");

        /// <summary>Returns the closest or randomly selected valid bot zone.</summary>
        private static BotZone? GetNearestZone(List<BotZone> zones, string fallbackName)
        {
            return zones.FirstOrDefault(z => z?.NameZone == fallbackName && !z.IsNullOrDestroyed()) ??
                   zones.Where(z => z != null && !z.IsNullOrDestroyed())
                        .OrderBy(_ => Guid.NewGuid())
                        .FirstOrDefault();
        }

        /// <summary>Determines if a zone name fits sniper naming convention.</summary>
        private static bool IsSnipeZoneName(string name) =>
            !string.IsNullOrWhiteSpace(name) && name.ToLowerInvariant().Contains("custom_snipe");

        /// <summary>Finds the original zone name assigned by ID.</summary>
        private static string GetBotZoneNameById(SpawnPointParams[] points, string id)
        {
            foreach (var point in points)
            {
                if (point.Id == id)
                {
                    return point.BotZoneName ?? string.Empty;
                }
            }

            return string.Empty;
        }

        /// <summary>Updates bot zone name in-place via struct reassignment.</summary>
        private static void SetBotZoneName(SpawnPointParams[] points, string id, string newName)
        {
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i].Id == id)
                {
                    var updated = points[i];
                    updated.BotZoneName = newName;
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

                // Skip already valid zones
                if (marker.BotZone != null && !marker.BotZone.IsNullOrDestroyed())
                    continue;

                var zoneName = GetBotZoneNameById(parameters, marker.Id);
                bool isSniper = IsSnipeZoneName(zoneName);

                if (!isSniper || !snipeZones.Any(z => z.NameZone == zoneName))
                {
                    var fallback = GetNearestZone(regularZones, zoneName);
                    if (fallback != null)
                    {
                        AccessTools.Field(typeof(BotZone), "_maxPersons").SetValue(fallback, -1);
                        marker.BotZone = fallback;
                        Plugin.LogSource.LogDebug($"[SniperPatch] Fallback regular zone '{fallback.NameZone}' assigned to '{marker.Id}'.");
                    }
                }
                else
                {
                    SetBotZoneName(parameters, marker.Id, string.Empty);

                    var sniperZone = GetNearestZone(snipeZones, zoneName);
                    if (sniperZone != null)
                    {
                        int newMax = sniperZone.MaxPersons > 0 ? sniperZone.MaxPersons + 1 : 5;
                        AccessTools.Field(typeof(BotZone), "_maxPersons").SetValue(sniperZone, newMax);
                        marker.BotZone = sniperZone;
                        Plugin.LogSource.LogDebug($"[SniperPatch] Sniper zone '{sniperZone.NameZone}' assigned to '{marker.Id}'.");
                    }
                }
            }

            Plugin.LogSource.LogInfo("[SniperPatch] Sniper zone assignment complete.");
        }
    }
}
