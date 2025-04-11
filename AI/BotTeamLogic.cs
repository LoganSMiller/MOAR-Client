using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace MOAR.AI
{
    public class BotTeamLogic
    {
        private readonly BotOwner _bot;
        private List<BotOwner> _teammates;

        public BotTeamLogic(BotOwner bot)
        {
            _bot = bot;
            _teammates = new List<BotOwner>();
        }

        public void SetTeammates(List<BotOwner> allBots)
        {
            _teammates.Clear();

            string myGroup = _bot.GetPlayer.Profile.Info.GroupId;

            foreach (var other in allBots)
            {
                if (other != null && other != _bot && other.GetPlayer.Profile.Info.GroupId == myGroup)
                {
                    _teammates.Add(other);
                }
            }
        }

        public void ShareTarget(IPlayer enemy)
        {
            foreach (var teammate in _teammates)
            {
                if (teammate?.Memory?.GoalEnemy == null || teammate.Memory.GoalEnemy.Person.Id != enemy.Id)
                {
                    teammate.EnemiesController?.AddEnemy(enemy);
                }
            }
        }

        public void CoordinateMovement()
        {
            if (_teammates.Count < 1) return;

            Vector3 centroid = Vector3.zero;
            int active = 0;

            foreach (var teammate in _teammates)
            {
                if (teammate.IsActive)
                {
                    centroid += teammate.Position;
                    active++;
                }
            }

            if (active > 0)
            {
                centroid /= active;
                _bot.Mover?.SetTargetPoint(centroid + Random.insideUnitSphere * 3f);
            }
        }
    }
}
