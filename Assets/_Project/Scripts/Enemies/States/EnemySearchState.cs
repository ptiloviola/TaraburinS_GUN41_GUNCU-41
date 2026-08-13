using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemySearchState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private float _searchTimer;
        private bool _reachedLocation;

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
            
            _searchTimer = 0f;
            _reachedLocation = false;
        }

        public void Tick()
        {
            // Если во время поиска снова увидели игрока — мгновенно в бой!
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemyCombatState(_brain));
                return;
            }

            // 1. Идем к точке, где последний раз видели игрока
            if (!_reachedLocation)
            {
                if (!_brain.Agent.pathPending && _brain.Agent.remainingDistance <= 1f)
                {
                    _reachedLocation = true;
                    Debug.Log("<color=yellow>[EnemySearch]</color> Прибыл на место. Осматриваюсь...");
                }
            }
            // 2. Дошли. Осматриваемся.
            else
            {
                _searchTimer += Time.deltaTime;
                
                // Имитируем осмотр по сторонам (крутимся)
                _brain.transform.Rotate(Vector3.up, 60f * Time.deltaTime);

                // Если время поиска вышло — сдаемся и идем в патруль
                if (_searchTimer >= _brain.Config.SearchDuration)
                {
                    Debug.Log("<color=yellow>[EnemySearch]</color> Никого нет. Возвращаюсь в патруль.");
                    _brain.StateMachine.ChangeState(new EnemyPatrolState(_brain));
                }
            }
        }

        public void Exit() { }
    }
}