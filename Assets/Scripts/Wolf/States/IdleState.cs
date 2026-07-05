using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;
using UnityEngine.AI;
using Cysharp.Threading.Tasks;
using System;

namespace MeatMushrooms.Wolf.States
{
    public class IdleState : IWolfState
    {
        private enum SubState { Deciding, Standing, Shaking, MovingToRest, Resting }
        
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfAnimator _animator;
        private readonly WolfConfig _config;

        private SubState _subState;
        private float _timer;

        public IdleState(WolfStats stats, WolfLocomotion locomotion, WolfAnimator animator, WolfConfig config)
        {
            _stats = stats;
            _locomotion = locomotion;
            _animator = animator;
            _config = config;
        }

        public float CalculateScore()
        {
            float score = _config.Idle.BaseScore - (_stats.Hunger * _config.Idle.HungerPenaltyMultiplier); 
            
            if (_subState == SubState.Resting || _subState == SubState.MovingToRest)
            {
                score += _config.Idle.StickyRestScoreBonus;
            }

            return Mathf.Max(0, score); 
        }

        public void Enter()
        {
            Debug.Log("[IdleState] Волк перешел в состояние покоя.");
            _locomotion.SetSpeed(_config.Locomotion.WalkSpeed);
            _subState = SubState.Deciding;
        }

        public void Tick()
        {
            switch (_subState)
            {
                case SubState.Deciding:
                    DecideNextAction();
                    break;

                case SubState.Standing:
                case SubState.Shaking:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0) 
                    {
                        _subState = SubState.Deciding;
                    }
                    break;

                case SubState.MovingToRest:
                    if (_locomotion.HasReachedDestination())
                    {
                        _locomotion.Stop();
                        _animator.PlaySleep();
                        _timer = _config.Idle.RestDuration;
                        _subState = SubState.Resting;
                    }
                    break;

                case SubState.Resting:
                    _timer -= Time.deltaTime;
                    if (_timer <= 0)
                    {
                        _animator.PlayWakeUp();
                        _subState = SubState.Deciding;
                    }
                    break;
            }
        }

        public void Exit()
        {
            Debug.Log("[IdleState] Волк выходит из покоя.");
            
            if (_subState == SubState.Resting)
            {
                WakeUpAsync().Forget();
            }
            
            _locomotion.Stop();
        }

        private async UniTaskVoid WakeUpAsync()
        {

            _locomotion.SetStun(true);
            _animator.PlayWakeUp();

            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

            if (_locomotion != null)
            {
                _locomotion.SetStun(false);
            }
        }

        private void DecideNextAction()
        {
            if (_stats.IsFoodComa(_config.Idle.MealsToSleep, _config.Idle.MealTimeWindow))
            {
                Vector3 retreatPoint = GetRetreatPoint();
                _locomotion.MoveTo(retreatPoint);
                _subState = SubState.MovingToRest;
                return;
            }

            if (UnityEngine.Random.value <= _config.Idle.ShakeProbability)
            {
                _animator.PlayShake();
                _timer = _config.Idle.ShakeDuration;
                _subState = SubState.Shaking;
                return;
            }

            _timer = UnityEngine.Random.Range(_config.Idle.MinIdleTime, _config.Idle.MaxIdleTime);
            _subState = SubState.Standing;
        }

        private Vector3 GetRetreatPoint()
        {
            Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * _config.Idle.RetreatRadius;
            randomDirection += _locomotion.transform.position;

            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _config.Idle.RetreatRadius, NavMesh.AllAreas))
            {
                return hit.position;
            }
            return _locomotion.transform.position;
        }
    }
}