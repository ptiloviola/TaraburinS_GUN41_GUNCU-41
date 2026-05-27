using UnityEngine;
using Zenject;
using Gameplay.Core;

namespace Gameplay.Enemies
{


    public class EnemyFacade : MonoBehaviour
    {
        private IMovementStrategy _movementStrategy;
        private Pool _pool;

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

        // Магия Zenject: он сам вставит сюда ссылку на пул при инстанцировании префаба!
        [Inject]
        public void Construct(Pool pool)
        {
            _pool = pool;
        }

        // --- НОВОЕ: Ищем компонент, если забыли назначить в инспекторе ---
        private void Awake()
        {
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
                _health.Initialize(); // Восстанавливаем 100% ХП
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
            Despawn(); // Если ХП упало до нуля, просто возвращаем врага в пул
        }

        public void Despawn()
        {
            if (_pool != null)
            {
                // Очищаем логику движения перед возвратом в пул
                _movementStrategy = null;
                
                var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) 
                {
                    agent.enabled = false;
                }

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