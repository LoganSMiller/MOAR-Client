using EFT;
using UnityEngine;
using System.Collections.Generic;
using MOAR.Helpers;
using System.Diagnostics;

namespace MOAR.AI
{
    /// <summary>
    /// Monitors bots for suppression based on harsh lighting conditions:
    /// accumulated flashlight exposure, ambient darkness, or vision occlusion.
    /// </summary>
    [DisallowMultipleComponent]
    public class BotLightingSuppressionSystem : MonoBehaviour
    {
        [Header("Flashlight Accumulation")]
        public float flashThreshold = 1.5f;
        public float maxFlashDuration = 1f;

        [Header("Ambient Suppression")]
        public float ambientThreshold = 0.18f;
        public float visibilityThreshold = 15f;

        [Header("Polling")]
        public float checkInterval = 0.25f;
        public float maxLightDistance = 15f;

        private float _timer;
        private readonly Dictionary<Player, float> flashExposure = new();
        private static readonly RaycastHit[] hitBuffer = new RaycastHit[2];

        private void Awake()
        {
            LogDebug("BotLightingSuppressionSystem active.");
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            foreach (var bot in BotRegistry.Bots)
            {
                if (!BotCacheUtility.TryGet(bot, out var cache))
                    continue;

                if (!flashExposure.TryGetValue(bot, out float exposure))
                    exposure = 0f;

                float accumulatedIntensity = 0f;

                foreach (var light in FlashlightRegistry.GetAllLights())
                {
                    if (!light.enabled || light.intensity < flashThreshold || light.range < 5f)
                        continue;

                    Vector3 toBot = cache.Head.position - light.transform.position;
                    float distance = toBot.magnitude;
                    if (distance > maxLightDistance) continue;

                    if (!FlashLightUtils.IsFacingTarget(light.transform.position, cache.Head.position, light.transform.forward, 60f))
                        continue;

                    int hits = Physics.RaycastNonAlloc(light.transform.position, toBot.normalized, hitBuffer, distance);
                    if (hits > 0 && hitBuffer[0].collider?.gameObject == bot.gameObject)
                        accumulatedIntensity += light.intensity;
                }

                // Accumulate or decay exposure
                exposure = (accumulatedIntensity > 0f)
                    ? exposure + checkInterval
                    : Mathf.Max(0f, exposure - checkInterval);

                flashExposure[bot] = exposure;

                bool shouldSuppress =
                    exposure >= maxFlashDuration ||
                    BotSuppressionHelper.ShouldTriggerSuppression(bot, visibilityThreshold, ambientThreshold);

                if (shouldSuppress)
                {
                    string cause = (exposure >= maxFlashDuration) ? "Lighting" : "Ambient";

                    if (BotPanicUtility.TryGet(bot, out var panic))
                    {
                        panic.TriggerPanic(cause, bot.Transform.position);
                    }
                    else
                    {
                        BotSuppressionHelper.TrySuppressBot(bot, bot.Transform.position);
                    }

                    flashExposure[bot] = 0f;
                    LogDebug($"Bot '{bot.Profile?.Nickname ?? bot.name}' suppressed ({cause}, exposure={exposure:F2})");
                }
            }
        }

        [Conditional("DEBUG"), Conditional("UNITY_EDITOR")]
        private void LogDebug(string msg)
        {
            UnityEngine.Debug.Log($"[RLO] {msg}");
        }
    }
}
