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
        
        // Буфер для оптимизированного поиска целей (NonAlloc)
        private readonly Collider[] _hitColliders = new Collider[5];

        public void Initialize(EnemyConfig config)
        {
            _config = config as MeleeEnemyConfig;
            
            // Ищем перехватчик событий на дочернем объекте с аниматором
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
            // Запускаем анимацию удара. Саму физику урона обсчитает метод DealDamage, 
            // когда сработает Animation Event
            animator?.PlayMeleeAttack();
        }

        private void DealDamage()
        {
            if (_config == null) return;

            // Бьем перед собой (на дистанции 1 метр)
            Vector3 strikeCenter = transform.position + Vector3.up * 1f + transform.forward * 1f;
            float strikeRadius = 1.2f;

            int hits = Physics.OverlapSphereNonAlloc(strikeCenter, strikeRadius, _hitColliders, _config.TargetMask);

            for (int i = 0; i < hits; i++)
            {
                if (_hitColliders[i].TryGetComponent(out IDamageable targetDamageable))
                {
                    targetDamageable.TakeDamage(_config.MeleeDamage);
                    
                    // Если хочешь, можно добавить звук удара (шлепок по плоти)
                    // _audioService?.PlaySFX("Enemy_Hit_Flesh", strikeCenter);
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
            // Отписываемся, чтобы мертвый враг не наносил урон, 
            // если его убили прямо во время замаха
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