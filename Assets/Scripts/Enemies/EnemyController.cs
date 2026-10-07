using System;
using UnityEngine;
using UnityEngine.AI;
using Gameplay.Enemies.Data;
using Gameplay.Enemies.FSM;
using Gameplay.Combat;
using Gameplay.Combat.Statuses;
using Gameplay.Base;
using Gameplay.Enemies.Visuals;
using Gameplay.Enemies.Statuses;
using Gameplay.Enemies.Services;
using Gameplay.Enemies.Combat;
using Gameplay.Enemies.Movement;


namespace Gameplay.Enemies
{
    public class EnemyController
    {
        private readonly HealthComponent _health;
        private readonly NavMeshAgent _agent;
        private readonly Collider _collider;
        private readonly DamageReceiver _damageReceiver;
        private readonly EnemyVisualsBase _visuals;
        private readonly EnemyStateMachineFactory _fsmFactory;
        private readonly BaseArrivalHandler _baseArrivalHandler;

        private Transform _transform;
        private EnemyConfig _config;
        private EnemyStateMachine _stateMachine;
        private EnemyStatusController _statusController;
        private IMovementStrategy _movementStrategy;

        public bool IsDespawned { get; private set; } = true;
        public bool IsTargetable => !IsDespawned && _stateMachine != null && _stateMachine.CurrentStateType != EnemyStateType.Death && _stateMachine.CurrentStateType != EnemyStateType.ReachedBase;
        public TargetType TargetType => _config.Type;
        public Vector3 Velocity => _agent != null ? _agent.velocity : Vector3.zero;

        public event Action<EnemyStateType> OnStateChanged;
        public event Action OnDespawnRequested;

        public EnemyController(
            HealthComponent health,
            NavMeshAgent agent,
            Collider collider,
            DamageReceiver damageReceiver,
            EnemyVisualsBase visuals,
            EnemyStateMachineFactory fsmFactory,
            BaseArrivalHandler baseArrivalHandler)
        {
            _health = health;
            _agent = agent;
            _collider = collider;
            _damageReceiver = damageReceiver;
            _visuals = visuals;
            _fsmFactory = fsmFactory;
            _baseArrivalHandler = baseArrivalHandler;
        }

        public void Initialize(Transform transform, EnemyConfig config, IMovementStrategy movement, Vector3 spawnPosition)
        {
            Deactivate();

            _transform = transform;
            _config = config;
            IsDespawned = false;

            if (_agent.enabled) _agent.enabled = false;
            _transform.position = spawnPosition + Vector3.up * _agent.baseOffset;
            _transform.gameObject.name = $"Enemy_{_config.name}"; 

            _collider.enabled = true;
            _agent.enabled = true;

            ArmorCalculator armorCalculator = new ArmorCalculator(config.Armor);
            _health.Initialize(config.Stats.MaxHealth);
            _damageReceiver.Initialize(_health, armorCalculator);

            _statusController = new EnemyStatusController(_transform.gameObject); 
            _statusController.Initialize(type => _config.GetResistMultiplier(type));

            _movementStrategy = movement;
            _movementStrategy.Initialize(_agent);

            _health.OnDied += HandleDeath;

            _stateMachine = _fsmFactory.Create(_transform, RequestDespawn, _agent, _collider, _visuals, _config, _movementStrategy);
            _stateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);
            
            _stateMachine.ChangeState(EnemyStateType.Move);
        }

        public void Tick(float dt)
        {
            if (IsDespawned) return;

            _statusController?.Tick(dt);
            
            if (IsDespawned) return; 

            _stateMachine?.Tick(dt);

            if (IsTargetable)
            {
                _movementStrategy.UpdateSpeed(_config.Stats.MoveSpeed, _statusController.SpeedMultiplier);
            }
        }

        public void ApplyStatus(IStatusEffect effect)
        {
            if (IsTargetable) _statusController?.AddStatus(effect);
        }

        public void TryReachBase(BaseCore baseCore)
        {
            if (!IsTargetable) return;

            _stateMachine.ChangeState(EnemyStateType.ReachedBase);
            _baseArrivalHandler.ProcessArrival(baseCore, _config);
        }

        private void HandleDeath()
        {
            _health.OnDied -= HandleDeath;
            _stateMachine.ChangeState(EnemyStateType.Death);
        }

        public void RequestDespawn()
        {
            if (IsDespawned) return;
            
            Deactivate();
            OnDespawnRequested?.Invoke();
        }

        public void Deactivate()
        {
            if (IsDespawned) return;
            IsDespawned = true;

            _health.OnDied -= HandleDeath;
            if (_collider != null) _collider.enabled = false;
            if (_agent != null && _agent.isOnNavMesh) _agent.enabled = false;

            _statusController?.Cleanup();
            _stateMachine?.Cleanup();
        }
    }
}