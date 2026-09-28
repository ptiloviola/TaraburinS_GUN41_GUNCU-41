using System;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Combat;
using Gameplay.Infrastructure.Signals;
using Gameplay.Enemies.Data;
using Gameplay.Enemies.FSM;
using Gameplay.Base;
using Gameplay.Enemies.Statuses;

namespace Gameplay.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyFacade : MonoBehaviour
    {
        [Header("Компоненты")]
        [SerializeField] private HealthComponent _health;
        [SerializeField] private NavMeshAgent _agent;
        [SerializeField] private Collider _collider;
        [SerializeField] private DamageReceiver _damageReceiver;

        private Pool _pool;
        private SignalBus _signalBus;
        private EnemyStateMachine _stateMachine;
        private IMovementStrategy _movementStrategy;

        private ArmorCalculator _armorCalculator;

        private static int _spawnCounter = 0;

        public EnemyConfig Config { get; private set; }
        public TargetType TargetType => Config.Type;
        public NavMeshAgent Agent => _agent;
        public HealthComponent Health => _health;
        public SignalBus SignalBus => _signalBus;
        public IMovementStrategy MovementStrategy => _movementStrategy;
        public EnemyStatusController StatusController { get; private set; }

        public bool IsDead => _stateMachine != null && 
                     (_stateMachine.CurrentStateType == EnemyStateType.Death || 
                      _stateMachine.CurrentStateType == EnemyStateType.ReachedBase);



        public event Action<EnemyStateType> OnStateChanged;

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
        }

        public void SetPool(Pool pool)
        {
            _pool = pool;
        }

        private void OnEnable()
        {
            _spawnCounter++;
            gameObject.name = $"Enemy_{_spawnCounter}";
            
            _collider.enabled = true;
            _agent.enabled = true;

            if (_health != null)
            {
                _health.OnDied += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDied -= HandleDeath;
            }

            StatusController?.Cleanup();
            
            _stateMachine?.Cleanup(); 
            
            OnStateChanged = null;
        }

        public void InitConfig(EnemyConfig config)
        {
            Config = config;
            
            _armorCalculator = new ArmorCalculator(config.Armor);
            
            if (_health != null)
            {
                _health.Initialize(config.Stats.MaxHealth);
            }
            
            if (_damageReceiver != null)
            {
                _damageReceiver.Initialize(_health, _armorCalculator);
            }

            _agent.speed = Config.Stats.MoveSpeed;
        }

        public void InitializeMovement(IMovementStrategy movementStrategy)
        {
            _movementStrategy = movementStrategy;
            _movementStrategy.Initialize(this);

            _stateMachine = new EnemyStateMachine();
            
            _stateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);

            StatusController = new EnemyStatusController(this);

            _stateMachine.AddState(new MoveState(this, _movementStrategy));
            _stateMachine.AddState(new DeathState(this));
            _stateMachine.AddState(new ReachedBaseState(this));
            
            
            _stateMachine.ChangeState(EnemyStateType.Move);
        }

        private void Update()
        {
            StatusController?.Tick(Time.deltaTime);
            _stateMachine.Tick(Time.deltaTime);
        }

        private void HandleDeath()
        {
            _stateMachine.ChangeState(EnemyStateType.Death);
        }

        public void ForceDespawn()
        {
            if (_pool != null)
            {
                _agent.enabled = false;
                _pool.Despawn(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_stateMachine.CurrentStateType == EnemyStateType.ReachedBase || 
                _stateMachine.CurrentStateType == EnemyStateType.Death) return;

            BaseCore baseCore = other.GetComponentInParent<BaseCore>();

            if (baseCore != null)
            {
                int damage = Config.Stats.DamageToBase;
                Debug.Log($"<color=orange>[EnemyFacade] {gameObject.name} коснулся базы! Пытаемся нанести {damage} урона.</color>");
                
                baseCore.TakeDamage(damage);
                
                _signalBus.Fire<SignalEnemyReachedBase>();
                _stateMachine.ChangeState(EnemyStateType.ReachedBase);
            }
        }

        public class Pool : MonoMemoryPool<EnemyFacade> { }
    }
}