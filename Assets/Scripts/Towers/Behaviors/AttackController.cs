using UnityEngine;
using Zenject;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Towers.Behaviors.Targeting;
using Gameplay.Towers.Behaviors.Weapons; // Не забываем неймспейс экзекуторов

namespace Gameplay.Towers.Behaviors
{
    public class AttackController : ITowerBehavior
    {
        private readonly WeaponAdapter _adapter;
        private readonly IInstantiator _instantiator; // Сохраняем инстанциатор
        private TowerFacade _facade;
        
        private Transform _currentTarget;
        private float _cooldownTimer;
        private AttackStats _currentStats;

        private ITargetingStrategy _targetingStrategy;
        
        // Делаем свойство публичным, чтобы Адаптер мог рисовать по нему Gizmos
        public IAimStrategy AimStrategy { get; private set; }
        
        // Наш новый чистый экзекутор
        private IAttackExecutor _executor;

        public AttackController(WeaponAdapter adapter, IInstantiator instantiator)
        {
            _adapter = adapter;
            _instantiator = instantiator;
        }

        public void Initialize(TowerFacade facade)
        {
            _facade = facade;
            _cooldownTimer = 0f;
            _currentStats = _facade.GetCurrentStats().Attack;
            
            _targetingStrategy = _currentStats.Targeting switch
            {
                TargetingType.Closest => new ClosestTargetStrategy(),
                _ => new ClosestTargetStrategy()
            };

            // Передаем все физические параметры в стратегии прицеливания
            AimStrategy = _currentStats.Aiming switch
            {
                AimingType.Horizontal => new HorizontalAimStrategy(
                    _adapter.transform, _adapter.FirePoint, _adapter.EnvironmentMask, 
                    _currentStats.MinPitch, _currentStats.MaxPitch, _currentStats.FieldOfView, _currentStats.CheckLineOfSight),
                    
                AimingType.Omni => new OmniAimStrategy(
                    _adapter.transform, _adapter.FirePoint, _adapter.EnvironmentMask, 
                    _currentStats.MinPitch, _currentStats.MaxPitch, _currentStats.FieldOfView, _currentStats.CheckLineOfSight),
                    
                _ => new OmniAimStrategy(_adapter.transform, _adapter.FirePoint, _adapter.EnvironmentMask, -10f, 80f, 360f, false)
            };

            // 1. ФАБРИКА ЭКЗЕКУТОРОВ: Собираем оружие из конфига
            _executor = _currentStats.Executor switch
            {
                ExecutorType.Hitscan => new HitscanExecutor(),
                ExecutorType.Projectile => new ProjectileExecutor(_currentStats.ProjectilePrefab, _instantiator),
                _ => new HitscanExecutor()
            };

            _adapter.TriggerBuildStarted();
        }

        public void Tick(float deltaTime)
        {
            if (_currentStats == null || _targetingStrategy == null) return;

            _cooldownTimer -= deltaTime;

            if (!_targetingStrategy.IsTargetValid(_currentTarget, _adapter.LogicalRotator, _currentStats, AimStrategy))
            {
                _currentTarget = _targetingStrategy.FindTarget(_adapter.LogicalRotator, _currentStats, _adapter.EnemyMask, AimStrategy);
            }

            if (_currentTarget != null)
            {
                AimStrategy?.AimAtTarget(_adapter.LogicalRotator, _currentTarget, _adapter.TurnSpeed);

                bool isFacing = AimStrategy == null || AimStrategy.IsFacingTarget(_adapter.LogicalRotator, _currentTarget);

                if (_cooldownTimer <= 0f && isFacing)
                {
                    ExecuteShot();
                    _cooldownTimer = _currentStats.Cooldown;
                }
            }
        }

        private void ExecuteShot()
        {
            if (_currentStats.PayloadStrategy == null) return;

            DamagePayload damagePayload = new DamagePayload(_currentStats.Damage, _currentStats.Type);
            IProjectilePayload payload = _currentStats.PayloadStrategy.CreatePayload(damagePayload);
            
            // Дергаем наш свежий POCO-экзекутор
            _executor?.ExecuteAttack(_currentTarget, payload, _adapter.FirePoint);
            
            _adapter.TriggerShotFired(_currentTarget.position);
        }

        



    }
}