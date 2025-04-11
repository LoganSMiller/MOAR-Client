using EFT;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOAR.AI
{
    public class BotGroupSyncCoordinator : MonoBehaviour
    {
        private static readonly Dictionary<string, Vector3> SharedLoot = new();
        private static readonly Dictionary<string, Vector3> SharedExtract = new();

        private string _matchId;

        public void Init(BotOwner bot)
        {
            _matchId = bot.GetPlayer.ProfileId.Substring(0, 4); // FIKA: use actual match ID if exposed
        }

        public void BroadcastLootPoint(Vector3 point)
        {
            SharedLoot[_matchId] = point;
        }

        public void BroadcastExtractPoint(Vector3 point)
        {
            SharedExtract[_matchId] = point;
        }

        public Vector3? GetSharedLootTarget()
        {
            return SharedLoot.TryGetValue(_matchId, out var point) ? point : null;
        }

        public Vector3? GetSharedExtractTarget()
        {
            return SharedExtract.TryGetValue(_matchId, out var point) ? point : null;
        }
    }
}
