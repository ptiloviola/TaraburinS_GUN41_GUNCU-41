using System.Diagnostics.Tracing;
using UnityEngine;
using Zenject;

namespace Gameplay.Enemies
{
    public class EnemyFacade : MonoBehaviour, IPoolable<IMovementStrategy, IMemoryPool>
    {
        private IMovementStrategy _movementStrategy;
        private IMemoryPool _pool;

        private void Update()
        {
            // Обновляем логику движения
            _movementStrategy?.Tick(Time.deltaTime);
        }      


        // Этот метод вызывается Zenject в момент, когда враг "оживает" из пула
        public void OnSpawned(IMovementStrategy movementStrategy, IMemoryPool pool)
        {
            
            _pool = pool;

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
            _movementStrategy.Initialize(transform);
        }



        // Метод для возврата врага обратно в пул
        public void Despawn()
        {
            _pool.Despawn(this);
        }

        // Вызывается Zenject, когда враг возвращается в пул (очищаем ссылки)
        public void OnDespawned()
        {
            _movementStrategy = null;
            _pool = null;
            Debug.Log($"[EnemyPool] Враг {gameObject.name} вернулся в пул.");
        }

        // Вложенный класс пула, который мы зарегистрируем в Zenject
        public class Pool : MonoMemoryPool<IMovementStrategy, IMemoryPool, EnemyFacade> {}

    }
}


