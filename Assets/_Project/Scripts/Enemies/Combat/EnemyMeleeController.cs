using UnityEngine;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Enemies.Core;
using TpsShooter.Core; // Для CharacterAnimationEvents
using TpsShooter.Combat;

namespace TpsShooter.Enemies.Combat
{
    public class EnemyMeleeController : MonoBehaviour, IEnemyCombatHandler
    {
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
                Debug.LogWarning($"<color=yellow>[EnemyMelee]</color> Не найден CharacterAnimationEvents на враге {gameObject.name}");
            }
        }

        public void PerformAttack(PlayerFacade target, EnemyAnimator animator)
        {
            // ИСПРАВЛЕНИЕ: Берем случайный удар из нашего массива в конфиге!
            if (_config != null && _config.AttackAnimTriggers.Length > 0)
            {
                string randomAttack = _config.AttackAnimTriggers[Random.Range(0, _config.AttackAnimTriggers.Length)];
                animator?.PlayCustomMelee(randomAttack, _config.AttackCooldown); 
                // Передаем кулдаун, чтобы аниматор знал, сколько длится блокировка стейта
            }
        }

        private void DealDamage()
        {
            if (_config == null) return;

            // Бьем перед собой (на дистанции 1 метр)
            Vector3 strikeCenter = transform.position + Vector3.up * 1f + transform.forward * 1f;
            float strikeRadius = 2.5f;

            int hits = Physics.OverlapSphereNonAlloc(strikeCenter, strikeRadius, _hitColliders, _config.TargetMask);

            for (int i = 0; i < hits; i++)
            {
                if (_hitColliders[i].TryGetComponent(out IDamageable targetDamageable))
                {
                    targetDamageable.TakeDamage(_config.MeleeDamage);
                }
            }

#if UNITY_EDITOR
            Debug.DrawRay(strikeCenter, Vector3.up * strikeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.down * strikeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.left * strikeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.right * strikeRadius, Color.red, 2f);
#endif
        }

        public void OnDeath()
        {
            // Отписываемся, чтобы мертвый враг не наносил урон
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