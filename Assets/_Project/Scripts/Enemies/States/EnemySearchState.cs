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
            _brain.Agent.speed = _brain.Config.PatrolSpeed; 
            _brain.Agent.isStopped = false;
            _brain.Agent.SetDestination(_brain.LastKnownTargetPosition);
            _brain.Animator?.PlayWalk();
            
            _searchTimer = 0f;
            _reachedLocation = false;
        }

        public void Tick()
        {
            // 1. ИСПРАВЛЕНИЕ: Если увидели игрока - идем в Alert (испуг/рык), 
            // а Alert сам потом переведет в нужный CombatState через конфиг!
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemyAlertState(_brain));
                return;
            }

            if (!_reachedLocation)
            {
                // Защита от бага NavMesh с дистанцией
                bool hasReached = !_brain.Agent.pathPending && 
                                  _brain.Agent.remainingDistance <= 1f &&
                                  (!_brain.Agent.hasPath || _brain.Agent.velocity.sqrMagnitude < 0.1f);

                if (hasReached)
                {
                    _reachedLocation = true;
                    Debug.Log("<color=yellow>[EnemySearch]</color> Осматриваюсь...");
                    _brain.Animator?.PlayIdle(); 
                }
            }
            else
            {
                _searchTimer += Time.deltaTime;
                _brain.transform.Rotate(Vector3.up, 60f * Time.deltaTime);

                if (_searchTimer >= _brain.Config.SearchDuration)
                {
                    // 2. ИСПРАВЛЕНИЕ: Возвращаемся в патруль ТОЛЬКО через фабрику конфига!
                    // Стрелок пойдет по точкам, каратист пойдет шататься.
                    _brain.StateMachine.ChangeState(_brain.Config.CreatePatrolState(_brain));
                }
            }
        }

        public void Exit() { }

        public void OnDamageTaken() 
        { 
            // 3. ИСПРАВЛЕНИЕ: Если в нас стреляют, пока мы ищем — мгновенно в бой через Фабрику!
            _brain.Agent.isStopped = false;
            _brain.StateMachine.ChangeState(_brain.Config.CreateCombatState(_brain));
        }
    }
}