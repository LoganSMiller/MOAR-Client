using EFT;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MOAR.AI;

namespace MOAR.AI
{
    public class BotVisionController : MonoBehaviour
    {
        public float updateInterval = 0.25f;
        private float updateTimer;

        private readonly Dictionary<string, FieldInfo> visionFields = new();
        private readonly Dictionary<string, float> botVisionFactors = new();

        [Header("Brightness Response")]
        public AnimationCurve brightnessCurve = AnimationCurve.EaseInOut(0f, 0.15f, 1f, 1f);

        [Header("Debug")]
        public bool showVisionRadius = false;
        public Color gizmoColor = new(1f, 1f, 0.3f, 0.25f);

        private void Start() => CacheVisionFields();

        private void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < updateInterval) return;
            updateTimer = 0f;

            float ambient = RenderSettings.ambientLight.grayscale;
            float fogFactor = RenderSettings.fog ? RenderSettings.fogDensity * 1.5f : 0f;
            float brightness = Mathf.Clamp01(ambient - fogFactor);
            float targetLightFactor = brightnessCurve.Evaluate(brightness);

            foreach (BotOwner bot in BotCacheUtility.AllActiveBots)
            {
                if (bot == null || !bot.IsAI) continue;

                string id = bot.ProfileId;
                float current = botVisionFactors.TryGetValue(id, out var prev) ? prev : 1f;
                float updated = Mathf.Lerp(current, targetLightFactor, Time.deltaTime * 1.5f);
                botVisionFactors[id] = updated;

                ApplyVisionParams(updated);
            }
        }

        private void CacheVisionFields()
        {
            var type = AccessTools.TypeByName("BotGlobalLookData");
            if (type == null)
            {
                Debug.LogWarning("[MOAR] Could not locate BotGlobalLookData.");
                return;
            }

            foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                if (!visionFields.ContainsKey(field.Name))
                    visionFields[field.Name] = field;
            }
        }

        private void ApplyVisionParams(float factor)
        {
            factor = Mathf.Clamp(factor, 0.05f, 3f);

            TrySet("VISIBLE_DISNACE_WITH_LIGHT", Mathf.Clamp(100f * factor, 20f, 160f));
            TrySet("ENEMY_LIGHT_ADD", Mathf.Clamp(Mathf.Lerp(0.3f, 2.5f, factor), 0.1f, 3f));
            TrySet("VISIBLE_ANG_LIGHT", Mathf.Clamp(Mathf.Lerp(80f, 120f, factor), 40f, 150f));
            TrySet("MAX_VISION_GRASS_METERS", Mathf.Clamp(Mathf.Lerp(6f, 16f, factor), 4f, 20f));
            TrySet("LightOnVisionDistance", Mathf.Clamp(Mathf.Lerp(20f, 60f, factor), 10f, 100f));
        }

        private void TrySet(string name, float value)
        {
            if (!visionFields.TryGetValue(name, out var field)) return;

            try
            {
                field.SetValue(null, value);
            }
            catch
            {
                Debug.LogWarning($"[MOAR] Failed to set BotGlobalLookData field '{name}'");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showVisionRadius || !Application.isPlaying) return;

            Gizmos.color = gizmoColor;

            foreach (BotOwner bot in BotCacheUtility.AllActiveBots)
            {
                if (bot == null || !bot.IsAI) continue;
                var pos = bot.Position + Vector3.up * 1.5f;
                Gizmos.DrawWireSphere(pos, 30f);
            }
        }
    }
}
