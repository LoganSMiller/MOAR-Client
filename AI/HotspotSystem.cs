using System.Collections.Generic;
using System.IO;
using System.Linq;
using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using UnityEngine;

namespace MOAR.AI
{
    public class HotspotSystem : MonoBehaviour
    {
        private class HotspotData
        {
            public string name;
            public List<Vector3> positions;
        }

        private Dictionary<string, List<HotspotData>> _hotspotsByMap = new();
        private readonly Dictionary<BotOwner, HotspotSession> _botSessions = new();

        private const float HOTSPOT_SWITCH_INTERVAL = 120f;
        private const string HOTSPOT_FOLDER = "hotspots/";

        private void Start()
        {
            LoadAllHotspots();
        }

        private void Update()
        {
            foreach (var bot in Singleton<BotsController>.Instance.Bots.BotOwners)
            {
                if (bot == null || bot.IsDead || bot.AIData == null)
                    continue;

                if (!_botSessions.TryGetValue(bot, out var session))
                {
                    session = AssignHotspot(bot);
                    _botSessions[bot] = session;
                }

                session.Update();
            }
        }

        private HotspotSession AssignHotspot(BotOwner bot)
        {
            var mapName = Singleton<GameWorld>.Instance.LocationId;
            if (!_hotspotsByMap.TryGetValue(mapName, out var allHotspots))
                return null;

            // Filter for bot type
            var personality = MOARBotOwner.Get(bot)?.PersonalityProfile;
            bool isPmc = bot.Profile.Info.Side == EPlayerSide.Usec || bot.Profile.Info.Side == EPlayerSide.Bear;

            var pool = allHotspots.Where(h => isPmc || personality?.Personality == PersonalityType.Dumb).ToList();

            if (pool.Count == 0)
                return null;

            // Select based on personality
            HotspotData selected = SelectByPersonality(pool, personality);
            return new HotspotSession(bot, selected);
        }

        private HotspotData SelectByPersonality(List<HotspotData> pool, BotPersonalityProfile personality)
        {
            if (personality == null || personality.Personality == PersonalityType.Dumb)
                return pool[Random.Range(0, pool.Count)];

            if (personality.Personality == PersonalityType.Explorer || personality.Personality == PersonalityType.Strategic)
                return pool.OrderByDescending(h => h.positions.Count).First();

            if (personality.Personality == PersonalityType.Cautious)
                return pool.OrderBy(h => h.name.Length).First();

            return pool[Random.Range(0, pool.Count)];
        }

        public void ReloadHotspots()
        {
            _hotspotsByMap.Clear();
            LoadAllHotspots();
        }

        private void LoadAllHotspots()
        {
            var fullPath = Path.Combine(Application.dataPath, HOTSPOT_FOLDER);
            if (!Directory.Exists(fullPath))
                return;

            foreach (var file in Directory.GetFiles(fullPath, "*.json"))
            {
                string json = File.ReadAllText(file);
                var data = JsonConvert.DeserializeObject<Dictionary<string, List<HotspotData>>>(json);
                foreach (var kvp in data)
                    _hotspotsByMap[kvp.Key] = kvp.Value;
            }
        }

        private class HotspotSession
        {
            private readonly BotOwner _bot;
            private readonly HotspotData _hotspot;
            private int _currentIndex;
            private float _nextSwitchTime;

            public HotspotSession(BotOwner bot, HotspotData hotspot)
            {
                _bot = bot;
                _hotspot = hotspot;
                _currentIndex = 0;
                _nextSwitchTime = Time.time + HOTSPOT_SWITCH_INTERVAL;
            }

            public void Update()
            {
                if (_bot == null || _bot.IsDead || _hotspot.positions.Count == 0)
                    return;

                if (Time.time >= _nextSwitchTime)
                {
                    _currentIndex = (_currentIndex + 1) % _hotspot.positions.Count;
                    _nextSwitchTime = Time.time + HOTSPOT_SWITCH_INTERVAL;
                }

                var target = _hotspot.positions[_currentIndex];
                _bot.GoToPoint(target, false);
            }
        }
    }
}
