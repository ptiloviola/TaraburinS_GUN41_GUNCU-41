using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies.Movement.Strategies
{
    public enum MovementType { PathOnly, FreeRoam }

    public class ContinuousMovementStrategy : IMovementStrategy
    {
        private readonly Vector3 _targetPosition;
        private NavMeshAgent _agent;
        
        private readonly MovementType _movementType;
        private readonly string _pathAreaName;
        private readonly string _groundAreaName;

        public ContinuousMovementStrategy(
            Vector3 targetPosition, MovementType movementType, string pathAreaName, string groundAreaName)
        {
            _targetPosition = targetPosition;
            _movementType = movementType;
            _pathAreaName = pathAreaName;
            _groundAreaName = groundAreaName;
        }

        public void Initialize(NavMeshAgent agent)
        {
            _agent = agent;

            int pathArea = NavMesh.GetAreaFromName(_pathAreaName);
            int groundArea = NavMesh.GetAreaFromName(_groundAreaName);

            if (pathArea == -1 || groundArea == -1)
            {
                pathArea = 0;
                groundArea = 0;
            }

            if (_movementType == MovementType.PathOnly)
                _agent.areaMask = (1 << pathArea);
            else
                _agent.areaMask = (1 << pathArea) | (1 << groundArea);
            
            if (_agent.isOnNavMesh)
                _agent.SetDestination(_targetPosition);
            else
                Gameplay.Tools.GameLogger.LogError("[ContinuousMovement] Агент не на NavMesh!");
        }

        public void Tick(float deltaTime)
        {
            
        }

        public void UpdateSpeed(float baseSpeed, float multiplier)
        {
            if (_agent != null)
            {
                _agent.speed = baseSpeed * multiplier;
            }
        }
    }
}