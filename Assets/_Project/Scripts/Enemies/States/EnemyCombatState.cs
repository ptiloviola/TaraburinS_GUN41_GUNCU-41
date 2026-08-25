using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemyCombatState : IEnemyState
    {
        private const float TurnSpeed = 10f;
        private readonly EnemyBrain _brain;
        private float _lastAttackTime;

        public Color StateGizmoColor => Color.red;

        public EnemyCombatState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            DevLogger.Log("<color=red>[EnemyState]</color> Переход в CombatState");
            _brain.Agent.speed = _brain.Config.ChaseSpeed;
            
            // ИСПРАВЛЕНИЕ: Вызываем сервис, а не локальный static счетчик
            _brain.AudioService?.AddCombatant();
        }

        public void Tick()
        {
            // ИСПРАВЛЕНИЕ: Не бьем труп. Если игрок умер — возвращаемся в патруль
            if (_brain.Target == null || _brain.Target.Health.IsDead)
            {
                _brain.StateMachine.ChangeState(new EnemyWaypointPatrolState(_brain));
                return;
            }

            if (!_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemySearchState(_brain));
                return;
            }

            float distance = Vector3.Distance(_brain.transform.position, _brain.Target.transform.position);

            if (distance > _brain.Config.AttackRange)
            {
                if (_brain.Agent.isStopped) _brain.Agent.isStopped = false;
                
                _brain.Animator?.PlayRun(); 
                _brain.Agent.SetDestination(_brain.Target.transform.position);
            }
            else
            {
                _brain.Agent.isStopped = true;
                LookAtTarget();

                if (Time.time - _lastAttackTime >= _brain.Config.AttackCooldown)
                {
                    PerformAttack();
                }
            }
        }

        public void Exit() 
        {
            _brain.AudioService?.RemoveCombatant();
        }

        private void LookAtTarget()
        {
            Vector3 dir = (_brain.Target.transform.position - _brain.transform.position).normalized;
            dir.y = 0; 
            if (dir != Vector3.zero)
            {
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * TurnSpeed);
            }
        }

        private void PerformAttack()
        {
            _lastAttackTime = Time.time;
            _brain.CombatHandler?.PerformAttack(_brain.Target, _brain.Animator);
        }
        
        public void OnDamageTaken() {  }
    }
}