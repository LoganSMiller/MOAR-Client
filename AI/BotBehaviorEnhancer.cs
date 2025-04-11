using System.Collections;
using System.Linq;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using UnityEngine;

namespace MOAR.AI
{
    public class BotBehaviorEnhancer : MonoBehaviour
    {
        private BotOwner _bot;
        private MOARBotOwner _moar;
        private BotGroupSyncCoordinator _groupSync;

        private float _lastLootCheck;
        private float _lastExtractCheck;
        private float _lastEnemySeenTime;

        private readonly float _lootCooldown = 10f;
        private readonly float _extractCheckCooldown = 5f;
        private readonly float _combatCalmDelay = 8f;

        private bool _isExtracting = false;
        private bool _hasExtracted = false;

        private const float LOOT_SCAN_RADIUS = 5f;
        private const float EXTRACT_RADIUS = 8f;
        private const float EXTRACT_GROUP_RADIUS = 10f;
        private const float RETREAT_RADIUS = 15f;

        public void Init(BotOwner bot)
        {
            _bot = bot;
            _moar = bot.GetPlayer.GetComponent<MOARBotOwner>();
            _groupSync = bot.GetPlayer.gameObject.AddComponent<BotGroupSyncCoordinator>();
            _groupSync.Init(bot);
        }

        private void Update()
        {
            if (_bot == null || !_bot.GetPlayer?.HealthController?.IsAlive == true || _hasExtracted)
                return;

            if (_bot.Memory.GoalEnemy?.IsVisible == true)
                _lastEnemySeenTime = Time.time;

            if (Time.time - _lastLootCheck > _lootCooldown)
            {
                TryLootNearby();
                _lastLootCheck = Time.time;
            }

            if (Time.time - _lastExtractCheck > _extractCheckCooldown)
            {
                TryExtract();
                _lastExtractCheck = Time.time;
            }

            EvaluateRetreat();
        }

        private void TryLootNearby()
        {
            if (_moar?.PersonalityProfile == null)
                return;

            // Skip looting if bot thinks combat may still be ongoing
            if (Time.time - _lastEnemySeenTime < _combatCalmDelay)
                return;

            var containers = FindObjectsOfType<LootableContainer>();
            foreach (var container in containers)
            {
                float distance = Vector3.Distance(_bot.Position, container.transform.position);
                if (distance <= LOOT_SCAN_RADIUS && HasBackpackSpace())
                {
                    float value = EstimateContainerValue(container);

                    // Cautious bots skip low-value loot even post-fight
                    if (value < 10000f && _moar.PersonalityProfile.RiskTolerance < 0.5f)
                        continue;

                    EnsureDoorOpened(container.transform.position);
                    container.Interact(new InteractionResult(EInteractionType.Open));

                    Say(EPhraseTrigger.OnLoot);
                    _groupSync?.BroadcastLootPoint(container.transform.position);
                    break;
                }
            }

            Vector3? sharedTarget = _groupSync?.GetSharedLootTarget();
            if (sharedTarget.HasValue && Vector3.Distance(_bot.Position, sharedTarget.Value) > 5f)
            {
                _bot.GoToPoint(sharedTarget.Value, false);
            }
        }

        private float EstimateContainerValue(LootableContainer container)
        {
            float total = 0f;
            foreach (var item in container.ItemOwner.RootItem.GetAllItems())
            {
                if (item?.Template?.Weight > 0)
                    total += item.Template.CreditsPrice;
            }
            return total;
        }

        private bool HasBackpackSpace()
        {
            var inv = _bot.GetPlayer?.Inventory;
            var bpSlot = inv?.Equipment?.GetSlot(EquipmentSlot.Backpack);
            var backpack = bpSlot?.ContainedItem;

            return backpack != null && backpack.StackObjectsCount < backpack.Template.StackMaxSize;
        }

