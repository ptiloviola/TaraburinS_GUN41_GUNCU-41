using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemySearchState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private float _searchTimer;
        private bool _reachedLocation;

        public Color StateGizmoColor => Color.yellow;

        public EnemySearchState(EnemyBrain brain)
        {
            _brain = brain;
        }
        

        public void Enter()
        {
            Debug.Log("<color=yellow>[EnemyState]</color> Переход в SearchState");
            _brain.Agent.speed = _brain.Config.PatrolSpeed; // Осторожно идем к точке
            _brain.Agent.isStopped = false;
            _brain.Agent.SetDestination(_brain.LastKnownTargetPosition);
            _brain.Animator?.PlayWalk();
            
            _searchTimer = 0f;
            _reachedLocation = false;
        }

        public void Tick()
        {
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemyCombatState(_brain));
                return;
            }

            if (!_reachedLocation)
            {
                if (!_brain.Agent.pathPending && _brain.Agent.remainingDistance <= 1f)
                {
                    _reachedLocation = true;
                    Debug.Log("<color=yellow>[EnemySearch]</color> Осматриваюсь...");
                    _brain.Animator?.PlayIdle(); // <--- ДОШЕЛ ДО МЕСТА - ОСТАНОВИЛСЯ
                }
            }
            else
            {
                _searchTimer += Time.deltaTime;
                _brain.transform.Rotate(Vector3.up, 60f * Time.deltaTime);

                if (_searchTimer >= _brain.Config.SearchDuration)
                {
                    _brain.StateMachine.ChangeState(new EnemyPatrolState(_brain));
                }
            }
        }

        public void Exit() { }

        public void OnDamageTaken() { /* Уже ищем, ничего не делаем */ }
    }
}