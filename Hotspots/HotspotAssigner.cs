using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EFT;
using MOAR.Hotspots;
using Comfort.Common;

namespace MOAR.Hotspots
{
    public static class HotspotAssigner
    {
        private static readonly Dictionary<string, Dictionary<int, HotspotLocation>> _assignedHotspots = new();

        public static void AssignHotspotToBot(BotOwner botOwner)
        {
            if (botOwner == null || botOwner.Profile == null || botOwner.BotsGroup == null)
                return;

            string mapName = Singleton<GameWorld>.Instance?.MainPlayer?.Location?.ToLowerInvariant() ?? "unknown";
            int teamId = botOwner.BotsGroup.GroupId;
            BotSide side = DetermineSide(botOwner);

            // Init map record
            if (!_assignedHotspots.ContainsKey(mapName))
                _assignedHotspots[mapName] = new Dictionary<int, HotspotLocation>();

            var assigned = _assignedHotspots[mapName];

            if (!assigned.TryGetValue(teamId, out var hotspot))
            {
                var available = HotspotLoader.GetHotspotsForTeam(mapName, side, teamId)
                    .Where(h => !assigned.Values.Contains(h)) // prevent duplicates across teams
                    .ToList();

                if (available.Count == 0)
                {
                    Debug.LogWarning($"[MOAR] No available hotspots for team {teamId} on {mapName}");
                    return;
                }

                hotspot = available[Random.Range(0, available.Count)];
                assigned[teamId] = hotspot;
            }

            botOwner.Memory?.SetTargetZone(hotspot.positions[Random.Range(0, hotspot.positions.Count)]); // Replace with your logic
        }

        private static BotSide DetermineSide(BotOwner botOwner)
        {
            return botOwner.Profile.Info.Side switch
            {
                EPlayerSide.Bear or EPlayerSide.Usec => BotSide.PMC,
                EPlayerSide.Savage => BotSide.Scav,
                _ => BotSide.Any
            };
        }
    }
}
