using System.Collections.Generic;
using UnityEngine;
using EFT;

namespace MOAR.AI.Optimization
{
    public class BotOwnerPathfindingCache
    {
        private readonly Dictionary<string, List<Vector3>> _cache = new();

        /// <summary>
        /// Returns a cached path for the bot if available, otherwise computes a new one.
        /// </summary>
        public List<Vector3> GetOptimizedPath(BotOwner botOwner, Vector3 destination)
        {
            if (botOwner?.Profile?.Id == null)
                return new List<Vector3> { destination }; // fallback

            string botId = botOwner.Profile.Id;

            if (_cache.TryGetValue(botId, out var cachedPath))
            {
                return cachedPath;
            }

            var newPath = CalculatePath(botOwner, destination);
            _cache[botId] = newPath;
            return newPath;
        }

        /// <summary>
        /// Very basic direct path for now. Future: add NavMesh / obstacle-aware routing.
        /// </summary>
        private List<Vector3> CalculatePath(BotOwner botOwner, Vector3 destination)
        {
            Vector3 startPos = botOwner.Transform?.position ?? destination;

            return new List<Vector3>
            {
                startPos,
                destination
            };
        }

        /// <summary>
        /// Optionally clear cache for respawning/rebuilding.
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}
