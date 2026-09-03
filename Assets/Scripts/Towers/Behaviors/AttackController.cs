using UnityEngine;
using Gameplay.Towers.Data.Modules;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Towers.Behaviors
{
    public class AttackController : ITowerBehavior
    {
        private readonly WeaponAdapter _adapter;
        private TowerFacade _facade;
        
        private Transform _currentTarget;
        private float _cooldownTimer;
        private AttackStats _currentStats;

        // Контроллер принимает свой Адаптер через конструктор
        public AttackController(WeaponAdapter adapter)
        {
            _adapter = adapter;
        }

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            _currentStats = _facade.GetCurrentStats().Attack;
            
            _adapter.TriggerBuildStarted();
        }

        public void Tick(float deltaTime)
        {
            if (_currentStats == null || _adapter.Targeting == null) return;

            _cooldownTimer -= deltaTime;

            // 1. Поиск цели через адаптер
            if (!_adapter.Targeting.IsTargetValid(_currentTarget, _adapter.LogicalRotator, _currentStats.Range, _adapter.Aiming))
            {
                _currentTarget = _adapter.Targeting.FindTarget(_adapter.LogicalRotator, _currentStats.Range, _adapter.EnemyMask, _adapter.Aiming);
            }

            // 2. Логика стрельбы
            if (_currentTarget != null)
            {
                _adapter.Aiming?.AimAtTarget(_adapter.LogicalRotator, _currentTarget, _adapter.TurnSpeed);

                bool isFacing = _adapter.Aiming == null || _adapter.Aiming.IsFacingTarget(_adapter.LogicalRotator, _currentTarget);

                if (_cooldownTimer <= 0f && isFacing)
                {
                    ExecuteShot();
                    _cooldownTimer = _currentStats.Cooldown;
                }
            }
        }

        private void ExecuteShot()
        {
            if (_currentStats.PayloadStrategy == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[AttackController] Нет PayloadStrategy на башне {_facade.name}!");
#endif
                return;
            }

            // Формируем новую Data-Driven посылку (DamagePayload)
            DamagePayload damagePayload = new DamagePayload(_currentStats.Damage, _currentStats.Type);
            IProjectilePayload payload = _currentStats.PayloadStrategy.CreatePayload(damagePayload);
            
            _adapter.Executor?.ExecuteAttack(_currentTarget, payload, _adapter.FirePoint);
            _adapter.TriggerShotFired(_currentTarget.position);
        }
    }
}