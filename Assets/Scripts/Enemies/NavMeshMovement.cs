using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies
{
    public class NavMeshMovement : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private Vector3 _targetPosition;
        private EnemyFacade _enemy; // Ссылка на фасад

        // Передаем целевую точку через конструктор
        public NavMeshMovement(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        public void Initialize(EnemyFacade enemy)
        {
            _enemy = enemy;
            _agent = _enemy.GetComponent<NavMeshAgent>();

            if (_agent != null)
            {
                _agent.enabled = true;

                // Получаем индексы наших кастомных зон
                int pathArea = UnityEngine.AI.NavMesh.GetAreaFromName("CustomPath");
                int groundArea = UnityEngine.AI.NavMesh.GetAreaFromName("CustomGround");

                // Настраиваем маску навигации с помощью битовых сдвигов (1 << индекс_зоны)
                if (_enemy.EnemyMovementType == EnemyFacade.MovementType.PathOnly)
                {
                    // Агент видит ТОЛЬКО дорогу
                    _agent.areaMask = (1 << pathArea);
                }
                else
                {
                    // Агент видит и дорогу, и обычную землю вокруг
                    _agent.areaMask = (1 << pathArea) | (1 << groundArea);
                }
                
                _agent.SetDestination(_targetPosition);
            }
            else
            {
                Debug.LogError($"[NavMeshMovement] На объекте {_enemy.name} отсутствует компонент NavMeshAgent!");
            }
        }

        public void Tick(float deltaTime)
        {
            // // Здесь можно обрабатывать специфичную логику, 
            // // например, проверять, дошел ли враг до финиша.
            
            // if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            // {
            //     return;
            // }
            // if (_agent.pathPending)
            // {
            //     return;
            // }
            
            // if (_agent.remainingDistance <= 1.0f)
            // {
            //     if (!_agent.hasPath || _agent.velocity.sqrMagnitude < 1f)
            //     {
            //         // Враг дошел до базы! 
            //         // Отключаем агент, чтобы он не пытался двигаться в пуле
                    
            //         Debug.Log($"<color=green>[NavMeshMovement] Враг {_enemy.gameObject.name} успешно достиг цели!</color>");
                    
            //         _agent.enabled = false; 
                    
            //         // Возвращаем врага в пул (он исчезнет со сцены)
            //         _enemy.Despawn();
            //     }
            // }

            // Оставляем только базовую защиту от ошибок.
            // Больше мы не проверяем remainingDistance!
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) 
                return;
            
        }
    }
}


