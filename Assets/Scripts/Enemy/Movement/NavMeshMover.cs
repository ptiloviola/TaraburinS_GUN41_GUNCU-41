using UnityEngine;
using UnityEngine.AI;
using Infrastructure.Interfaces;
namespace Enemy.Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class NavMeshMover : MonoBehaviour, IMover
    {
        private NavMeshAgent _agent;

        public float CurrentSpeed => _agent.velocity.magnitude;

        public void Initialize(float speed, float baseOffset)
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.speed = speed;
            _agent.baseOffset = baseOffset;
        }

        public void SetDestination(Vector3 target)
        {
            if (_agent.isOnNavMesh) _agent.SetDestination(target);
        }

        public void Stop()
        {
            if (_agent.hasPath) _agent.ResetPath();
        }
    }
}