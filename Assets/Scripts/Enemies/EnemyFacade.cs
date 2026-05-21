using System.Diagnostics.Tracing;
using UnityEngine;
using Zenject;

namespace Gameplay.Enemies
{
    public class EnemyFacade : MonoBehaviour, IPoolable<IMovementStrategy, IMemoryPool>
    {
        private IMovementStrategy _movementStrategy;
        private IMemoryPool _pool;

        private bool _isActive;

        private void Update()
        {
            // Выполняем логику ТОЛЬКО если враг активен
            if (_isActive && _movementStrategy != null)
            {
                _movementStrategy.Tick(Time.deltaTime);
            }
        }      


        // Этот метод вызывается Zenject в момент, когда враг "оживает" из пула
        public void OnSpawned(IMovementStrategy movementStrategy, IMemoryPool pool)
        {
            
            _pool = pool;
            _isActive = true; // Враг ожил

            // 1. ЖЕСТКО ВКЛЮЧАЕМ ОБЪЕКТ ПРИ ПОЯВЛЕНИИ ИЗ ПУЛА
            gameObject.SetActive(true);

            // Если при спавне пришел movementStrategy (например, из старого кода), 
            // можем его инициализировать, но теперь мы будем делать это явно через метод ниже
            if (movementStrategy != null)
            {
                InitializeMovement(movementStrategy);
            }

            Debug.Log($"[EnemyPool] Враг {gameObject.name} успешно заспавнен из пула.");
        }

        public void InitializeMovement(IMovementStrategy movementStrategy)
        {
            _movementStrategy = movementStrategy;
            _movementStrategy.Initialize(this);
        }



        // Метод для возврата врага обратно в пул
        public void Despawn()
        {
            // Сигнальная ракета 1: Дошел ли сигнал сюда?
            Debug.Log($"<color=orange>[EnemyFacade] Запрос на деспавн. _isActive: {_isActive}, _pool_exists: {_pool != null}</color>");

            if (_isActive && _pool != null)
            {
                _isActive = false; // Блокируем повторные вызовы
                _pool.Despawn(this); // Отдаем команду Zenject
            }
            else if (_pool == null)
            {
                // План "Б": Если пул по какой-то причине потерян (чтобы сферы не зависали)
                Debug.LogError($"<color=red>[EnemyFacade] ОШИБКА: Пул потерян! Жестко уничтожаем объект {gameObject.name}</color>");
                gameObject.SetActive(false); // Выключаем из сцены
                Destroy(gameObject); // Уничтожаем насовсем, раз пул сломался
            }
        }

        // Вызывается Zenject, когда враг возвращается в пул (очищаем ссылки)
        public void OnDespawned()
        {
            // Сигнальная ракета 2: Подхватил ли Zenject команду?
            Debug.Log($"<color=orange>[EnemyFacade] Zenject вызвал OnDespawned. Отключаем агента и объект.</color>");
            
            var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
            if (agent != null) 
            {
                agent.enabled = false;
            }

            _movementStrategy = null;
            _pool = null;
            _isActive = false;

            // Жестко выключаем визуал и физику
            gameObject.SetActive(false);
        }

        // Вложенный класс пула, который мы зарегистрируем в Zenject
        public class Pool : MonoMemoryPool<IMovementStrategy, IMemoryPool, EnemyFacade> {}

    }
}


