using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemyPatrolState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private int _currentWaypointIndex;

        public EnemyPatrolState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            Debug.Log("<color=green>[EnemyState]</color> Переход в PatrolState");
            _brain.Agent.speed = _brain.Config.PatrolSpeed;
            _brain.Agent.isStopped = false;
            MoveToNextWaypoint();
        }

        public void Tick()
        {
            // Условие выхода: заметили игрока!
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemyCombatState(_brain));
                return;
            }

            // Если дошли до точки — идем к следующей
            if (!_brain.Agent.pathPending && _brain.Agent.remainingDistance <= _brain.Agent.stoppingDistance + 0.1f)
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
    }
}