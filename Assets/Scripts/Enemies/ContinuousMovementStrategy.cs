using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies
{
    public enum MovementType { PathOnly, FreeRoam }

    public class ContinuousMovementStrategy : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private readonly Vector3 _targetPosition;
        private EnemyFacade _enemy;
        
        private readonly MovementType _movementType;
        private readonly string _pathAreaName;
        private readonly string _groundAreaName;

        public ContinuousMovementStrategy(
            Vector3 targetPosition, 
            MovementType movementType, 
            string pathAreaName, 
            string groundAreaName)
        {
            _targetPosition = targetPosition;
            _movementType = movementType;
            _pathAreaName = pathAreaName;
            _groundAreaName = groundAreaName;
        }

        public void Initialize(EnemyFacade enemy)
        {
            _enemy = enemy;
            _agent = _enemy.Agent; 

            if (_agent != null)
            {
                _agent.enabled = true;

                int pathArea = NavMesh.GetAreaFromName(_pathAreaName);
                int groundArea = NavMesh.GetAreaFromName(_groundAreaName);

                if (pathArea == -1 || groundArea == -1)
                {
#if UNITY_EDITOR
                    Debug.LogError($"[ContinuousMovement] Зоны '{_pathAreaName}' или '{_groundAreaName}' не найдены!");
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
        }

        public void Tick(float deltaTime)
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) 
                return;
            // Динамически применяем множитель скорости из статусов
            _agent.speed = _enemy.Config.Movement.MoveSpeed * _enemy.StatusController.SpeedMultiplier;
        
        }
    }
}