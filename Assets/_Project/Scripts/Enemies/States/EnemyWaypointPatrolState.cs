using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemyWaypointPatrolState : IEnemyState
    {
        private const float WaypointTolerance = 0.1f;
        
        private readonly EnemyBrain _brain;
        private int _currentWaypointIndex;

        public Color StateGizmoColor => Color.green;

        public EnemyWaypointPatrolState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            DevLogger.Log("<color=green>[EnemyState]</color> Переход в PatrolState");
            _brain.Agent.speed = _brain.Config.PatrolSpeed;
            _brain.Agent.isStopped = false;
            
            _brain.Animator?.PlayWalk(); 
            MoveToNextWaypoint();
        }

        public void Tick()
        {
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.LastKnownTargetPosition = _brain.Target.transform.position;
                _brain.StateMachine.ChangeState(new EnemyAlertState(_brain));
                return;
            }

            if (!_brain.Agent.pathPending && _brain.Agent.remainingDistance <= _brain.Agent.stoppingDistance + WaypointTolerance)
            {
                MoveToNextWaypoint();
            }
        }

        public void Exit() 
        {
            _brain.Agent.isStopped = true;
        }

        private void MoveToNextWaypoint()
        {
            if (_brain.PatrolPoints == null || _brain.PatrolPoints.Length == 0) return;

            _brain.Agent.SetDestination(_brain.PatrolPoints[_currentWaypointIndex].position);
            _currentWaypointIndex = (_currentWaypointIndex + 1) % _brain.PatrolPoints.Length;
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