using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class InvestigateState : IWolfState
    {
        private readonly WolfLocomotion _locomotion;
        private readonly WolfAnimator _animator;
        private readonly WolfPerception _perception;
        private readonly WolfConfig _config;

        private InvestigatePhase _currentPhase;
        private Vector3 _lockedTargetPosition;
        private float _actionTimer; 
        private bool _isActive;

        private const float ListeningDuration = 2.5f;

        private enum InvestigatePhase
        {
            Listening,
            Trotting,
            LookingAround
        }

        public InvestigateState(WolfLocomotion locomotion, WolfAnimator animator, WolfPerception perception, WolfConfig config)
        {
            _locomotion = locomotion;
            _animator = animator;
            _perception = perception;
            _config = config;
        }

        public float CalculateScore()
        {

            if (_perception.CurrentSuspicion <= 0f) return 0f;

            if (!_isActive && _perception.CurrentSuspicion < _config.Investigate.NoticeThreshold)
            {
                return 0f;
            }

            return _config.Investigate.BaseScore + (_perception.CurrentSuspicion * 0.5f);
        }

        public void Enter()
        {
            _isActive = true;
            _currentPhase = InvestigatePhase.Listening;
            _actionTimer = ListeningDuration; 
            
            _locomotion.Stop();
            _animator.PlayNotice(); 
            
            Debug.Log($"<color=yellow>[Investigate]</color> 🐺 Волк напряг уши! (Таймер пошел)");
        }

        public void Tick()
        {
            switch (_currentPhase)
            {
                case InvestigatePhase.Listening:
                    if (_perception.CurrentSuspicion >= _config.Investigate.MoveThreshold)
                    {
                        _currentPhase = InvestigatePhase.Trotting;
                        _lockedTargetPosition = _perception.LastKnownPosition; 
                        
                        _locomotion.SetSpeed(_config.Investigate.TrotSpeed);
                        _locomotion.MoveTo(_lockedTargetPosition);
                        _animator.PlayTrot();
                        break;
                    }

                    _actionTimer -= Time.deltaTime;
                    if (_actionTimer <= 0)
                    {
                        _perception.ClearSuspicion(); 
                    }
                    break;

                case InvestigatePhase.Trotting:
                    if (_locomotion.HasReachedDestination())
                    {
                        _currentPhase = InvestigatePhase.LookingAround;
                        _actionTimer = _config.Investigate.LookAroundTime;
                        
                        _locomotion.Stop();
                        _animator.PlayNotice(); 
                    }
                    break;

                case InvestigatePhase.LookingAround:
                    _actionTimer -= Time.deltaTime;
                    if (_actionTimer <= 0)
                    {
                        _perception.ClearSuspicion(); 
                    }
                    break;
            }
        }

        public void Exit()
        {
            _isActive = false;
            _locomotion.SetSpeed(3.5f); 
            _animator.StopInvestigate(); 
        }
    }
}