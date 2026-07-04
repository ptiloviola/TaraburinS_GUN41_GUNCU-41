using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;


namespace MeatMushrooms.Wolf.States
{
    public class HowlState : IWolfState
    {
        private readonly WolfStats _stats;
        private readonly WolfLocomotion _locomotion;
        private readonly WolfAnimator _animator;
        private readonly WolfSocial _social;
        private readonly WolfConfig _config;
        

        private float _timer;
        private float _nextHowlTime; // Заменили таймер на абсолютное время
        private float _lastCheckTime;
        
        private bool _wantsToHowlSpontaneously;
        private string _wolfName; 
        private readonly WolfEventBus _eventBus;

        public HowlState(WolfStats stats, WolfLocomotion locomotion, 
            WolfAnimator animator, WolfSocial social, WolfConfig config, WolfEventBus eventBus)
        {
            _stats = stats;
            _locomotion = locomotion;
            _animator = animator;
            _social = social;
            _config = config;
            _wolfName = _locomotion.gameObject.name;
            _eventBus = eventBus;
        }

        public float CalculateScore()
        {
            // ЗАЩИТА: Если мы уже воем, держим абсолютный приоритет, чтобы мозг нас не прервал!
            if (_timer > 0) return 1000f;

            // Проверяем кулдаун через абсолютное время
            if (Time.time < _nextHowlTime) return 0f;

            // Если слишком голодны - не до песен
            if (_stats.Hunger > _config.Howl.MaxHungerToHowl) return 0f;

            // Радар (раз в секунду)
            if (Time.time - _lastCheckTime > 1f)
            {
                _lastCheckTime = Time.time;

                if (CheckForHowlingNeighbors())
                {
                    return _config.Howl.JoinHowlScore; 
                }

                if (Random.value <= _config.Howl.SpontaneousChance)
                {
                    _wantsToHowlSpontaneously = true;
                    Debug.Log($"<color=cyan>[HowlState]</color> На кубиках {_wolfName} выпал шанс завыть!");
                }
            }

            return _wantsToHowlSpontaneously ? _config.Howl.SpontaneousScore : 0f;
        }

        public void Enter()
        {
            Debug.Log($"<color=cyan>[HowlState]</color> 🐺 <b>{_wolfName}</b> НАЧАЛ ВЫТЬ!");
            _locomotion.Stop();
            _animator.PlayHowl();
            _eventBus.FireHowl();
            
            _social.IsHowling = true; 
            _wantsToHowlSpontaneously = false; 
            
            _timer = _config.Howl.HowlDuration; // Запускаем таймер воя
        }

        public void Tick()
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                // Когда таймер станет <= 0, в следующий вызов CalculateScore вернет 0, и мозг нас переключит
            }
        }

        public void Exit()
        {
            Debug.Log($"<color=orange>[HowlState]</color> 💤 <b>{_wolfName}</b> закончил выть.");
            
            _social.IsHowling = false; 
            _nextHowlTime = Time.time + _config.Howl.Cooldown; // Устанавливаем время следующего возможного воя
            
            _animator.StopHowl(); 
        }

        private bool CheckForHowlingNeighbors()
        {
            Collider[] colliders = Physics.OverlapSphere(_locomotion.transform.position, _config.Howl.HearRadius);
            
            foreach (var col in colliders)
            {
                WolfSocial otherSocial = col.GetComponentInParent<WolfSocial>();
                if (otherSocial != null && otherSocial != _social)
                {
                    if (otherSocial.IsHowling) 
                    {
                        Debug.Log($"<color=green>[HowlState]</color> 🔊 <b>{_wolfName}</b> услышал сородича и РЕШИЛ ПОДПЕТЬ!");
                        return true;
                    }
                }
            }
            return false;
        }
    }
}