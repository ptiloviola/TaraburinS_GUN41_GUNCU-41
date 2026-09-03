using UnityEngine;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Towers.Behaviors.Targeting;

namespace Gameplay.Towers.Behaviors
{
    public class AttackController : ITowerBehavior
    {
        private readonly WeaponAdapter _adapter;
        private TowerFacade _facade;
        
        private Transform _currentTarget;
        private float _cooldownTimer;
        private AttackStats _currentStats;

        // Чистые стратегии живут прямо в памяти контроллера
        private ITargetingStrategy _targetingStrategy;
        private IAimStrategy _aimStrategy;

        public AttackController(WeaponAdapter adapter)
        {
            _adapter = adapter;
        }

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            _currentStats = _facade.GetCurrentStats().Attack;
            
            // 1. Фабрика стратегий на лету (читаем из конфига)
            _targetingStrategy = _currentStats.Targeting switch
            {
                TargetingType.Closest => new ClosestTargetStrategy(),
                _ => new ClosestTargetStrategy()
            };

            _aimStrategy = _currentStats.Aiming switch
            {
                AimingType.Horizontal => new HorizontalAimStrategy(15f), 
                AimingType.Omni => new OmniAimStrategy(),
                _ => new OmniAimStrategy()
            };

            _adapter.TriggerBuildStarted();
        }

        public void Tick(float deltaTime)
        {
            // ИСПРАВЛЕНО: Проверяем локальную стратегию, а не адаптер!
            if (_currentStats == null || _targetingStrategy == null) return;

            _cooldownTimer -= deltaTime;

            // 2. Делегируем поиск чистой стратегии
            if (!_targetingStrategy.IsTargetValid(_currentTarget, _adapter.LogicalRotator, _currentStats.Range, _aimStrategy))
            {
                _currentTarget = _targetingStrategy.FindTarget(_adapter.LogicalRotator, _currentStats.Range, _adapter.EnemyMask, _aimStrategy);
            }

            // 3. Цель найдена — наводимся и стреляем
            if (_currentTarget != null)
            {
                _aimStrategy?.AimAtTarget(_adapter.LogicalRotator, _currentTarget, _adapter.TurnSpeed);

                bool isFacing = _aimStrategy == null || _aimStrategy.IsFacingTarget(_adapter.LogicalRotator, _currentTarget);

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

            // Собираем Data-Driven урон и стреляем
            DamagePayload damagePayload = new DamagePayload(_currentStats.Damage, _currentStats.Type);
            IProjectilePayload payload = _currentStats.PayloadStrategy.CreatePayload(damagePayload);
            
            _adapter.Executor?.ExecuteAttack(_currentTarget, payload, _adapter.FirePoint);
            _adapter.TriggerShotFired(_currentTarget.position);
        }
    }
}