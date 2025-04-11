using System.Collections.Generic;
using Comfort.Common;
using EFT;
using UnityEngine;
using MOAR.AI;
using MOAR.AI.Optimization;

namespace MOAR.Components.AI
{
    public class BotAIController : MonoBehaviour
    {
        private readonly Dictionary<BotOwner, BotPerceptionSystem> _perceptionSystems = new();
        private readonly Dictionary<BotOwner, BotGroupBehavior> _groupBehaviors = new();
        private readonly Dictionary<BotOwner, BotCoverLogic> _coverLogics = new();

        private readonly HashSet<BotOwner> _initializedBots = new();

        private void Update()
        {
            if (!Singleton<BotsController>.Instantiated || Singleton<BotsController>.Instance == null)
                return;

            foreach (var botOwner in Singleton<BotsController>.Instance.Bots.BotOwners)
            {
                if (botOwner == null || botOwner.IsDead || botOwner.AIData == null)
                {
                    CleanupBot(botOwner);
                    continue;
                }

                if (!_initializedBots.Contains(botOwner))
                    InitializeBot(botOwner);

                UpdateBotLogic(botOwner);
            }
        }

        private void InitializeBot(BotOwner bot)
        {
            if (bot == null) return;

            _initializedBots.Add(bot);

            var perception = new BotPerceptionSystem(bot);
            _perceptionSystems[bot] = perception;

            _groupBehaviors[bot] = new BotGroupBehavior(bot, perception);
            _coverLogics[bot] = new BotCoverLogic(bot);
        }

        private void UpdateBotLogic(BotOwner bot)
        {
            if (_perceptionSystems.TryGetValue(bot, out var perception))
                perception.Update();

            if (_groupBehaviors.TryGetValue(bot, out var group))
                group.Update();

            if (_coverLogics.TryGetValue(bot, out var cover))
                cover.Update();
        }

        private void CleanupBot(BotOwner bot)
        {
            if (bot == null)
                return;

            if (_initializedBots.Remove(bot))
            {
                if (_perceptionSystems.TryGetValue(bot, out var perception))
                {
                    perception.Dispose();
                    _perceptionSystems.Remove(bot);
                }

                _groupBehaviors.Remove(bot);
                _coverLogics.Remove(bot);
            }
        }
    }
}
