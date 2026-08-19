using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemySearchState : IEnemyState
    {
        private const float SearchRotationSpeed = 60f; 

        private readonly EnemyBrain _brain;
        private float _searchTimer;

        public Color StateGizmoColor => Color.yellow;

        public EnemySearchState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            DevLogger.Log("<color=yellow>[EnemyState]</color> Осматриваю точку (SearchState)");
            

            _brain.Agent.isStopped = true;
            _brain.Animator?.PlayIdle(); 
            _searchTimer = 0f;
        }

        public void Tick()
        {
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(_brain.Config.CreateCombatState(_brain));
                return;
            }

            _searchTimer += Time.deltaTime;
            _brain.transform.Rotate(Vector3.up, SearchRotationSpeed * Time.deltaTime);

            if (_searchTimer >= _brain.Config.SearchDuration)
            {
                DevLogger.Log("<color=yellow>[EnemySearch]</color> Никого нет. Возвращаюсь к патрулю.");
                _brain.StateMachine.ChangeState(_brain.Config.CreatePatrolState(_brain));
            }
        }

        public void Exit() { }

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