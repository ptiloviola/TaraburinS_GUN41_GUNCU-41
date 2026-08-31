using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies
{
    // Перенесли Enum из Фасада сюда, так как это относится к логике движения
    public enum MovementType { PathOnly, FreeRoam }

    public class NavMeshMovement : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private readonly Vector3 _targetPosition;
        private EnemyFacade _enemy;
        private readonly MovementType _movementType;

        // Избавляемся от магических строк!
        private const string CustomPathArea = "CustomPath";
        private const string CustomGroundArea = "CustomGround";

        // Передаем настройки через конструктор (в будущем MovementType будем брать из конфига)
        public NavMeshMovement(Vector3 targetPosition, MovementType movementType = MovementType.PathOnly)
        {
            _targetPosition = targetPosition;
            _movementType = movementType;
        }

        public void Initialize(EnemyFacade enemy)
        {
            _enemy = enemy;
            // У нашего фасада уже есть публичный геттер для агента, используем его
            _agent = _enemy.Agent; 

            if (_agent != null)
            {
                _agent.enabled = true;

                int pathArea = NavMesh.GetAreaFromName(CustomPathArea);
                int groundArea = NavMesh.GetAreaFromName(CustomGroundArea);

                // Защита: если зоны в Unity не настроены, движок вернет -1. 
                if (pathArea == -1 || groundArea == -1)
                {
#if UNITY_EDITOR
                    Debug.LogError($"[NavMeshMovement] Ошибка: Зоны '{CustomPathArea}' или '{CustomGroundArea}' не найдены в Navigation!");
#endif
                    pathArea = 0;
                    groundArea = 0;
                }

                if (_movementType == MovementType.PathOnly)
                {
                    _agent.areaMask = (1 << pathArea);
                }
                else
                {
                    _agent.areaMask = (1 << pathArea) | (1 << groundArea);
                }
                
                _agent.SetDestination(_targetPosition);
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError($"[NavMeshMovement] На объекте {_enemy.gameObject.name} отсутствует NavMeshAgent!");
#endif
            }
        }

        public void Tick(float deltaTime)
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) 
                return;
            
            // Здесь в будущем может быть логика проверки замедлений или динамического перестроения маршрута
        }
    }
}