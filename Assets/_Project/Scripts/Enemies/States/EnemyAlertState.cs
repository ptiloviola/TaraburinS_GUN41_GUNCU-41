using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyAlertState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private float _timer;

        public Color StateGizmoColor => Color.magenta; // Рисуем фиолетовым!

        public EnemyAlertState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            Debug.Log("<color=magenta>[EnemyState]</color> ЗАМЕТИЛ ИГРОКА (Alert)!");
            
            // Тормозим врага
            _brain.Agent.isStopped = true;

            // Пытаемся достать конфиг и запустить уникальную анимацию
            if (_brain.Config is MeleeEnemyConfig meleeConfig)
            {
                _timer = meleeConfig.AlertDuration;
                _brain.Animator?.PlayAlert(meleeConfig.AlertAnimState, _timer);
            }
            else
            {
                // Резервный вариант, если это обычный стрелок
                _timer = 1.0f;
                _brain.Animator?.PlayIdle(); 
            }
        }

        public void Tick()
        {
            _timer -= Time.deltaTime;
            
            if (_timer <= 0)
            {
                _brain.Agent.isStopped = false;
                // ИСПРАВЛЕНИЕ: Берем правильный стейт из конфига!
                _brain.StateMachine.ChangeState(_brain.Config.CreateCombatState(_brain));
            }
        }

        public void Exit() 
        {
            _brain.Agent.isStopped = false;
        }

        public void OnDamageTaken()
        {
            _brain.Agent.isStopped = false;
            // ИСПРАВЛЕНИЕ: Берем правильный стейт из конфига!
            _brain.StateMachine.ChangeState(_brain.Config.CreateCombatState(_brain));
        }
    }
}