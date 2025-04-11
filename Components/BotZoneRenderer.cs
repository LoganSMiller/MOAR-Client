#nullable enable
using System.Collections.Generic;
using System.Linq;
using Comfort.Common;
using EFT;
using EFT.Game.Spawning;
using MOAR.Helpers;
using UnityEngine;
using Fika.Core.Coop.Utils;

namespace MOAR.Components
{
    /// <summary>
    /// Renders real-time labels above spawn points for dev/debug use.
    /// Automatically disables in headless FIKA sessions.
    /// </summary>
    public class BotOwnerZoneRenderer : MonoBehaviour
    {
        private static Player? Player => Singleton<GameWorld>.Instance?.MainPlayer;

        private readonly List<BotOwnerZone> _BotOwnerZones = new();
        private readonly List<SpawnPointInfo> _spawnPointInfos = new();

        // ✅ Initialize to avoid CS8618
        private GUIStyle _guiStyle = new();
        private float _screenScale = 1f;

        private void Awake()
        {
            if (Settings.IsFika && FikaBackendUtils.IsHeadless)
            {
                Plugin.LogSource.LogInfo("[BotOwnerZoneRenderer] Skipped setup (headless mode).");
                Destroy(this);
                return;
            }

            if (CameraClass.Instance?.SSAA?.isActiveAndEnabled == true)
            {
                float output = CameraClass.Instance.SSAA.GetOutputWidth();
                float input = CameraClass.Instance.SSAA.GetInputWidth();
                if (input > 0f)
                {
                    _screenScale = output / input;
                }
            }

            RefreshZones();
        }

        public void RefreshZones()
        {
            _BotOwnerZones.Clear();
            _spawnPointInfos.Clear();

            _BotOwnerZones.AddRange(LocationScene.GetAllObjectsAndWhenISayAllIActuallyMeanIt<BotOwnerZone>());

            foreach (var zone in _BotOwnerZones)
            {
                AddZoneSpawnPoints(zone);
            }

            Plugin.LogSource.LogDebug($"[BotOwnerZoneRenderer] Refreshed {_spawnPointInfos.Count} spawn labels.");
        }

        private void AddZoneSpawnPoints(BotOwnerZone zone)
        {
            string zoneId = zone.Id.ToString();
            Color zoneColor = GenerateZoneColor(zoneId);

            if (zone.SpawnPoints == null)
                return;

            foreach (var point in zone.SpawnPoints)
            {
                string label = point.GetBotOwnerDebugName();
                _spawnPointInfos.Add(new SpawnPointInfo(point.Position, new GUIContent($"{zoneId} [{label}]"), zoneColor));
            }
        }

        private void OnGUI()
        {
            if (Application.isBatchMode || !Settings.enablePointOverlay.Value || Player == null || Camera.main == null)
                return;

            _guiStyle ??= CreateLabelStyle();

            foreach (var info in _spawnPointInfos)
            {
                if (string.IsNullOrEmpty(info.Content.text))
                    continue;

                float distance = Vector3.Distance(info.Position, Player.Transform.position);
                if (distance > 200f)
                    continue;

                Vector3 screenPos = Camera.main.WorldToScreenPoint(info.Position + Vector3.up * 1.5f);
                if (screenPos.z <= 0f)
                    continue;

                Vector2 size = _guiStyle.CalcSize(info.Content);
                Rect labelRect = new(
                    screenPos.x * _screenScale - size.x / 2f,
                    Screen.height - (screenPos.y * _screenScale + size.y),
                    size.x,
                    size.y
                );

                GUI.Box(labelRect, info.Content, _guiStyle);
            }
        }

        private void OnDestroy()
        {
            _spawnPointInfos.Clear();
            _BotOwnerZones.Clear();
        }

        private GUIStyle CreateLabelStyle()
        {
            return new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
                normal = { textColor = Color.white },
                wordWrap = false
            };
        }

        private static Color GenerateZoneColor(string zoneId)
        {
            int hash = zoneId.GetHashCode();
            UnityEngine.Random.InitState(hash);
            return UnityEngine.Random.ColorHSV(0f, 1f, 0.6f, 1f, 0.6f, 1f);
        }

        private class SpawnPointInfo
        {
            public Vector3 Position { get; }
            public GUIContent Content { get; }
            public Color Color { get; }

            public SpawnPointInfo(Vector3 pos, GUIContent content, Color color)
            {
                Position = pos;
                Content = content;
                Color = color;
            }
        }
    }

    public static class SpawnPointExtensions
    {
        public static string GetBotOwnerDebugName(this ISpawnPoint spawnPoint)
        {
            if (spawnPoint == null)
                return "NullPoint";

            var prop = spawnPoint.GetType().GetProperty("BotOwnerTemplateId");
            return prop?.GetValue(spawnPoint) as string ?? spawnPoint.ToString();
        }
    }
}
