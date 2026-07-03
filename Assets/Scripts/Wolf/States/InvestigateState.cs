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
        private float _lookAroundTimer;

        // Внутренние стадии нашего поиска
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
            // Теперь стейт просыпается уже на первой ступени (NoticeThreshold)
            if (_perception.CurrentSuspicion < _config.Investigate.NoticeThreshold)
            {
                return 0f;
            }

            return _config.Investigate.BaseScore + (_perception.CurrentSuspicion * 0.5f);
        }

        public void Enter()
        {
            Debug.Log($"<color=yellow>[Investigate]</color> 🐺 Волк {_locomotion.gameObject.name} что-то услышал! (1 ступень)");
            
            // Начинаем всегда со слушания
            _currentPhase = InvestigatePhase.Listening;
            _locomotion.Stop();
            _animator.PlayNotice(); 
        }

        public void Tick()
        {
            switch (_currentPhase)
            {
                case InvestigatePhase.Listening:
                    // Если подозрение пробивает вторую ступень, переходим к движению
                    if (_perception.CurrentSuspicion >= _config.Investigate.MoveThreshold)
                    {
                        Debug.Log($"<color=yellow>[Investigate]</color> Шум подтвердился! Волк идет проверять. (2 ступень)");
                        
                        _currentPhase = InvestigatePhase.Trotting;
                        
                        // ФИКСИРУЕМ ТОЧКУ: Волк пойдет именно туда, где был шум в эту секунду
                        _lockedTargetPosition = _perception.LastKnownPosition; 
                        
                        _locomotion.SetSpeed(_config.Investigate.TrotSpeed);
                        _locomotion.MoveTo(_lockedTargetPosition);
                        _animator.PlayTrot();
                    }
                    break;

                case InvestigatePhase.Trotting:
                    if (_locomotion.HasReachedDestination())
                    {
                        Debug.Log($"<color=yellow>[Investigate]</color> Волк на точке. Осматривается.");
                        
                        _currentPhase = InvestigatePhase.LookingAround;
                        _lookAroundTimer = _config.Investigate.LookAroundTime;
                        _locomotion.Stop();
                        
                        // Снова включаем анимацию "Прислушиваюсь", пока он стоит на точке
                        _animator.PlayNotice(); 
                    }
                    break;

                case InvestigatePhase.LookingAround:
                    _lookAroundTimer -= Time.deltaTime;
                    
                    if (_lookAroundTimer <= 0)
                    {
                        Debug.Log($"<color=yellow>[Investigate]</color> Никого нет. Возвращаюсь к делам.");
                        _perception.ClearSuspicion(); 
                    }
                    break;
            }
        }

        public void Exit()
        {
            // Возвращаем дефолтные настройки
            _locomotion.SetSpeed(3.5f); // Или скорость из конфига Wander
            _animator.StopInvestigate(); // Возвращает Аниматор в норму
        }
    }
}