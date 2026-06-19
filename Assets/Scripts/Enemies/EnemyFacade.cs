using UnityEngine;
using Zenject;
using Gameplay.Core;
using Infrastructure.Signals;
using Gameplay.Enemies.Data;
using UnityEngine.AI;

namespace Gameplay.Enemies
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyFacade : MonoBehaviour
    {
        private IMovementStrategy _movementStrategy;
        private Pool _pool;
        private SignalBus _signalBus;
        private NavMeshAgent _agent;

        // --- НОВОЕ: Ссылка на здоровье ---
        [SerializeField] private HealthComponent _health;

        // --- НОВОЕ: Статический счетчик ---
        private static int _spawnCounter = 0;

        // Перечисление типов навигации врага
        public enum MovementType { PathOnly, FreeRoam }
        
        [Header("Настройки навигации")]
        [SerializeField] private MovementType _movementType = MovementType.PathOnly;

        // Геттер, чтобы стратегия движения могла прочитать этот режим
        public MovementType EnemyMovementType => _movementType;
        // НОВОЕ: Свойство для доступа к конфигу (понадобится базе для расчета урона)
        public EnemyConfig Config { get; private set; }

        // Магия Zenject: он сам вставит сюда ссылку на пул при инстанцировании префаба!
        [Inject]
        public void Construct(SignalBus signalBus)
        {
            
            _signalBus = signalBus;
        }

        // 2. НОВОЕ: Добавь этот метод. Режиссер вызовет его при спавне.
        public void SetPool(Pool pool)
        {
            _pool = pool;
        }

        // --- НОВОЕ: Ищем компонент, если забыли назначить в инспекторе ---
        private void Awake()
        {
            // Кешируем компонент один раз при рождении объекта
            _agent = GetComponent<NavMeshAgent>();
            if (_health == null) _health = GetComponent<HealthComponent>();
        }

        // --- НОВОЕ: Подготовка врага при доставании из пула ---
        private void OnEnable()
        {
            Debug.Log($"Родился {gameObject.name}");
            // Каждое появление из пула увеличивает счетчик и меняет имя объекта
            _spawnCounter++;
            gameObject.name = $"Enemy_{_spawnCounter}";
            
            if (_health != null)
            {
                // ИСПРАВЛЕНО: _health.Initialize() убрано отсюда, оно будет в InitConfig
                _health.OnDied += HandleDeath; // Подписываемся на смерть
            }
        }

        // --- НОВОЕ: Отписка при возврате в пул (защита от утечек памяти) ---
        private void OnDisable()
        {
            if (_health != null)
            {
                _health.OnDied -= HandleDeath;
            }
        }

        // НОВОЕ: Метод инициализации из конфига. Вызывается Режиссером при спавне.
        public void InitConfig(EnemyConfig config)
        {
            Config = config;
            if (_health != null)
            {
                _health.Initialize(config.MaxHealth);
            }
            _agent.speed = Config.MoveSpeed;
            // добавить ангулар спид как у защитника

        }



        public void InitializeMovement(IMovementStrategy movementStrategy)
        {
            _movementStrategy = movementStrategy;
            _movementStrategy.Initialize(this);
        }

        private void Update()
        {
            // Обновляем логику только если стратегия назначена
            if (_movementStrategy != null)
            {
                _movementStrategy.Tick(Time.deltaTime);
            }
        }

        // --- НОВОЕ: Обработчик смерти ---
        private void HandleDeath()
        {
            int reward = Config != null ? Config.RewardMoney : 10;
            _signalBus.Fire(new SignalEnemyKilled { Reward = reward });
            Despawn(); // Если ХП упало до нуля, просто возвращаем врага в пул
        }

        public void Despawn()
        {
            if (_pool != null)
            {
                // Очищаем логику движения перед возвратом в пул
                _movementStrategy = null;
                
                
                
                _agent.enabled = false;
                

                // MonoMemoryPool сам сделает gameObject.SetActive(false)!
                _pool.Despawn(this);
            }
            else
            {
                Debug.LogError($"[EnemyFacade] Пул потерян, жестко удаляем {gameObject.name}");
                Destroy(gameObject);
            }
        }

        // Простой, чистый пул без лишних параметров
        public class Pool : MonoMemoryPool<EnemyFacade> { }
    }
}