        private void TryExtract()
        {
            if (_isExtracting || _bot == null || _moar?.PersonalityProfile == null)
                return;

            if (!IsGroupAlignedForExtract() && _moar.PersonalityProfile.Cohesion > 0.5f)
                return;

            var extractZones = FindObjectsOfType<ExfiltrationPoint>();
            foreach (var zone in extractZones)
            {
                if (zone.Status != EExfiltrationStatus.RegularMode)
                    continue;

                float dist = Vector3.Distance(_bot.Position, zone.transform.position);
                if (dist <= EXTRACT_RADIUS)
                {
                    StartExtract(zone);
                    break;
                }
            }

            Vector3? groupExtract = _groupSync?.GetSharedExtractTarget();
            if (groupExtract.HasValue && !_isExtracting)
            {
                _bot.GoToPoint(groupExtract.Value, false);
            }
        }

        private bool IsGroupAlignedForExtract()
        {
            if (_bot.BotsGroup == null || _bot.BotsGroup.MembersCount <= 1)
                return true;

            int nearAllies = 0;
            for (int i = 0; i < _bot.BotsGroup.MembersCount; i++)
            {
                var other = _bot.BotsGroup.Member(i);
                if (other != null && other != _bot && Vector3.Distance(other.Position, _bot.Position) < EXTRACT_GROUP_RADIUS)
                {
                    nearAllies++;
                }
            }

            return nearAllies >= Mathf.CeilToInt(_bot.BotsGroup.MembersCount * _moar.PersonalityProfile.Cohesion);
        }

        private void StartExtract(ExfiltrationPoint point)
        {
            _isExtracting = true;
            _groupSync?.BroadcastExtractPoint(point.transform.position);

            Say(EPhraseTrigger.MumblePhrase);
            _bot.GoToPoint(point.transform.position, false);

            StartCoroutine(CompleteExtractAfterDelay(6f));
        }

        private IEnumerator CompleteExtractAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            Say(EPhraseTrigger.MumblePhrase);
            _hasExtracted = true;

            _bot.Deactivate();
            _bot.GetPlayer?.gameObject.SetActive(false);
        }

        private void EvaluateRetreat()
        {
            if (_moar?.PersonalityProfile == null || !_bot.Memory.IsUnderFire)
                return;

            float current = 0f, max = 0f;
            foreach (EBodyPart part in System.Enum.GetValues(typeof(EBodyPart)))
            {
                var hp = _bot.GetPlayer.HealthController.GetBodyPartHealth(part);
                current += hp.Current;
                max += hp.Maximum;
            }

            float ratio = current / max;
            if (ratio < _moar.PersonalityProfile.RetreatThreshold)
            {
                Vector3 retreat = _bot.Position - _bot.Memory.GoalEnemy.CurrPosition;
                Vector3 fallback = _bot.Position + retreat.normalized * 10f;

                Say(EPhraseTrigger.MumblePhrase);
                _bot.GoToPoint(fallback, false);
                _groupSync?.BroadcastExtractPoint(fallback);

                // Fallback sync: teammates follow
                if (_bot.BotsGroup != null)
                {
                    for (int i = 0; i < _bot.BotsGroup.MembersCount; i++)
                    {
                        var other = _bot.BotsGroup.Member(i);
                        if (other != null && other != _bot && Vector3.Distance(other.Position, _bot.Position) < RETREAT_RADIUS)
                        {
                            float cohesion = _moar.PersonalityProfile.Cohesion;
                            if (Random.value < cohesion)
                                other.GoToPoint(fallback, false);
                        }
                    }
                }
            }
        }

        private void EnsureDoorOpened(Vector3 target)
        {
            Vector3 direction = (target - _bot.Position).normalized;
            if (Physics.Raycast(_bot.Position, direction, out RaycastHit hit, 2f))
            {
                if (hit.collider.TryGetComponent<Door>(out var door) && door.DoorState != EDoorState.Open)
                {
                    door.Interact(new InteractionResult(EInteractionType.Open));
                }
            }
        }

        private void Say(EPhraseTrigger phrase)
        {
            _bot?.BotTalk?.TrySay(phrase);
        }
    }
}
