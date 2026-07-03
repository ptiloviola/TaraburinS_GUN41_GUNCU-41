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
        private bool _isActive; // Флаг залипания стейта

        private const float ListeningDuration = 2.5f; // Волк будет прислушиваться ровно 2.5 секунды

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
            // Если логика внутри Tick решила, что всё чисто, она обнулит Suspicion.
            // Только в этом случае мы отдаем управление мозгу.
            if (_perception.CurrentSuspicion <= 0f) return 0f;

            // Если мы еще НЕ в стейте, проверяем порог входа (1 ступень)
            if (!_isActive && _perception.CurrentSuspicion < _config.Investigate.NoticeThreshold)
            {
                return 0f;
            }

            // ЗАЛИПАНИЕ: Если мы уже активны, мы держим очки высокими, даже если подозрение слегка упало
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
                    // Если Шапочка продолжает шуметь, переходим ко 2 ступени ДО окончания таймера
                    if (_perception.CurrentSuspicion >= _config.Investigate.MoveThreshold)
                    {
                        _currentPhase = InvestigatePhase.Trotting;
                        _lockedTargetPosition = _perception.LastKnownPosition; 
                        
                        _locomotion.SetSpeed(_config.Investigate.TrotSpeed);
                        _locomotion.MoveTo(_lockedTargetPosition); // Используем твой MoveTo!
                        _animator.PlayTrot();
                        break;
                    }

                    // Ждем, пока волк вслушивается
                    _actionTimer -= Time.deltaTime;
                    if (_actionTimer <= 0)
                    {
                        // Время вышло. Шум не усилился. Ложная тревога.
                        // Это сбросит Score в 0 при следующем кадре!
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