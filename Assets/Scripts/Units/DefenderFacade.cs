using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Gameplay.Units.Data;
using System;

namespace Gameplay.Units
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class DefenderFacade : MonoBehaviour
    {
        public enum DefenderState 
        { 
            Idle, 
            MovingToRallyPoint 
        }

        private Pool _pool;
        private NavMeshAgent _agent;
        private DefenderConfig _config;

        public DefenderState CurrentState { get; private set; }

        public event Action<DefenderFacade> OnDespawned;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _agent.enabled = false; 
        }

        public void SetPool(Pool pool)
        {
            _pool = pool;
        }

        public void WarpTo(Vector3 position)
        {
            _agent.enabled = false; 
            transform.position = position; 
            _agent.enabled = true; 
        }

        public void InitConfig(DefenderConfig config)
        {
            _config = config;
            
            _agent.speed = _config.MoveSpeed;
            _agent.angularSpeed = _config.AngularSpeed;
            
            SetState(DefenderState.Idle);
        }

        public void SendToRallyPoint(Vector3 destination)
        {
            if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            {
                _agent.SetDestination(destination);
                SetState(DefenderState.MovingToRallyPoint);
            }
            else
            {
                Gameplay.Tools.GameLogger.LogWarning($"<color=orange>[DefenderFacade] {gameObject.name} не на NavMesh! Не могу пойти на точку.</color>");
            }
        }

        private void Update()
        {
            switch (CurrentState)
            {
                case DefenderState.Idle:
                    break;
                    
                case DefenderState.MovingToRallyPoint:
                    if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
                    {
                        SetState(DefenderState.Idle);
                    }
                    break;
            }
        }

        private void SetState(DefenderState newState)
        {
            CurrentState = newState;
        }

        public void Despawn()
        {
            if (_pool != null)
            {
                _agent.enabled = false;
                _pool.Despawn(this);
                OnDespawned?.Invoke(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public class Pool : MonoMemoryPool<DefenderFacade> { }
    }
}