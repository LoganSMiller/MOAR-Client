using System.Collections.Generic;
using UnityEngine;
using EFT;
using UnityEngine.AI;

namespace MOAR.AI
{
    public class BotVisionSystem
    {
        private readonly BotOwner _bot;
        private const float VIEW_ANGLE = 120f;
        private const float MAX_VIEW_DISTANCE = 150f;
        private readonly List<IPlayer> _visibleEnemies = new();

        public BotVisionSystem(BotOwner bot)
        {
            _bot = bot;
        }

        public List<IPlayer> ScanForEnemies(List<IPlayer> allPlayers)
        {
            _visibleEnemies.Clear();
            Vector3 botPosition = _bot.Position;
            Vector3 botForward = _bot.LookDirection;

            foreach (var player in allPlayers)
            {
                if (player == null || player.HealthController.IsDead || player.Id == _bot.GetPlayer.Id)
                    continue;

                // Ignore teammates
                if (_bot.GetPlayer.Profile.Info.GroupId == player.Profile.Info.GroupId)
                    continue;

                Vector3 dirToPlayer = player.Transform.position - botPosition;
                float distance = dirToPlayer.magnitude;

                if (distance > MAX_VIEW_DISTANCE)
                    continue;

                float angle = Vector3.Angle(botForward, dirToPlayer);
                if (angle > VIEW_ANGLE / 2f)
                    continue;

                if (!HasLineOfSight(botPosition, player.Transform.position))
                    continue;

                _visibleEnemies.Add(player);
            }

            return _visibleEnemies;
        }

        private bool HasLineOfSight(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            float distance = direction.magnitude;

            if (Physics.Raycast(from + Vector3.up * 1.5f, direction.normalized, out RaycastHit hit, distance))
            {
                // Only allow clear line if we hit the player
                return hit.collider.GetComponent<IPlayer>() != null;
            }

            return false;
        }
    }
}
