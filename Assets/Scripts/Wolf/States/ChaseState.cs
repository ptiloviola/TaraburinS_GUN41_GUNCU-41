using MeatMushrooms.Player.Components;
using MeatMushrooms.Wolf.Components;
using MeatMushrooms.Wolf.Configs;
using MeatMushrooms.Wolf.Contracts;
using UnityEngine;

namespace MeatMushrooms.Wolf.States
{
    public class ChaseState : IWolfState
    {
        private readonly WolfLocomotion _locomotion;
        private readonly WolfAnimator _animator;
        private readonly WolfPerception _perception;
        private readonly WolfConfig _config;
        private readonly PlayerController _player;

        private ChasePhase _currentPhase;
        private float _actionTimer;
        private bool _isActive;

        private enum ChasePhase
        {
            Howling,
            Chasing,
            Attacking
        }

        public ChaseState(WolfLocomotion locomotion, WolfAnimator animator, WolfPerception perception, WolfConfig config, PlayerController player)
        {
            _locomotion = locomotion;
            _animator = animator;
            _perception = perception;
            _config = config;
            _player = player; // Zenject сам найдет Шапочку и передаст сюда
        }

        public float CalculateScore()
        {
            // Если мы видим цель ПРЯМО СЕЙЧАС - 1000 очков, абсолютный приоритет
            if (_perception.IsTargetInSight) return 1000f;

            // Если цель забежала за камень, но мы УЖЕ в режиме погони - сохраняем 1000 очков,
            // чтобы добежать до камня и проверить.
            if (_isActive) return 1000f;

            return 0f;
        }

        public void Enter()
        {
            _isActive = true;
            _currentPhase = ChasePhase.Howling;
            _actionTimer = _config.Chase.HowlDuration;

            _locomotion.Stop();
            
            // Используем анимацию обычного воя для старта
            _animator.PlayHowl(); 
            
            Debug.Log($"<color=red>[Chase]</color> 🐺 Волк {_locomotion.gameObject.name} ЗАМЕТИЛ ИГРОКА! Поднимает тревогу!");

            // --- СТАЙНЫЙ ЗОВ ---
            // Ищем всех волков в радиусе и передаем им координаты Шапочки
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
            // Если игрок уже мертв, просто стоим (чтобы не кусать труп бесконечно)
            if (_player.IsDead)
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
                        // Вой закончен, бросаемся в атаку!
                        _currentPhase = ChasePhase.Chasing;
                        _locomotion.SetSpeed(_config.Chase.ChaseSpeed);
                        _animator.PlayChase();
                    }
                    break;

                case ChasePhase.Chasing:
                    _locomotion.MoveTo(_perception.LastKnownPosition);

                    // --- ИГНОРИРУЕМ ВЫСОТУ ПРИ РАСЧЕТЕ ДИСТАНЦИИ ---
                    Vector3 wolfPos = _locomotion.transform.position;
                    Vector3 playerPos = _player.transform.position;
                    wolfPos.y = 0f;
                    playerPos.y = 0f;

                    float distToPlayer = Vector3.Distance(wolfPos, playerPos);
                    
                    if (distToPlayer <= _config.Chase.AttackDistance)
                    {
                        // Догнали!
                        _currentPhase = ChasePhase.Attacking;
                        _locomotion.Stop();
                        _animator.PlayAttack();
                        
                        _player.Kill();
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
            _locomotion.SetSpeed(3.5f);
        }
    }
}