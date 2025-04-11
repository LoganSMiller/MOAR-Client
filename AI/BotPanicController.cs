using EFT;
using UnityEngine;
using System.Collections;
using System.Diagnostics;
using MOAR.Helpers;
using MOAR.AI.Optimization;

namespace MOAR.AI
{
    /// <summary>
    /// Handles AI panic behavior such as suppression, blinding, and evasion.
    /// Supports different panic causes (e.g., lighting, gunfire, explosions).
    /// </summary>
    public class BotPanicController : MonoBehaviour
    {
        [Header("Panic Tuning")]
        public float panicDuration = 3f;
        public float visionMultiplier = 0.5f;
        public float scatterChance = 0.4f;
        public float panicMovementSpeed = 0.25f;

        [Header("Optional Panic Audio Cue")]
        public string panicVoiceLine = "panic_generic";

        private BotComponentCache cache = null!;
        private bool isPanicActive;
        private string _panicCause = "Unknown";
        private Vector3 _panicOrigin;

        public bool IsPanicActive => isPanicActive;

        private void Awake()
        {
            if (!BotComponentCache.TryGet(GetComponent<Player>(), out cache))
            {
                enabled = false;
                return;
            }
        }

        /// <summary>
        /// Triggers panic behavior from any cause (e.g., "Lighting", "Gunfire").
        /// </summary>
        public void TriggerPanic(string cause, Vector3 threatSource)
        {
            if (isPanicActive) return;

            _panicCause = cause;
            _panicOrigin = threatSource;
            StartCoroutine(ExecutePanicRoutine());
        }

        private IEnumerator ExecutePanicRoutine()
        {
            isPanicActive = true;

            float originalVision = cache.Look?.ClearVisibleDist ?? 60f;
            if (cache.Look != null)
                cache.Look.ClearVisibleDist = originalVision * visionMultiplier;

            cache.Owner?.Mover?.Stop();
            cache.Owner?.Mover?.SetTargetMoveSpeed(panicMovementSpeed);

            ApplyAimScatter();
            RotateHeadToward(_panicOrigin);
            TriggerPanicVoice();

            LogDebug($"PANIC START → {cache.Bot.Profile?.Info?.Settings?.Role} | Cause={_panicCause}");

            yield return new WaitForSeconds(panicDuration);

            if (cache.Look != null)
                cache.Look.ClearVisibleDist = originalVision;

            cache.Owner?.Mover?.SetTargetMoveSpeed(1f);
            isPanicActive = false;

            LogDebug($"PANIC RECOVERED → {cache.Bot.Profile?.Info?.Settings?.Role} | Cause={_panicCause}");
        }

        /// <summary>
        /// Introduces randomized aim scatter using the bot's AimingManager.
        /// </summary>
        private void ApplyAimScatter()
        {
            if (cache.Owner?.AimingManager?.CurrentAiming == null || Random.value > scatterChance)
                return;

            Vector3 scatterOffset = Random.insideUnitSphere * 1.5f;
            Vector3 panicLookTarget = cache.Head.position + scatterOffset;

            cache.Owner.AimingManager.CurrentAiming.SetTarget(panicLookTarget);
        }

        /// <summary>
        /// Visually turns the bot's head toward the panic threat source.
        /// </summary>
        private void RotateHeadToward(Vector3 target)
        {
            Vector3 direction = (target - cache.Head.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            cache.Head.rotation = Quaternion.Slerp(cache.Head.rotation, lookRotation, 0.6f);
        }

        /// <summary>
        /// Triggers a panic voice line or bark if supported.
        /// </summary>
        private void TriggerPanicVoice()
        {
            try
            {
                cache.Owner?.Talk?.Say(panicVoiceLine);
            }
            catch
            {
#if UNITY_EDITOR || DEBUG
                Debug.LogWarning($"[RLO] Bot {cache.Bot.Profile?.Nickname ?? name} could not say: {panicVoiceLine}");
#endif
            }
        }

        [Conditional("DEBUG"), Conditional("UNITY_EDITOR")]
        private void LogDebug(string msg)
        {
            if (RLOConfig.DebugEnabled.Value)
                UnityEngine.Debug.Log($"[RLO] {msg}");
        }
    }
}
