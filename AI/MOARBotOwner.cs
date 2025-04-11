using UnityEngine;
using EFT;
using MOAR.AI.Optimization;

namespace MOAR.AI
{
    /// <summary>
    /// Tracks per-bot data such as personality, performance stats, and optimization hooks.
    /// </summary>
    public class MOARBotOwner : MonoBehaviour
    {
        public BotOwner BotOwner { get; private set; }
        public BotPersonalityProfile PersonalityProfile { get; private set; }
        public BotComponentCache ComponentCache { get; private set; }

        private void Awake()
        {
            BotOwner = GetComponent<BotOwner>();
            ComponentCache = GetComponent<BotComponentCache>() ?? gameObject.AddComponent<BotComponentCache>();

            // Set default personality
            PersonalityProfile = BotPersonalityPresets.GetRandomPreset();
        }

        public void SetPersonality(PersonalityType type)
        {
            PersonalityProfile = BotPersonalityPresets.Get(type);
        }

        public void SetCustomPersonality(BotPersonalityProfile profile)
        {
            PersonalityProfile = profile;
        }
    }
}
