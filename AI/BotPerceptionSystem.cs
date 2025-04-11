using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace MOAR.AI
{
    public class BotPerceptionSystem
    {
        private readonly BotOwner _botOwner;
        private float lastFlashTime;
        private float lastLoudSoundTime;

        private bool isBlindedInternal;
        private bool isDeafenedInternal;
        private bool shouldPanicInternal;

        private const float FLASH_DURATION = 5f;
        private const float SOUND_DURATION = 6f;
        private const float SOUND_RADIUS = 14f;
        private const float FLASHLIGHT_RADIUS = 10f;
        private const float FLASHLIGHT_ANGLE = 35f;
        private const float LASER_RADIUS = 12f;
        private const float LASER_ANGLE = 30f;

        public BotPerceptionSystem(BotOwner bot)
        {
            _botOwner = bot;

            var player = bot?.GetPlayer;
            if (player != null && player.IsAI)
            {
                BotRegistry.Register(player);
            }
        }

        public void Update()
        {
            if (_botOwner?.HealthController == null || !_botOwner.HealthController.IsAlive)
                return;

            EvaluateFlashExposure();
            DetectFlashlightBlindness();
            DetectLaserExposure();
            EvaluateSoundExposure();
            ApplyEffects();
            EvaluatePanicState();
        }

        public void Dispose()
        {
            var player = _botOwner?.GetPlayer;
            if (player != null)
            {
                BotRegistry.Unregister(player);
            }
        }

        private void EvaluateFlashExposure()
        {
            if (_botOwner.FlashGrenade?.IsFlashed == true && !HasEyeProtection())
            {
                lastFlashTime = Time.time;
                isBlindedInternal = true;
                PanicNow(_botOwner.Position);
                Say(EPhraseTrigger.OnFight);
            }

            if (isBlindedInternal && Time.time - lastFlashTime > FLASH_DURATION)
                isBlindedInternal = false;
        }

        private void DetectFlashlightBlindness()
        {
            Collider[] hits = Physics.OverlapSphere(_botOwner.Position, FLASHLIGHT_RADIUS);
            foreach (var hit in hits)
            {
                if (!hit.gameObject.name.ToLower().Contains("flashlight"))
                    continue;

                Vector3 toLight = hit.transform.position - _botOwner.Position;
                float angle = Vector3.Angle(_botOwner.Transform.forward, toLight);
                if (angle < FLASHLIGHT_ANGLE && HasLineOfSight(hit.transform.position) && !HasEyeProtection())
                {
                    lastFlashTime = Time.time;
                    isBlindedInternal = true;
                    PanicNow(hit.transform.position);
                    Say(EPhraseTrigger.OnFight);
                }
            }
        }

        private void DetectLaserExposure()
        {
            Collider[] hits = Physics.OverlapSphere(_botOwner.Position, LASER_RADIUS);
            foreach (var hit in hits)
            {
                if (!hit.name.ToLower().Contains("laser"))
                    continue;

                Vector3 toLaser = hit.transform.position - _botOwner.Position;
                float angle = Vector3.Angle(_botOwner.Transform.forward, toLaser);
                if (angle < LASER_ANGLE && HasLineOfSight(hit.transform.position))
                {
                    PanicNow(hit.transform.position);
                    Say(EPhraseTrigger.OnFight);
                }
            }
        }

        private void EvaluateSoundExposure()
        {
            Vector3? soundPos = SoundController.Instance?.AudioListenerTransform?.position;
            if (soundPos == null) return;

            float dist = Vector3.Distance(_botOwner.Position, soundPos.Value);
            bool hasEarPro = HasItem(EquipmentSlot.Headwear) && IsHeadsetEquipped();

            if (dist <= SOUND_RADIUS && !hasEarPro)
            {
                lastLoudSoundTime = Time.time;
                isDeafenedInternal = true;
                PanicNow(soundPos.Value);
                Say(EPhraseTrigger.OnFight);
            }

            if (isDeafenedInternal && Time.time - lastLoudSoundTime > SOUND_DURATION)
                isDeafenedInternal = false;
        }

        private void ApplyEffects()
        {
            if (isBlindedInternal && _botOwner.Memory?.GoalEnemy != null)
            {
                _botOwner.Memory.GoalEnemy = null;
            }
        }

        private void EvaluatePanicState()
        {
            if (_botOwner.Memory == null)
                return;

            bool inCover = _botOwner.Memory.IsInCover;
            bool underFire = _botOwner.Memory.IsUnderFire;

            shouldPanicInternal = (isBlindedInternal || isDeafenedInternal) && (!inCover || underFire);

            if (shouldPanicInternal)
            {
                if (_botOwner.BotsGroup?.MembersCount > 1)
                {
                    for (int i = 0; i < _botOwner.BotsGroup.MembersCount; i++)
                    {
                        BotOwner teammate = _botOwner.BotsGroup.Member(i);
                        if (teammate != null && teammate != _botOwner)
                        {
                            teammate.Memory?.SetPanicPoint(
                                new PlaceForCheck(_botOwner.Position, PlaceForCheckType.danger), true
                            );
                        }
                    }
                }

                string role = _botOwner.Profile.Info.Settings.Role.ToString().ToLowerInvariant();
                if (role.Contains("assault") || role.Contains("boss"))
                {
                    var scatter = _botOwner.Settings?.FileSettings?.Scattering;
                    if (scatter != null)
                    {
                        scatter.WorkingScatter += 0.25f;
                        scatter.MinScatter += 0.05f;
                    }
                }

                Vector3 fallbackPosition = _botOwner.Position;

                var covers = UnityEngine.Object.FindObjectOfType<AICoversData>();
                if (covers != null)
                {
                    var voxel = AICoversData.smethod_0(covers, _botOwner.Position);
                    if (voxel != null)
                    {
                        var point = covers.GetClosestsPointInVoxelesExtended(
                            voxel.IndexX, voxel.IndexY, voxel.IndexZ, 2,
                            _botOwner.Position, null
                        );

                        if (point != null)
                        {
                            fallbackPosition = point.Position;
                        }
                    }
                }

                if (_botOwner.BotsGroup != null)
                {
                    Vector3 center = _botOwner.Position;
                    int count = 0;
                    for (int i = 0; i < _botOwner.BotsGroup.MembersCount; i++)
                    {
                        var member = _botOwner.BotsGroup.Member(i);
                        if (member != null)
                        {
                            center += member.Position;
                            count++;
                        }
                    }
                    if (count > 0)
                    {
                        center /= (count + 1);
                        fallbackPosition = Vector3.Lerp(fallbackPosition, center, 0.35f);
                    }
                }

                var nav = new NavPoint(0, fallbackPosition);
                var groupPoint = new GroupPoint(
                    0,
                    nav,
                    fallbackPosition,
                    null,
                    null,
                    (CoverLevel)0,
                    false,
                    Vector3.up,
                    Vector3.forward,
                    (PointWithNeighborType)0
                );
                var navPoint = new CustomNavigationPoint(groupPoint);
                _botOwner.Mover?.GoToPoint(navPoint, false);

                _botOwner.Memory.SetPanicPoint(new PlaceForCheck(_botOwner.Position, PlaceForCheckType.danger), true);
            }
        }

        private void PanicNow(Vector3 dangerPoint)
        {
            _botOwner.Memory?.SetPanicPoint(new PlaceForCheck(dangerPoint, PlaceForCheckType.danger), true);
        }

        private void Say(EPhraseTrigger phrase)
        {
            _botOwner?.GetPlayer?.Say(phrase);
        }

        private bool HasItem(EquipmentSlot slot)
        {
            return _botOwner.GetPlayer?.Profile?.Inventory?.Equipment?.GetSlot(slot)?.ContainedItem != null;
        }

        private bool HasEyeProtection()
        {
            var eyewear = _botOwner.GetPlayer?.Profile?.Inventory?.Equipment?.GetSlot(EquipmentSlot.Eyewear)?.ContainedItem;
            return eyewear != null && !eyewear.Name.ToLowerInvariant().Contains("none");
        }

        private bool IsHeadsetEquipped()
        {
            var item = _botOwner.GetPlayer?.Profile?.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear)?.ContainedItem;
            if (item == null) return false;
            string name = item.Name.ToLowerInvariant();
            return name.Contains("headset") || name.Contains("comtac") || name.Contains("sordin");
        }

        private bool HasLineOfSight(Vector3 to)
        {
            Vector3 dir = to - _botOwner.Position;
            if (Physics.Raycast(_botOwner.Position, dir.normalized, out RaycastHit hit, dir.magnitude))
            {
                return hit.collider != null && hit.collider.gameObject.transform.position == to;
            }
            return true;
        }

        private bool IsBrightLightInFront()
        {
            Collider[] hits = Physics.OverlapSphere(_botOwner.Position, 8f);
            foreach (var hit in hits)
            {
                if (!hit.name.ToLower().Contains("flashlight"))
                    continue;

                Vector3 toLight = hit.transform.position - _botOwner.Position;
                float angle = Vector3.Angle(_botOwner.Transform.forward, toLight);
                if (angle < 35f && HasLineOfSight(hit.transform.position))
                    return true;
            }
            return false;
        }

        public bool IsBlinded => isBlindedInternal;
        public bool IsDeafened => isDeafenedInternal;
        public bool ShouldPanic => shouldPanicInternal;
    }
}
