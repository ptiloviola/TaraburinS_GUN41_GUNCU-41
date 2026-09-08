using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies
{
    public class DynamicFlightMovementStrategy : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private readonly Vector3 _targetPosition;
        private EnemyFacade _enemy;
        
        private readonly MovementType _movementType;
        private readonly string _pathAreaName;
        private readonly string _groundAreaName;

        private readonly float _baseHeight;
        private readonly float _amplitude;
        private readonly float _frequency;
        
        private readonly float _takeoffDistance;
        private readonly float _landingDistance;
        
        private float _timePhaseOffset;
        private Vector3 _startPosition; // Запоминаем точку старта для расчета взлета

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

        public void Initialize(EnemyFacade enemy)
        {
            _enemy = enemy;
            _agent = _enemy.Agent; 
            _timePhaseOffset = Random.Range(0f, 100f);
            
            // Фиксируем координаты точки спавна
            _startPosition = _enemy.transform.position;

            if (_agent != null)
            {
                // Начинаем с высоты 0 (с земли), чтобы красиво взлететь
                _agent.baseOffset = 0f;
                _agent.enabled = true;

                int pathArea = NavMesh.GetAreaFromName(_pathAreaName);
                int groundArea = NavMesh.GetAreaFromName(_groundAreaName);

                if (pathArea == -1) pathArea = 0;
                if (groundArea == -1) groundArea = 0;

                _agent.areaMask = _movementType == MovementType.PathOnly 
                    ? (1 << pathArea) 
                    : (1 << pathArea) | (1 << groundArea);
                
                if (!_agent.isOnNavMesh)
                {
                    _agent.Warp(_enemy.transform.position);
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
            
            _agent.speed = _enemy.Config.Movement.MoveSpeed * _enemy.StatusController.SpeedMultiplier;
            
            // 1. Считаем рабочую высоту с учетом синусоиды
            float currentWaveOffset = Mathf.Sin((Time.time + _timePhaseOffset) * _frequency) * _amplitude;
            float targetHeight = _baseHeight + currentWaveOffset;

            // 2. Логика взлета и посадки (Множитель от 0.0 до 1.0)
            float heightMultiplier = 1f;
            
            // ИСПРАВЛЕНИЕ: Считаем дистанцию только по плоскости XZ, игнорируя текущую высоту агента
            Vector2 currentPosXZ = new Vector2(_enemy.transform.position.x, _enemy.transform.position.z);
            Vector2 startPosXZ = new Vector2(_startPosition.x, _startPosition.z);
            Vector2 targetPosXZ = new Vector2(_targetPosition.x, _targetPosition.z);

            float distFromStart = Vector2.Distance(currentPosXZ, startPosXZ);
            float distToTarget = Vector2.Distance(currentPosXZ, targetPosXZ);

            if (distFromStart < _takeoffDistance)
            {
                // Плавно растем от 0 до 1 на старте
                heightMultiplier = distFromStart / _takeoffDistance;
            }
            else if (distToTarget < _landingDistance)
            {
                // Плавно падаем от 1 до 0 при подлете к базе
                heightMultiplier = distToTarget / _landingDistance;
            }

            // 3. Применяем итоговую высоту к агенту
            _agent.baseOffset = targetHeight * heightMultiplier;
        }
    }
}