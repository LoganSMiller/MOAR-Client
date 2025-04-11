using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.Communications;

namespace MOAR.AI
{
    public class BotMissionSystem : MonoBehaviour
    {
        private BotOwner _bot = null!;
        private Vector3 _currentObjective;
        private MissionType _missionType;

        private float _lastUpdate;
        private readonly float _objectiveCooldown = 15f;
        private bool _isLooting = false;
        private bool _readyToExtract = false;

        private const float REACHED_THRESHOLD = 6f;

        private readonly List<Vector3> _lootZones = new();
        private readonly List<Vector3> _hotZones = new();
        private readonly List<Vector3> _questZones = new();
        private readonly System.Random _rng = new();

        public void Init(BotOwner bot)
        {
            _bot = bot ?? throw new ArgumentNullException(nameof(bot));
            CacheMapZones();
            PickRandomMission();
        }

        private void Update()
        {
            if (_bot == null || _bot.GetPlayer == null || !_bot.GetPlayer.HealthController.IsAlive)
                return;

            if (Time.time - _lastUpdate > _objectiveCooldown)
            {
                EvaluateMission();
                _lastUpdate = Time.time;
            }

            if (Vector3.Distance(_bot.Position, _currentObjective) < REACHED_THRESHOLD)
            {
                OnObjectiveReached();
            }
        }

        private void CacheMapZones()
        {
            foreach (var obj in FindObjectsOfType<LootableContainer>())
                _lootZones.Add(obj.transform.position);

            foreach (var trigger in FindObjectsOfType<BotZone>())
                _hotZones.Add(trigger.transform.position);

            // Fallback: no QuestMarker in EFT — reuse hot zones for quests if needed
            _questZones.AddRange(_hotZones);
        }

        private void PickRandomMission()
        {
            _missionType = (MissionType)_rng.Next(0, 3);
            _currentObjective = GetRandomZone(_missionType);
            _bot.GoToPoint(_currentObjective, false);
        }

        private void EvaluateMission()
        {
            switch (_missionType)
            {
                case MissionType.Loot:
                    if (!_readyToExtract && InventoryUtil.IsBackpackFull(_bot))
                    {
                        _readyToExtract = true;
                        TryExtract();
                    }
                    else if (!_readyToExtract)
                    {
                        _currentObjective = GetRandomZone(MissionType.Loot);
                        _bot.GoToPoint(_currentObjective, false);
                    }
                    break;

                case MissionType.Fight:
                    _currentObjective = GetRandomZone(MissionType.Fight);
                    _bot.GoToPoint(_currentObjective, false);
                    break;

                case MissionType.Quest:
                    _currentObjective = GetRandomZone(MissionType.Quest);
                    _bot.GoToPoint(_currentObjective, false);
                    break;
            }
        }

        private void TryExtract()
        {
            var extracts = FindObjectsOfType<ExfiltrationPoint>()
                .Where(p => p.Status == EExfiltrationStatus.RegularMode)
                .OrderBy(p => Vector3.Distance(p.transform.position, _bot.Position));

            var target = extracts.FirstOrDefault();
            if (target != null)
            {
                _bot.GoToPoint(target.transform.position, false);
                Say(EPhraseTrigger.ExitLocated);
            }
            else
            {
                Say(EPhraseTrigger.MumblePhrase); // Fallback message
            }
        }

        private void OnObjectiveReached()
        {
            if (_missionType == MissionType.Loot && !_isLooting)
            {
                _isLooting = true;
                Say(EPhraseTrigger.OnLoot);
                _bot.Mover?.Stop();
                Invoke(nameof(ResumeMovement), 4f);
            }

            if (_readyToExtract)
            {
                Say(EPhraseTrigger.MumblePhrase); // Used in place of ExfilComplete
                _bot.Deactivate();
                if (_bot.GetPlayer != null)
                    _bot.GetPlayer.gameObject.SetActive(false);
            }
        }

        private void ResumeMovement()
        {
            if (_bot != null && _bot.Mover != null)
                _bot.GoToPoint(_currentObjective, false);
        }

        private Vector3 GetRandomZone(MissionType type)
        {
            return type switch
            {
                MissionType.Loot => _lootZones.OrderBy(_ => _rng.Next()).FirstOrDefault(),
                MissionType.Fight => _hotZones.OrderBy(_ => _rng.Next()).FirstOrDefault(),
                MissionType.Quest => _questZones.OrderBy(_ => _rng.Next()).FirstOrDefault(),
                _ => _bot.Position
            };
        }

        private void Say(EPhraseTrigger phrase)
        {
            try
            {
                _bot?.GetPlayer?.Say(phrase);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[BotMissionSystem] Failed to play voice: {ex.Message}");
            }
        }

        private enum MissionType { Loot, Fight, Quest }
    }

    public static class InventoryUtil
    {
        public static bool IsBackpackFull(BotOwner bot)
        {
            var inv = bot?.GetPlayer?.Inventory;
            var bpSlot = inv?.Equipment?.GetSlot(EquipmentSlot.Backpack);
            var backpack = bpSlot?.ContainedItem;

            return backpack != null && backpack.StackObjectsCount >= backpack.Template.StackMaxSize;
        }
    }
}
