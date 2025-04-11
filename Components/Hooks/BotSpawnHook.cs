using UnityEngine;
using EFT;
using Comfort.Common;
using System.Collections.Generic;
using MOAR.AI;
using MOAR.AI.Optimization;

namespace MOAR.Components.Hooks
{
    public class BotSpawnHook : MonoBehaviour
    {
        private static readonly List<PersonalityType> _personalities = new()
        {
            PersonalityType.Aggressive,
            PersonalityType.Cautious,
            PersonalityType.TeamPlayer,
            PersonalityType.Reckless,
            PersonalityType.Frenzied,
            PersonalityType.Sniper,
            PersonalityType.Strategic,
            PersonalityType.RiskTaker,
            PersonalityType.Dumb,
            PersonalityType.Explorer
        };

        private void Awake()
        {
            BotsController botsController = Singleton<BotsController>.Instance;
            if (botsController == null)
                return;

            botsController.OnBotCreated += AttachMOARComponents;
        }

        private void AttachMOARComponents(BotOwner botOwner)
        {
            if (botOwner == null || botOwner.GetPlayer == null)
                return;

            GameObject botGO = botOwner.GetPlayer.gameObject;

            // Attach and initialize BotComponentCache
            if (!botGO.TryGetComponent<BotComponentCache>(out var cache))
            {
                cache = botGO.AddComponent<BotComponentCache>();
                cache.Initialize(botOwner);
            }

            // Attach and assign random personality to MOARBotOwner
            if (!botGO.TryGetComponent<MOARBotOwner>(out var moarOwner))
            {
                moarOwner = botGO.AddComponent<MOARBotOwner>();
            }

            var random = UnityEngine.Random.Range(0, _personalities.Count);
            moarOwner.SetPersonality(_personalities[random]);

            // Assign personality to cache as well
            cache.Personality = moarOwner.PersonalityProfile;

            // Set dynamic fallback zone ID (can later be used by memory.SetTargetZone)
            botOwner.AIData?.SetGroupId(botOwner.BotsGroup?.GroupId ?? 0);
            botOwner.Memory?.SetTargetZone(botOwner.Position);

            Debug.Log($"[MOAR] Bot initialized with personality: {moarOwner.PersonalityProfile.Personality}");
        }
    }
}
