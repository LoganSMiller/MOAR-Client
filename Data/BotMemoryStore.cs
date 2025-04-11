using System.Collections.Generic;
using UnityEngine;

namespace MOAR.Data
{
    public static class BotMemoryStore
    {
        private static readonly Dictionary<string, List<Vector3>> DangerZonesByMap = new();

        public static void AddDangerZone(string mapId, Vector3 position)
        {
            if (!DangerZonesByMap.ContainsKey(mapId))
                DangerZonesByMap[mapId] = new List<Vector3>();

            if (!DangerZonesByMap[mapId].Exists(pos => Vector3.Distance(pos, position) < 3f))
                DangerZonesByMap[mapId].Add(position);
        }

        public static List<Vector3> GetDangerZones(string mapId)
        {
            return DangerZonesByMap.TryGetValue(mapId, out var zones)
                ? zones
                : new List<Vector3>();
        }

        public static void ClearAll() => DangerZonesByMap.Clear();
    }
}
