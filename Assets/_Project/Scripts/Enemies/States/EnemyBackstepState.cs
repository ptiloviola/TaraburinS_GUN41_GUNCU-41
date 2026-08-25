using UnityEngine;
using UnityEngine.AI;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyBackstepState : IEnemyState
    {
        private const float TurnSpeed = 30f; 
        
        private readonly EnemyBrain _brain;
        private readonly MeleeEnemyConfig _meleeConfig;
        
        private float _timer;

        public Color StateGizmoColor => Color.cyan; 

        public EnemyBackstepState(EnemyBrain brain)
        {
            _brain = brain;
            _meleeConfig = brain.Config as MeleeEnemyConfig;
        }

        public void Enter()
        {
            if (_meleeConfig == null)
            {
                _brain.StateMachine.ChangeState(new EnemyMeleeCombatState(_brain));
                return;
            }

            // ИСПРАВЛЕНИЕ: Выключаем автоматическое вращение агента,
            // но мы больше НЕ используем SetDestination, чтобы не было "кривых" лунных походок
            _brain.Agent.updateRotation = false; 
            _brain.Agent.isStopped = false;

            _timer = _meleeConfig.BackstepDuration;
            _brain.Animator?.PlayCustomLooping(_meleeConfig.BackstepAnimState);
        }

        public void Tick()
        {
            _timer -= Time.deltaTime;
            LookAtTarget();

            // ИСПРАВЛЕНИЕ: Принудительно двигаем агента строго назад с помощью Agent.Move().
            // Так он не будет пытаться обойти препятствия боком, глядя на игрока.
            Vector3 backDirection = -_brain.transform.forward;
            _brain.Agent.Move(backDirection * _meleeConfig.BackstepSpeed * Time.deltaTime);

            if (_timer <= 0f)
            {
                _brain.StateMachine.ChangeState(new EnemyMeleeCombatState(_brain));
            }
        }

        public void Exit()
        {
            _brain.Agent.updateRotation = true;
            _brain.Agent.isStopped = true;
        }

        private void LookAtTarget()
        {
            if (_brain.Target == null) return;
            
            Vector3 dir = (_brain.Target.transform.position - _brain.transform.position).normalized;
            dir.y = 0; 
            if (dir != Vector3.zero)
            {
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * TurnSpeed);
            }
        }

        public void OnDamageTaken() { }
    }
}