using UnityEngine;
using TpsShooter.Combat;
using TpsShooter.Player.Configs;


namespace TpsShooter.Player.Combat
{
    public class PlayerMeleeController
    {
        private const int MaxMeleeHits = 5;

        private readonly Animator _animator;
        private readonly Transform _playerTransform;
        private readonly PlayerConfig _config;
        
        private static readonly int MeleeTriggerHash = Animator.StringToHash("MeleeHit");
        private readonly Collider[] _hitColliders = new Collider[MaxMeleeHits];


        public PlayerMeleeController(Animator animator, Transform playerTransform, PlayerConfig config)
        {
            _animator = animator;
            _playerTransform = playerTransform;
            _config = config;
        }

        public void TryMeleeAttack()
        {
            _animator.SetTrigger(MeleeTriggerHash);
        }

        public void PerformStrike()
        {
            Vector3 strikeCenter = _playerTransform.position + Vector3.up * _config.MeleeHeightOffset + _playerTransform.forward * _config.MeleeRange;

            int hits = Physics.OverlapSphereNonAlloc(strikeCenter, _config.MeleeRadius, _hitColliders, _config.MeleeHitMask);

            bool hitSomething = false;

            for (int i = 0; i < hits; i++)
            {
                if (_hitColliders[i].TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.MeleeDamage);
                    hitSomething = true;
                    
                    DevLogger.Log($"<color=red>[Melee]</color> Удар по {_hitColliders[i].gameObject.name}! Урон: {_config.MeleeDamage}");

                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(strikeCenter, Vector3.up * _config.MeleeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.down * _config.MeleeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.left * _config.MeleeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.right * _config.MeleeRadius, Color.red, 2f);
            
            if (!hitSomething) DevLogger.Log("<color=red>[Melee]</color> Взмах по воздуху!");
#endif
        }
    }
}