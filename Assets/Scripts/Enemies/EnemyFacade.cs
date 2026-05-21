using UnityEngine;
using Zenject;

namespace Gameplay.Enemies
{
    public class EnemyFacade : MonoBehaviour
    {
        private IMovementStrategy _movementStrategy;
        private Pool _pool;

        // Магия Zenject: он сам вставит сюда ссылку на пул при инстанцировании префаба!
        [Inject]
        public void Construct(Pool pool)
        {
            _pool = pool;
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