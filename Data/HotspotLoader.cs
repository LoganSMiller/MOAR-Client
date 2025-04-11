using System.Collections.Generic;
using System.IO;
using System.Linq;
using EFT;
using Newtonsoft.Json;
using UnityEngine;

namespace MOAR.Hotspots
{
    public class HotspotLoader
    {
        private static readonly string HotspotDirectory = Path.Combine(BepInEx.Paths.ConfigPath, "MOAR", "hotspots");

        private static Dictionary<string, List<HotspotLocation>> _hotspotsByMap = new();

        public static void Init()
        {
            LoadAllHotspots();
        }

        public static void Reload()
        {
            _hotspotsByMap.Clear();
            LoadAllHotspots();
        }

        private static void LoadAllHotspots()
        {
            if (!Directory.Exists(HotspotDirectory))
            {
                Directory.CreateDirectory(HotspotDirectory);
                return;
            }

            foreach (var file in Directory.GetFiles(HotspotDirectory, "*.json"))
            {
                var mapName = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                var json = File.ReadAllText(file);

                try
                {
                    var hotspots = JsonConvert.DeserializeObject<List<HotspotLocation>>(json);
                    if (hotspots != null)
                        _hotspotsByMap[mapName] = hotspots;
                }
                catch
                {
                    Debug.LogError($"[MOAR] Failed to parse hotspot file: {file}");
                }
            }
        }

        public static List<HotspotLocation> GetHotspotsForMap(string mapName, BotSide side = BotSide.Any)
        {
            mapName = mapName.ToLowerInvariant();

            if (!_hotspotsByMap.TryGetValue(mapName, out var allHotspots))
                return new List<HotspotLocation>();

            return allHotspots
                .Where(h => side == BotSide.Any || h.AllowedSides.Contains(side))
                .ToList();
        }

        public static List<HotspotLocation> GetHotspotsForTeam(string mapName, BotSide side, int teamId)
        {
            // You could use teamId here in future to coordinate group-based assignments.
            return GetHotspotsForMap(mapName, side);
        }
    }

    public enum BotSide
    {
        Any,
        PMC,
        Scav
    }

    public class HotspotLocation
    {
        public string name;
        public List<Vector3> positions;

        [JsonIgnore]
        public List<BotSide> AllowedSides => pmcsOnly ? new List<BotSide> { BotSide.PMC } : new List<BotSide> { BotSide.PMC, BotSide.Scav };

        public bool pmcsOnly = false;

        public override string ToString()
        {
            return $"{name} [{positions.Count} points]";
        }
    }
}
