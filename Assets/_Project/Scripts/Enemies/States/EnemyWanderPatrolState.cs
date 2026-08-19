using UnityEngine;
using UnityEngine.AI;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyWanderPatrolState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private MeleeEnemyConfig _meleeConfig;
        
        private float _idleTimer;
        private bool _isIdling;

        public Color StateGizmoColor => Color.green;

        public EnemyWanderPatrolState(EnemyBrain brain)
        {
            _brain = brain;
            _meleeConfig = brain.Config as MeleeEnemyConfig;
        }

        public void Enter()
        {
            _brain.Agent.speed = _brain.Config.PatrolSpeed;
            _isIdling = false;
            FindNewWanderPoint();
        }

        public void Tick()
        {
            if (_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemyAlertState(_brain));
                return;
            }

            if (_isIdling)
            {
                _idleTimer -= Time.deltaTime;
                if (_idleTimer <= 0)
                {
                    _isIdling = false;
                    FindNewWanderPoint();
                }
            }
            else
            {
                // ЖЕСТКАЯ ПРОВЕРКА: Дошли ли мы?
                bool hasReached = !_brain.Agent.pathPending && 
                                  _brain.Agent.remainingDistance <= _brain.Agent.stoppingDistance &&
                                  (!_brain.Agent.hasPath || _brain.Agent.velocity.sqrMagnitude < 0.1f);

                if (hasReached)
                {
                    _isIdling = true;
                    _idleTimer = _meleeConfig != null ? _meleeConfig.IdlePauseDuration : 3f;

                    if (_meleeConfig != null && _meleeConfig.IdleAnimStates.Length > 0)
                    {
                        string randomIdle = _meleeConfig.IdleAnimStates[Random.Range(0, _meleeConfig.IdleAnimStates.Length)];
                        _brain.Animator?.PlayCustomIdle(randomIdle);
                        DevLogger.Log($"<color=green>[Wander]</color> Пришел. Играю: {randomIdle}");
                    }
                    else
                    {
                        _brain.Animator?.PlayIdle(); 
                    }
                }
                else
                {
                    _brain.Animator?.PlayWalk();
                }
            }
        }

        private void FindNewWanderPoint()
        {
            if (_meleeConfig == null) return;

            Vector3 randomDirection = Random.insideUnitSphere * _meleeConfig.WanderRadius;
            randomDirection += _brain.transform.position;
            
            // Ищем ближайшую валидную точку на NavMesh
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _meleeConfig.WanderRadius, NavMesh.AllAreas))
            {
                _brain.Agent.SetDestination(hit.position);
            }
        }

        public void Exit() { }
        
        public void OnDamageTaken()
        {
            if (_brain.Target != null)
            {
                DevLogger.Log("<color=red>[Wander]</color> Получил пулю! В ЯРОСТЬ!");
                _brain.LastKnownTargetPosition = _brain.Target.transform.position;
                
                // Мгновенный переход в бой, никаких поисков и пауз!
                _brain.StateMachine.ChangeState(new EnemyMeleeCombatState(_brain));
            }
        }
    }
}