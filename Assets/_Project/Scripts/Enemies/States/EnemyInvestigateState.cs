using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemyInvestigateState : IEnemyState
    {

        private readonly EnemyBrain _brain;

        public Color StateGizmoColor => new Color(1f, 0.5f, 0f);

        public EnemyInvestigateState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            DevLogger.Log("<color=orange>[EnemyState]</color> Иду на разведку (Investigate)!");
            
            _brain.Agent.speed = _brain.Config.ChaseSpeed; 
            _brain.Agent.isStopped = false;
            
            _brain.Agent.SetDestination(_brain.LastKnownTargetPosition);
            _brain.Animator?.PlayRun();
        }

        public void Tick()
        {
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(_brain.Config.CreateCombatState(_brain));
                return;
            }

            bool hasReached = !_brain.Agent.pathPending && 
                              _brain.Agent.remainingDistance <= 1f &&
                              (!_brain.Agent.hasPath || _brain.Agent.velocity.sqrMagnitude < 0.1f);

            if (hasReached)
            {
                _brain.StateMachine.ChangeState(new EnemySearchState(_brain));
            }
        }

        public void Exit()
        {
            _brain.Agent.isStopped = true;
        }

        public void OnDamageTaken()
        {
            if (_brain.Target != null)
            {
                _brain.LastKnownTargetPosition = _brain.Target.transform.position;
                _brain.StateMachine.ChangeState(new EnemyAlertState(_brain));
            }
        }
    }
}