using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Contracts;
using MeatMushrooms.Wolf.Configs;
using UnityEngine;
using UnityEngine.AI;

namespace MeatMushrooms.Wolf.States
{
    public class WanderState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfConfig _config;
        
        private bool _isWaiting; 
        private float _waitTimer;

        public WanderState(WolfStats stats, WolfLocomotion locomotion, WolfConfig config)
        {
            _stats = stats;
            _locomotion = locomotion;
            _config = config;
        }

        public float CalculateScore()
        {
            return _config.Wander.BaseScore + (_stats.Hunger * _config.Wander.HungerMultiplier); 
        }

        public void Enter()
        {
            Debug.Log("[WanderState] Волк начал блуждание.");
            _locomotion.SetSpeed(_config.Locomotion.WalkSpeed);
            _isWaiting = false;
            SetNewRandomDestination();
        }

        public void Tick()
        {
            if (_isWaiting)
            {
                _waitTimer -= Time.deltaTime;
                if (_waitTimer <= 0)
                {
                    _isWaiting = false;
                    SetNewRandomDestination(); 
                }
                return; 
            }

            if (_locomotion.HasReachedDestination())
            {
                _locomotion.Stop();
                _isWaiting = true;
                _waitTimer = Random.Range(_config.Wander.MinWaitTime, _config.Wander.MaxWaitTime);
            }
        }

        public void Exit()
        {
            _locomotion.Stop(); 
        }

        private void SetNewRandomDestination()
        {
            Vector3 randomDirection = Random.insideUnitSphere * _config.Wander.WanderRadius;
            randomDirection += _locomotion.transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _config.Wander.WanderRadius, NavMesh.AllAreas))
            {
                _locomotion.MoveTo(hit.position);
            }
            else
            {
                _isWaiting = true;
                _waitTimer = _config.Wander.ObstacleWaitTime;
            }
        }
    }
}