using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies
{
    public class DynamicFlightMovementStrategy : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private Transform _agentTransform;
        private readonly Vector3 _targetPosition;
        
        private readonly MovementType _movementType;
        private readonly string _pathAreaName;
        private readonly string _groundAreaName;

        private readonly float _baseHeight;
        private readonly float _amplitude;
        private readonly float _frequency;
        
        private readonly float _takeoffDistance;
        private readonly float _landingDistance;
        
        private float _timePhaseOffset;
        private Vector3 _startPosition;

        public DynamicFlightMovementStrategy(
            Vector3 targetPosition, MovementType movementType, string pathAreaName, string groundAreaName, 
            float baseHeight, float amplitude, float frequency, float takeoffDist, float landingDist)
        {
            _targetPosition = targetPosition;
            _movementType = movementType;
            _pathAreaName = pathAreaName;
            _groundAreaName = groundAreaName;
            
            _baseHeight = baseHeight;
            _amplitude = amplitude;
            _frequency = frequency;
            
            _takeoffDistance = takeoffDist;
            _landingDistance = landingDist;
        }

        public void Initialize(NavMeshAgent agent)
        {
            _agent = agent; 
            _agentTransform = _agent.transform;
            _timePhaseOffset = Random.Range(0f, 100f);
            
            _startPosition = _agentTransform.position;

            if (_agent != null)
            {
                _agent.baseOffset = 0f;

                int pathArea = NavMesh.GetAreaFromName(_pathAreaName);
                int groundArea = NavMesh.GetAreaFromName(_groundAreaName);

                if (pathArea == -1) pathArea = 0;
                if (groundArea == -1) groundArea = 0;

                _agent.areaMask = _movementType == MovementType.PathOnly 
                    ? (1 << pathArea) 
                    : (1 << pathArea) | (1 << groundArea);
                
                if (!_agent.isOnNavMesh)
                {
                    _agent.Warp(_agentTransform.position);
                }

                if (_agent.isOnNavMesh)
                {
                    _agent.SetDestination(_targetPosition);
                }
            }
        }

        public void Tick(float deltaTime)
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) 
                return;
            

            float currentWaveOffset = Mathf.Sin((Time.time + _timePhaseOffset) * _frequency) * _amplitude;
            float targetHeight = _baseHeight + currentWaveOffset;

            float heightMultiplier = 1f;
            
            Vector2 currentPosXZ = new Vector2(_agentTransform.position.x, _agentTransform.position.z);
            Vector2 startPosXZ = new Vector2(_startPosition.x, _startPosition.z);
            Vector2 targetPosXZ = new Vector2(_targetPosition.x, _targetPosition.z);

            float distFromStart = Vector2.Distance(currentPosXZ, startPosXZ);
            float distToTarget = Vector2.Distance(currentPosXZ, targetPosXZ);

            if (distFromStart < _takeoffDistance)
            {
                heightMultiplier = distFromStart / _takeoffDistance;
            }
            else if (distToTarget < _landingDistance)
            {
                heightMultiplier = distToTarget / _landingDistance;
            }
            
            _agent.baseOffset = targetHeight * heightMultiplier;
        }
    }
}