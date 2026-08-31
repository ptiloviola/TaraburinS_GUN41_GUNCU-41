using System;
using UnityEngine;
using UnityEngine.AI;
using Gameplay.Enemies.Data.Movement;

namespace Gameplay.Enemies
{
    public class DiscreteMovementStrategy : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private readonly Vector3 _targetPosition;
        private readonly DiscreteMovementConfig _config;

        private float _timer;
        private bool _isJumping;

        public event Action OnJumpStart;

        // Константы для зон
        private const string CustomPathArea = "CustomPath";
        private const string CustomGroundArea = "CustomGround";

        public DiscreteMovementStrategy(Vector3 targetPosition, DiscreteMovementConfig config)
        {
            _targetPosition = targetPosition;
            _config = config;
        }

        public void Initialize(EnemyFacade enemy)
        {
            _agent = enemy.Agent;
            if (_agent != null)
            {
                _agent.enabled = true;

                // --- ИСПРАВЛЕНИЕ 1: ВОЗВРАЩАЕМ МАСКУ ЗОН ---
                int pathArea = NavMesh.GetAreaFromName(CustomPathArea);
                int groundArea = NavMesh.GetAreaFromName(CustomGroundArea);

                if (pathArea == -1 || groundArea == -1)
                {
                    pathArea = 0; groundArea = 0;
                }

                if (_config.PathingType == MovementType.PathOnly)
                    _agent.areaMask = (1 << pathArea);
                else
                    _agent.areaMask = (1 << pathArea) | (1 << groundArea);

                _agent.SetDestination(_targetPosition);
                
                // --- ИСПРАВЛЕНИЕ 2: УБИРАЕМ ИНЕРЦИЮ ---
                // Чтобы агент не скользил, он должен разгоняться и тормозить моментально
                _agent.acceleration = 10000f;
                _agent.angularSpeed = 1000f; // Резкие повороты
                _agent.autoBraking = false;

                // Скорость рывка должна компенсировать простой
                float totalCycleTime = _config.JumpDuration + _config.PauseDuration;
                float speedMultiplier = totalCycleTime / _config.JumpDuration;
                _agent.speed = enemy.Config.Movement.MoveSpeed * speedMultiplier;

                // Стартуем с паузы
                _isJumping = false;
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero; // Жестко гасим скорость
                _timer = _config.PauseDuration;
            }
        }

        public void Tick(float deltaTime)
        {
            if (_agent == null || !_agent.isActiveAndEnabled || !_agent.isOnNavMesh) return;

            _timer -= deltaTime;

            if (_isJumping)
            {
                if (_timer <= 0f)
                {
                    _isJumping = false;
                    _agent.isStopped = true; 
                    _agent.velocity = Vector3.zero; // ИСПРАВЛЕНИЕ 2: Принудительный стоп без скольжения!
                    _timer = _config.PauseDuration;
                }
            }
            else
            {
                if (_timer <= 0f)
                {
                    _isJumping = true;
                    _agent.isStopped = false; 
                    _timer = _config.JumpDuration;
                    
                    OnJumpStart?.Invoke(); 
                }
            }
        }
    }
}