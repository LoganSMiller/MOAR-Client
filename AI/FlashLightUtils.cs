using UnityEngine;

namespace MOAR.AI
{
    public static class FlashLightUtils
    {
        public static bool IsBlindingLight(Transform lightTransform, Transform botHeadTransform, float angleThreshold = 30f)
        {
            if (lightTransform == null || botHeadTransform == null)
                return false;

            Vector3 directionToLight = (lightTransform.position - botHeadTransform.position).normalized;
            float angle = Vector3.Angle(botHeadTransform.forward, directionToLight);

            return angle < angleThreshold;
        }

        public static float GetFlashIntensityFactor(Transform lightTransform, Transform botHeadTransform)
        {
            if (lightTransform == null || botHeadTransform == null)
                return 0f;

            Vector3 dirToLight = (lightTransform.position - botHeadTransform.position).normalized;
            return Vector3.Dot(botHeadTransform.forward, dirToLight);
        }

        public static bool IsFacingTarget(Transform source, Transform target, float angleThreshold = 30f)
        {
            if (source == null || target == null)
                return false;

            Vector3 directionToTarget = (target.position - source.position).normalized;
            float angle = Vector3.Angle(source.forward, directionToTarget);

            return angle < angleThreshold;
        }
    }
}
