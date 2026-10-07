using System;
using UnityEngine;
using UnityEngine.AI;
using Gameplay.Enemies.Data.Movement;

namespace Gameplay.Enemies.Movement.Strategies
{
    public class DiscreteMovementStrategy : IMovementStrategy, IPhasedMovementNotifier
    {
        private NavMeshAgent _agent;
        private readonly Vector3 _targetPosition;
        private readonly DiscreteMovementConfig _config;

        private float _timer;
        private bool _isJumping;

        public event Action OnMovementPhaseStarted;
        public event Action OnPausePhaseStarted;

        private const string CustomPathArea = "CustomPath";
        private const string CustomGroundArea = "CustomGround";

        public DiscreteMovementStrategy(Vector3 targetPosition, DiscreteMovementConfig config)
        {
            _targetPosition = targetPosition;
            _config = config;
        }

        public void Initialize(NavMeshAgent agent)
        {
            _agent = agent;
            
            if (_agent != null)
            {
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
                
                _agent.acceleration = 10000f;
                _agent.angularSpeed = 1000f; 
                _agent.autoBraking = false;

                _isJumping = false;
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero; 
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
                    _agent.velocity = Vector3.zero; 
                    _timer = _config.PauseDuration;
                    
                    OnPausePhaseStarted?.Invoke();
                }
            }
            else
            {
                if (_timer <= 0f)
                {
                    _isJumping = true;
                    _agent.isStopped = false; 
                    _timer = _config.JumpDuration;
                    
                    OnMovementPhaseStarted?.Invoke(); 
                }
            }
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