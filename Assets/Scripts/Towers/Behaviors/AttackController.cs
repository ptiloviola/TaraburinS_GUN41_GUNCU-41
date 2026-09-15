using UnityEngine;
using Zenject;
using Gameplay.Towers.Data;
using Gameplay.Towers.Data.Modules;
using Gameplay.Core;
using Gameplay.Projectiles.Contracts;
using Gameplay.Towers.Behaviors.Aiming;
using Gameplay.Towers.Behaviors.Targeting;
using Gameplay.Towers.Behaviors.Weapons;
using Gameplay.Projectiles.Factories;

namespace Gameplay.Towers.Behaviors
{
    public class AttackController : ITowerBehavior
    {
        private readonly WeaponAdapter _adapter;
        
        private readonly ProjectileFactory _projectileFactory; 
        
        private TowerFacade _facade;
        private Transform _currentTarget;
        private float _cooldownTimer;
        private AttackStats _currentStats;

        private ITargetingStrategy _targetingStrategy;
        
        public IAimStrategy AimStrategy { get; private set; }
        
        private IAttackExecutor _executor;

        public AttackController(WeaponAdapter adapter, ProjectileFactory projectileFactory)
        {
            _adapter = adapter;
            _projectileFactory = projectileFactory;
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

            AimStrategy = _currentStats.Aiming switch
            {
                AimingType.Horizontal => new HorizontalAimStrategy(
                    _adapter.transform, _adapter.FirePoint, _adapter.EnvironmentMask, 
                    _currentStats.MinPitch, _currentStats.MaxPitch, _currentStats.FieldOfView, _currentStats.CheckLineOfSight),
                    
                AimingType.DualAxis => new DualAxisAimStrategy(
                    _adapter.transform, _adapter.ElevationPivot, _adapter.FirePoint, _adapter.EnvironmentMask, 
                    _currentStats.MinPitch, _currentStats.MaxPitch, _currentStats.FieldOfView, _currentStats.CheckLineOfSight),
                    
                AimingType.Omni => new OmniAimStrategy(
                    _adapter.transform, _adapter.FirePoint, _adapter.EnvironmentMask, 
                    _currentStats.MinPitch, _currentStats.MaxPitch, _currentStats.FieldOfView, _currentStats.CheckLineOfSight),
                    
                _ => new OmniAimStrategy(_adapter.transform, _adapter.FirePoint, _adapter.EnvironmentMask, -10f, 80f, 360f, false)
            };

            _executor = _currentStats.Executor switch
            {
                ExecutorType.Hitscan => new HitscanExecutor(),
                ExecutorType.Projectile => new ProjectileExecutor(_currentStats.ProjectilePrefab, _projectileFactory),
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
            
            _executor?.ExecuteAttack(_currentTarget, payload, _adapter.FirePoint);
            
            _adapter.TriggerShotFired(_currentTarget.position);
        }
    }
}