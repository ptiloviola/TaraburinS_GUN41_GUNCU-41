using MeatMushrooms.Wolf.Configs;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace MeatMushrooms.Wolf.Components
{
    [RequireComponent(typeof(NavMeshAgent))] 
    public class WolfLocomotion : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private WolfConfig _config;
        
        public bool IsStunned { get; private set; }
        public float CurrentTurn { get; private set; }
        public float CurrentSpeed => _agent.velocity.magnitude;
        private float _targetBaseSpeed;

        [Inject]
        public void Construct(WolfConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Start()
        {

            if (_agent != null && _config != null)
            {
                _agent.stoppingDistance = _config.Locomotion.StoppingDistance;
                _agent.angularSpeed = 50f;
            }
        }

        private void Update()
        {
            if (_agent.hasPath && !IsStunned)
            {
                Vector3 desiredDirection = _agent.desiredVelocity.normalized;
                
                if (desiredDirection != Vector3.zero)
                {
                    float angle = Vector3.SignedAngle(transform.forward, desiredDirection, Vector3.up);
                    
                    float targetTurn = Mathf.Clamp(angle / _config.Locomotion.MaxTurnAngle, -1f, 1f);
                    CurrentTurn = Mathf.Lerp(CurrentTurn, targetTurn, Time.deltaTime * _config.Locomotion.TurnSmoothSpeed);

                    float speedMultiplier = 1f - (Mathf.Abs(CurrentTurn) * _config.Locomotion.TurnPenalty);
                    
                    _agent.speed = _targetBaseSpeed * Mathf.Clamp01(speedMultiplier);
                }
            }
            else
            {
                CurrentTurn = Mathf.Lerp(CurrentTurn, 0f, Time.deltaTime * _config.Locomotion.TurnSmoothSpeed);
                if (_agent != null) _agent.speed = _targetBaseSpeed; 
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            if (IsStunned) return;
            _agent.isStopped = false;
            _agent.SetDestination(targetPosition);
        }

        public void Stop()
        {
            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.isStopped = true;
                _agent.velocity = Vector3.zero; 
            }
        }

        public bool HasReachedDestination()
        {
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

        public void SetAvoidancePriority(int priority)
        {
            if (_agent != null) _agent.avoidancePriority = priority;
        }
        
        public void SetStun(bool isStunned)
        {
            IsStunned = isStunned;
            if (_agent != null && _agent.isOnNavMesh)
            {
                _agent.isStopped = isStunned; 
                if (isStunned) _agent.velocity = Vector3.zero; 
            }
        }

        public void SetSpeed(float newSpeed)
        {
            _targetBaseSpeed = newSpeed;
            if (_agent != null) _agent.speed = newSpeed;
        }
    }
}