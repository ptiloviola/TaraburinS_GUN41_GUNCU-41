using MeatMushrooms.Player.Components;
using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;
using MeatMushrooms.Player;

namespace MeatMushrooms.Wolf.States
{
    public class ChaseState : IWolfState
    {
        private readonly WolfLocomotion _locomotion;
        private readonly WolfAnimator _animator;
        private readonly WolfPerception _perception;
        private readonly WolfConfig _config;
        private readonly PlayerRegistry _playerRegistry; // Наш реестр
        private readonly WolfEventBus _eventBus;

        private ChasePhase _currentPhase;
        private float _actionTimer;
        private bool _isActive;

        private enum ChasePhase
        {
            Howling,
            Chasing,
            Attacking
        }

        public ChaseState(WolfLocomotion locomotion, WolfAnimator animator, WolfPerception perception, 
            WolfConfig config, PlayerRegistry playerRegistry, WolfEventBus eventBus)
        {
            _locomotion = locomotion;
            _animator = animator;
            _perception = perception;
            _config = config;
            _playerRegistry = playerRegistry;
            _eventBus = eventBus;
        }

        public float CalculateScore()
        {
            if (_perception.IsTargetInSight) return 1000f;
            if (_isActive) return 1000f;
            return 0f;
        }

        public void Enter()
        {
            _isActive = true;
            _currentPhase = ChasePhase.Howling;
            _actionTimer = _config.Chase.HowlDuration;

            _locomotion.Stop();
            
            _animator.PlayHowl();
            _eventBus.FireHowl();
            
            Debug.Log($"<color=red>[Chase]</color> 🐺 Волк {_locomotion.gameObject.name} ЗАМЕТИЛ ИГРОКА! Поднимает тревогу!");

            Collider[] colliders = Physics.OverlapSphere(_locomotion.transform.position, _config.Chase.AlertRadius);
            foreach (var col in colliders)
            {
                WolfPerception allyPerception = col.GetComponentInParent<WolfPerception>();
                if (allyPerception != null && allyPerception != _perception)
                {
                    allyPerception.ReceiveAlert(_perception.LastKnownPosition);
                }
            }
        }

        public void Tick()
        {
            // ИСПРАВЛЕНИЕ 1: Обращаемся к здоровью через реестр
            if (_playerRegistry.Health.IsDead)
            {
                _locomotion.Stop();
                return; 
            }

            switch (_currentPhase)
            {
                case ChasePhase.Howling:
                    _actionTimer -= Time.deltaTime;
                    if (_actionTimer <= 0)
                    {
                        _currentPhase = ChasePhase.Chasing;
                        _locomotion.SetSpeed(_config.Chase.ChaseSpeed);
                        _animator.PlayChase();
                    }
                    break;

                case ChasePhase.Chasing:
                    _locomotion.MoveTo(_perception.LastKnownPosition);

                    Vector3 wolfPos = _locomotion.transform.position;
                    
                    // ИСПРАВЛЕНИЕ 2: Берем трансформ Шапочки из её контроллера
                    Vector3 playerPos = _playerRegistry.Controller.transform.position;
                    
                    wolfPos.y = 0f;
                    playerPos.y = 0f;

                    float distToPlayer = Vector3.Distance(wolfPos, playerPos);
                    
                    if (distToPlayer <= _config.Chase.AttackDistance)
                    {
                        _currentPhase = ChasePhase.Attacking;
                        _locomotion.Stop();
                        _animator.PlayAttack();
                        
                        // ИСПРАВЛЕНИЕ 3: Вызываем Kill у компонента здоровья
                        _playerRegistry.Health.Kill();
                        break;
                    }

                    if (_locomotion.HasReachedDestination() && !_perception.IsTargetInSight)
                    {
                        Debug.Log($"<color=orange>[Chase]</color> Потерял из виду! Перехожу в поиск.");
                        _isActive = false; 
                    }
                    break;
            }
        }

        public void Exit()
        {
            _isActive = false;
            _locomotion.SetSpeed(3.5f); // Возвращаем скорость к дефолтной для патруля
        }
    }
}