using System;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Enemies.Data;
using Gameplay.Enemies.FSM;
using Gameplay.Base;
using Gameplay.Enemies.Statuses;
using Gameplay.Enemies.Data.Death;
using Gameplay.Combat;
using Gameplay.Enemies.Visuals;
using Gameplay.Combat.Statuses;

namespace Gameplay.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyFacade : MonoBehaviour
    {
        [Header("Внутренние компоненты")]
        [SerializeField] private HealthComponent _health;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Collider _collider;
        [SerializeField] private DamageReceiver _damageReceiver;

        private SignalBus _signalBus;
        private EnemyStateMachine _stateMachine;
        private IMovementStrategy _movementStrategy;
        private ArmorCalculator _armorCalculator;
        private EnemyConfig _config;
        private EnemyVisualsBase _visuals;
        
        private EnemyStatusController _statusController; 

        public TargetType TargetType => _config.Type;
        public bool IsTargetable => _stateMachine != null && _stateMachine.CurrentStateType != EnemyStateType.Death && _stateMachine.CurrentStateType != EnemyStateType.ReachedBase;
        public Vector3 Position => transform.position;
        public Vector3 Velocity => _agent != null ? _agent.velocity : Vector3.zero;

        public event Action<EnemyStateType> OnStateChanged;
        public event Action<EnemyFacade> OnDespawnRequested;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void Awake()
        {
            if (_agent == null) _agent = GetComponent<NavMeshAgent>();
            if (_health == null) _health = GetComponent<HealthComponent>();
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_damageReceiver == null) _damageReceiver = GetComponent<DamageReceiver>();
            _visuals = GetComponent<EnemyVisualsBase>();

            _statusController = new EnemyStatusController(this);
        }

        public void Initialize(EnemyConfig config, IMovementStrategy movementStrategy, Vector3 spawnPosition)
        {
            _config = config;
            
            if (_agent.enabled) _agent.enabled = false;
            
            transform.position = spawnPosition + Vector3.up * _agent.baseOffset;
            gameObject.name = $"Enemy_{_config.name}"; 

            _collider.enabled = true;
            _agent.enabled = true;

            _armorCalculator = new ArmorCalculator(config.Armor);
            _health.Initialize(config.Stats.MaxHealth);
            _damageReceiver.Initialize(_health, _armorCalculator);

            _statusController.Initialize(type => _config.GetResistMultiplier(type));

            _movementStrategy = movementStrategy;
            _movementStrategy.Initialize(_agent);
            
            _health.OnDied -= HandleDeath;
            _health.OnDied += HandleDeath;

            _stateMachine?.Cleanup();
            _stateMachine = new EnemyStateMachine();
            _stateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);

            _stateMachine.AddState(new MoveState(this, _movementStrategy));
            _stateMachine.AddState(new DeathState(
                facade: this, 
                agent: _agent, 
                collider: _collider, 
                deathBehavior: _config.DeathBehavior, 
                signalBus: _signalBus, 
                visuals: _visuals, 
                rewardMoney: _config.Stats.RewardMoney
            ));
            _stateMachine.AddState(new ReachedBaseState(this, _collider));
            
            _stateMachine.ChangeState(EnemyStateType.Move);
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            _statusController.Tick(dt);
            _stateMachine.Tick(dt);
            
            if (IsTargetable)
            {
                UpdateMovementSpeed();
            }
        }

        private void UpdateMovementSpeed()
        {
            _agent.speed = _config.Stats.MoveSpeed * _statusController.SpeedMultiplier;
        }

        private void OnDisable()
        {
            _health.OnDied -= HandleDeath;
            _statusController.Cleanup();
            _stateMachine?.Cleanup();
        }

        private void HandleDeath()
        {
            _stateMachine.ChangeState(EnemyStateType.Death);
            _signalBus.Fire<SignalEnemyKilled>(); 
        }

        public void RequestDespawn()
        {
            _health.OnDied -= HandleDeath;
            _collider.enabled = false;
            _agent.enabled = false;
            OnDespawnRequested?.Invoke(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsTargetable) return;

            BaseCore baseCore = other.GetComponentInParent<BaseCore>();
            if (baseCore != null)
            {
                baseCore.TakeDamage(_config.Stats.DamageToBase);
                _signalBus.Fire<SignalEnemyReachedBase>();
                _stateMachine.ChangeState(EnemyStateType.ReachedBase);
            }
        }

        public void ApplyStatus(IStatusEffect effect) => _statusController.AddStatus(effect);

        public T GetMovementCapability<T>() where T : class
        {
            return _movementStrategy as T;
        }

        public class Pool : MonoMemoryPool<EnemyFacade> { }
    }
}