using UnityEngine;
using TpsShooter.Combat;

namespace TpsShooter.Player.Combat
{
    public class PlayerMeleeController
    {
        private readonly Animator _animator;
        private readonly Transform _playerTransform;
        
        private static readonly int MeleeTriggerHash = Animator.StringToHash("MeleeHit");
        
        // Настройки удара (в будущем можно вынести в конфиг)
        private readonly float _meleeRange = 1.0f;  // Как далеко бьем
        private readonly float _meleeRadius = 0.8f; // Ширина удара (чтобы легко попадать)
        private readonly float _meleeDamage = 25f;  // Урон
        private readonly LayerMask _hitMask;

        private readonly Collider[] _hitColliders = new Collider[5];

        public PlayerMeleeController(Animator animator, Transform playerTransform)
        {
            _animator = animator;
            _playerTransform = playerTransform;
            
            // Бьем по всему, кроме самого игрока
            _hitMask = ~LayerMask.GetMask("Ignore Raycast", "Player");
        }

        public void TryMeleeAttack()
        {
            // Запускаем анимацию удара
            _animator.SetTrigger(MeleeTriggerHash);
        }

        // Этот метод будет вызван в тот самый кадр, когда рука летит вперед
        public void PerformStrike()
        {
            // Центр сферы для удара (чуть впереди игрока и на уровне груди)
            Vector3 strikeCenter = _playerTransform.position + Vector3.up * 1f + _playerTransform.forward * _meleeRange;

            int hits = Physics.OverlapSphereNonAlloc(strikeCenter, _meleeRadius, _hitColliders, _hitMask);

            bool hitSomething = false;

            for (int i = 0; i < hits; i++)
            {
                if (_hitColliders[i].TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_meleeDamage);
                    hitSomething = true;
                    
#if UNITY_EDITOR
                    // ИСПРАВЛЕНИЕ: Берем имя у самого коллайдера, а не у интерфейса
                    DevLogger.Log($"<color=red>[Melee]</color> Удар по {_hitColliders[i].gameObject.name}! Урон: {_meleeDamage}");
#endif
                }
            }

#if UNITY_EDITOR
            // Рисуем крестик на месте удара, чтобы тебе было легко настраивать радиус
            Debug.DrawRay(strikeCenter, Vector3.up * _meleeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.down * _meleeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.left * _meleeRadius, Color.red, 2f);
            Debug.DrawRay(strikeCenter, Vector3.right * _meleeRadius, Color.red, 2f);
            
            if (!hitSomething) DevLogger.Log("<color=red>[Melee]</color> Взмах по воздуху!");
#endif
        }
    }
}