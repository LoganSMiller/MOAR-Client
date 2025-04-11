using UnityEngine;
using EFT;
using MOAR.AI.Extensions;

namespace MOAR.AI
{
    /// <summary>
    /// Handles panic behavior for bots. Triggers a retreat/fallback when blinded or under high threat.
    /// </summary>
    public class BotPanicHandler : MonoBehaviour
    {
        private BotOwner _bot;
        private BotComponentCache _cache;

        private float panicStartTime = -1f;
        private const float PanicDuration = 3.5f;

        private bool isPanicking = false;

        void Awake()
        {
            _bot = GetComponent<BotOwner>();
            _cache = GetComponent<BotComponentCache>();
        }

        void Update()
        {
            if (_bot == null || _cache == null)
                return;

            if (ShouldTriggerPanic())
            {
                if (!isPanicking)
                {
                    StartPanic();
                }
            }

            if (isPanicking && Time.time - panicStartTime > PanicDuration)
            {
                EndPanic();
            }
        }

        public void TriggerPanic()
        {
            if (!isPanicking)
            {
                StartPanic();
            }
        }

        private bool ShouldTriggerPanic()
        {
            // Flashbang response
            if (_cache.FlashGrenade != null && _cache.FlashGrenade.IsFlashed())
                return true;

            // Health-based panic
            if (_bot.HealthController.GetBodyPartHealth(EBodyPart.Common).Current < 25f)
                return true;

            return false;
        }

        private void StartPanic()
        {
            isPanicking = true;
            panicStartTime = Time.time;

            Vector3 fallbackDirection = -_bot.LookDirection.normalized;
            Vector3 fallbackPosition = _bot.Transform.position + fallbackDirection * 10f;

            _bot.FallbackTo(fallbackPosition);

            Debug.Log($"[MOAR] Bot {_bot.Profile?.Info?.Nickname} entered PANIC state.");
        }

        private void EndPanic()
        {
            isPanicking = false;

            _bot.SetCombatAggressionMode();

            Debug.Log($"[MOAR] Bot {_bot.Profile?.Info?.Nickname} ended PANIC state.");
        }
    }
}
