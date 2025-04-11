using UnityEngine;
using EFT;
using System.Collections;
using MOAR.Helpers;
using MOAR.AI.Components;
using MOAR.AI.Optimization;

namespace MOAR.AI
{
    /// <summary>
    /// AI behavior triggered by intense flashlight exposure: blind panic, vision drop, and movement disruption.
    /// </summary>
    public class BotFlashlightEvadeComponent : MonoBehaviour
    {
        [Header("Scan Settings")]
        public float checkInterval = 0.5f;
        public float exposureToBlindTime = 0.75f;

        [Header("Blindness Settings")]
        public float blindDuration = 2.5f;

        [Header("Detection Cone")]
        public float maxDetectionDistance = 15f;
        public float maxAngle = 60f;

        private float exposureTime;
        private float checkTimer;
        private BotComponentCache cache = null!;
        private static readonly RaycastHit[] hitBuffer = new RaycastHit[2];

        private void Start()
        {
            var player = GetComponent<Player>();
            cache = player.GetComponent<BotComponentCache>();
            if (cache == null)
            {
                Debug.LogWarning("[MOAR] BotFlashlightEvadeComponent: No BotComponentCache attached.");
                enabled = false;
                return;
            }
        }

        private void Update()
        {
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                checkTimer = 0f;
                EvaluateExposure();
            }
        }

        private void EvaluateExposure()
        {
            if (!IsExposedToFlashlight(out var light))
            {
                exposureTime = Mathf.Max(0f, exposureTime - checkInterval);
                return;
            }

            exposureTime += checkInterval;

            if (exposureTime >= exposureToBlindTime &&
                cache.FlashGrenade != null &&
                !cache.FlashGrenade.IsFlashed)
            {
                cache.FlashGrenade.AddBlindEffect(blindDuration, cache.Head.position);

                if (cache.TryGetPanicComponent(out var panic))
                    panic.TriggerPanic("Flashlight", light.transform.position);

                exposureTime = 0f;
            }
        }

        private bool IsExposedToFlashlight(out Light? lightSource)
        {
            lightSource = null;

            foreach (var light in FlashlightRegistry.GetAllLights())
            {
                if (!light.enabled || light.intensity < 2f || light.range < 5f)
                    continue;

                Vector3 toBot = cache.Head.position - light.transform.position;
                float distance = toBot.magnitude;
                if (distance > maxDetectionDistance) continue;

                if (!FlashLightUtils.IsFacingTarget(light.transform.position, cache.Head.position, light.transform.forward, maxAngle))
                    continue;

                int hitCount = Physics.RaycastNonAlloc(light.transform.position, toBot.normalized, hitBuffer, distance);
                if (hitCount > 0 && hitBuffer[0].collider?.gameObject == cache.Bot.gameObject)
                {
                    lightSource = light;
                    return true;
                }
            }

            return false;
        }
    }
}
