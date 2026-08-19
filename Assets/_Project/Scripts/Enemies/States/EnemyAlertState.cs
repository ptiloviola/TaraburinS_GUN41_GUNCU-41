using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyAlertState : IEnemyState
    {
        private const float DefaultAlertDuration = 1.0f;
        private const float TurnSpeed = 5f;
        
        private readonly EnemyBrain _brain;
        private float _timer;

        public Color StateGizmoColor => Color.magenta;

        public EnemyAlertState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            DevLogger.Log("<color=magenta>[EnemyState]</color> ЗАМЕТИЛ УГРОЗУ (Alert)!");
            
            _brain.Agent.isStopped = true;
            _brain.Agent.velocity = Vector3.zero;

            if (_brain.Config is MeleeEnemyConfig meleeConfig)
            {
                _timer = meleeConfig.AlertDuration;
                _brain.Animator?.PlayAlert(meleeConfig.AlertAnimState, _timer);
            }
            else
            {
                _timer = DefaultAlertDuration;
                _brain.Animator?.PlayIdle(); 
            }
        }

        public void Tick()
        {
            Vector3 directionToTarget = (_brain.LastKnownTargetPosition - _brain.transform.position).normalized;
            directionToTarget.y = 0; 

            if (directionToTarget != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, targetRotation, Time.deltaTime * TurnSpeed);
            }

            _timer -= Time.deltaTime;
            
            if (_timer <= 0)
            {
                if (_brain.Sensor.IsTargetVisible)
                {
                    _brain.StateMachine.ChangeState(_brain.Config.CreateCombatState(_brain));
                }
                else
                {
                    _brain.StateMachine.ChangeState(new EnemyInvestigateState(_brain));
                }
            }
        }

        public void Exit() 
        {
            _brain.Agent.isStopped = false;
        }

        public void OnDamageTaken()
        {
            if (_brain.Target != null)
            {
                _brain.LastKnownTargetPosition = _brain.Target.transform.position;
            }
        }
    }
}