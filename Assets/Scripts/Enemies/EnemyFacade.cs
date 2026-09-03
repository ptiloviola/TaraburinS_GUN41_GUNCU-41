using System;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Core;
using Infrastructure.Signals;
using Gameplay.Enemies.Data;
using Gameplay.Enemies.FSM;
using Gameplay.Base;

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
        public NavMeshAgent Agent => _agent;
        public HealthComponent Health => _health;
        public SignalBus SignalBus => _signalBus; // Чтобы стейты могли кидать сигналы
        public IMovementStrategy MovementStrategy => _movementStrategy;
        // --- СОБЫТИЯ ДЛЯ ВИЗУАЛА ---
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
            
            // Включаем физику при спавне
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
            
            // ОБЯЗАТЕЛЬНО ставим знак вопроса ?. 
            // Zenject вызовет OnDisable при добавлении в пул ДО того, как вызовется InitializeMovement
            _stateMachine?.Cleanup(); 
            
            // Защита от утечек
            OnStateChanged = null;
        }

        public void InitConfig(EnemyConfig config)
        {
            Config = config;
            
            // Создаем чистую бизнес-логику
            _armorCalculator = new ArmorCalculator(config.Armor);
            
            if (_health != null)
            {
                _health.Initialize(config.Stats.MaxHealth);
            }
            
            // Передаем зависимости в ресивер урона
            if (_damageReceiver != null)
            {
                _damageReceiver.Initialize(_health, _armorCalculator);
            }

            _agent.speed = Config.Movement.MoveSpeed;
        }

        public void InitializeMovement(IMovementStrategy movementStrategy)
        {
            _movementStrategy = movementStrategy;
            _movementStrategy.Initialize(this);

            // 1. Создаем чистую машину при каждом доставании из пула
            _stateMachine = new EnemyStateMachine();
            
            // 2. ПЕРЕНЕСЛИ СЮДА: Подписываемся на события только когда машина уже существует
            _stateMachine.OnStateChanged += state => OnStateChanged?.Invoke(state);

            // 3. Добавляем стейты
            _stateMachine.AddState(new MoveState(this, _movementStrategy));
            _stateMachine.AddState(new DeathState(this));
            _stateMachine.AddState(new ReachedBaseState(this));
            
            // 4. Стартуем
            _stateMachine.ChangeState(EnemyStateType.Move);
        }

        private void Update()
        {
            _stateMachine.Tick(Time.deltaTime);
        }

        private void HandleDeath()
        {
            // Теперь Фасад не удаляет себя сам, он просто говорит машине: "Я умер"
            // А DeathState отыграет анимацию и вызовет Despawn
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

            Gameplay.Base.BaseCore baseCore = other.GetComponentInParent<Gameplay.Base.BaseCore>();

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