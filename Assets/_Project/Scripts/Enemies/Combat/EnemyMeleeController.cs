using UnityEngine;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Core;
using TpsShooter.Core; 
using TpsShooter.Combat;

namespace TpsShooter.Enemies.Combat
{
    public class EnemyMeleeController : MonoBehaviour, IEnemyCombatHandler
    {
        private const float StrikeUpOffset = 1f;
        private const float StrikeForwardOffset = 1f;
        private const float StrikeRadius = 2.5f;

        private MeleeEnemyConfig _config;
        private CharacterAnimationEvents _animEvents;
        
        private readonly Collider[] _hitColliders = new Collider[5];

        public void Initialize(EnemyConfig config)
        {
            _config = config as MeleeEnemyConfig;
            
            _animEvents = GetComponentInChildren<CharacterAnimationEvents>();
            
            if (_animEvents != null)
            {
                _animEvents.OnMeleeStrike += DealDamage;
            }
            else
            {
                DevLogger.LogWarning($"<color=yellow>[EnemyMelee]</color> Не найден CharacterAnimationEvents на враге {gameObject.name}");
            }
        }

        public void PerformAttack(PlayerFacade target, EnemyAnimator animator)
        {
            if (_config != null && _config.AttackAnimStates.Length > 0)
            {
                string randomAttack = _config.AttackAnimStates[Random.Range(0, _config.AttackAnimStates.Length)];
                
                if (string.IsNullOrWhiteSpace(randomAttack))
                {
                    Debug.LogError($"<color=red>[EnemyMelee]</color> ОШИБКА: В конфиге {_config.name} пустое имя анимации атаки!");
                    return;
                }

                animator?.PlayAttack(randomAttack, _config.AttackAnimDuration); 
            }
        }

        private void DealDamage()
        {
            if (_config == null) return;

            Vector3 strikeCenter = transform.position + Vector3.up * StrikeUpOffset + transform.forward * StrikeForwardOffset;

            int hits = Physics.OverlapSphereNonAlloc(strikeCenter, StrikeRadius, _hitColliders, _config.TargetMask);

            for (int i = 0; i < hits; i++)
            {
                if (_hitColliders[i].TryGetComponent(out IDamageable targetDamageable))
                {
                    targetDamageable.TakeDamage(_config.MeleeDamage);
                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(strikeCenter, Vector3.up * StrikeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.down * StrikeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.left * StrikeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.right * StrikeRadius, Color.red, 2f);
#endif
        }

        public void OnDeath()
        {
            if (_animEvents != null)
            {
                _animEvents.OnMeleeStrike -= DealDamage;
            }
        }

        private void OnDestroy()
        {
            if (_animEvents != null)
            {
                _animEvents.OnMeleeStrike -= DealDamage;
            }
        }
    }
}