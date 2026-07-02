using UnityEngine;
using UnityEngine.AI;

namespace MeatMushrooms.Wolf.Components
{
    // Гарантирует, что Unity не даст удалить NavMeshAgent с префаба
    [RequireComponent(typeof(NavMeshAgent))] 
    public class WolfLocomotion : MonoBehaviour
    {
        private NavMeshAgent _agent;
        
        // Возвращает текущую физическую скорость волка (от 0 до значения Speed в NavMeshAgent)
        public float CurrentSpeed => _agent.velocity.magnitude;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        // Команда идти в точку
        public void MoveTo(Vector3 targetPosition)
        {
            _agent.isStopped = false;
            _agent.SetDestination(targetPosition);
        }

        // Команда остановиться
        public void Stop()
        {
            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero; // Гасим инерцию
            }
        }

        // Удобный метод для состояний, чтобы понять, пришли мы или нет
        public bool HasReachedDestination()
        {
            // Проверяем, не в процессе ли мы расчета пути, и достигли ли дистанции остановки
            if (!_agent.pathPending)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Этот метод мы будем использовать позже, чтобы волки толкались у еды
        public void SetAvoidancePriority(int priority)
        {
            _agent.avoidancePriority = priority;
        }
    }
}