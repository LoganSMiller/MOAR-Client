using System.Collections.Generic;
using UnityEngine;
using EFT;

namespace MOAR.AI
{
    public class FlashGrenadeComponent : MonoBehaviour
    {
        public BotOwner Bot { get; private set; }

        private float lastFlashTime;
        private bool isBlinded;
        private const float BlindDuration = 4.5f; // Seconds

        private static readonly float FlashlightThresholdAngle = 25f;
        private static readonly float FlashlightMinIntensity = 2.0f;

        void Awake()
        {
            Bot = GetComponent<BotOwner>();
        }

        void Update()
        {
            if (Bot == null || Bot.HealthController == null)
                return;

            CheckFlashlightExposure();

            if (isBlinded && Time.time - lastFlashTime > BlindDuration)
            {
                isBlinded = false;
            }
        }

        private void CheckFlashlightExposure()
        {
            Vector3 botForward = Bot.LookDirection;
            Vector3 botPosition = Bot.Transform.position;

            foreach (var light in FindObjectsOfType<Light>())
            {
                if (!light.enabled || light.type != LightType.Spot || light.intensity < FlashlightMinIntensity)
                    continue;

                Vector3 dirToLight = (light.transform.position - botPosition).normalized;
                float angle = Vector3.Angle(botForward, -dirToLight);

                if (angle < FlashlightThresholdAngle)
                {
                    AddBlindEffect(BlindDuration);
                    break;
                }
            }
        }

        public bool IsFlashed()
        {
            return isBlinded;
        }

        public void AddBlindEffect(float duration)
        {
            lastFlashTime = Time.time;
            isBlinded = true;

            Debug.Log($"[MOAR] Bot {Bot?.Profile?.Info?.Nickname} is flashed for {duration} seconds.");
        }
    }
}
