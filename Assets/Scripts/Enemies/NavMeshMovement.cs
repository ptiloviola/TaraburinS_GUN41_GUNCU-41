using UnityEngine;
using UnityEngine.AI;

namespace Gameplay.Enemies
{
    public class NavMeshMovement : IMovementStrategy
    {
        private NavMeshAgent _agent;
        private Vector3 _targetPosition;

        // Передаем целевую точку через конструктор
        public NavMeshMovement(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }

        public void Initialize(Transform enemyTransform)
        {
            _agent = enemyTransform.GetComponent<NavMeshAgent>();

            if (_agent != null)
            {
                _agent.enabled = true;
                _agent.SetDestination(_targetPosition);
            }
            else
            {
                Debug.LogError($"[NavMeshMovement] На объекте {enemyTransform.name} отсутствует компонент NavMeshAgent!");
            }
        }

        public void Tick(float deltaTime)
        {
            // Здесь можно обрабатывать специфичную логику, 
            // например, проверять, дошел ли враг до финиша.
            
            if (_agent != null && !_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        // Враг дошел до базы! (Логику отправки сигнала сделаем чуть позже)
                    }
                }
            }
        }
    }
}


