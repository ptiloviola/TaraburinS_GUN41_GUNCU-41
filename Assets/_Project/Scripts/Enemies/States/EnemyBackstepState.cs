using UnityEngine;
using UnityEngine.AI;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyBackstepState : IEnemyState
    {
        private const float TurnSpeed = 30f; 
        private const float ReachTolerance = 0.5f;
        
        private readonly EnemyBrain _brain;
        private readonly MeleeEnemyConfig _meleeConfig;
        
        private float _timer;

        public Color StateGizmoColor => Color.cyan; 

        public EnemyBackstepState(EnemyBrain brain)
        {
            _brain = brain;
            _meleeConfig = brain.Config as MeleeEnemyConfig;
        }

        public void Enter()
        {
            DevLogger.Log("<color=cyan>[EnemyState]</color> ТАКТИЧЕСКИЙ ОТХОД (Backstep)");
            
            if (_meleeConfig == null)
            {
                _brain.StateMachine.ChangeState(new EnemyMeleeCombatState(_brain));
                return;
            }

            _brain.Agent.speed = _meleeConfig.BackstepSpeed;
            _brain.Agent.updateRotation = false; 
            _brain.Agent.isStopped = false;

            _timer = _meleeConfig.BackstepDuration;

            Vector3 backDirection = -_brain.transform.forward;
            Vector3 targetPosition = _brain.transform.position + backDirection * _meleeConfig.BackstepDistance;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, _meleeConfig.BackstepDistance, NavMesh.AllAreas))
            {
                _brain.Agent.SetDestination(hit.position);
            }
            else
            {
                _brain.Agent.SetDestination(_brain.transform.position);
            }

            _brain.Animator?.PlayCustomLooping(_meleeConfig.BackstepAnimState);
        }

        public void Tick()
        {
            _timer -= Time.deltaTime;

            LookAtTarget();

            bool hasReached = !_brain.Agent.pathPending && _brain.Agent.remainingDistance <= ReachTolerance;

            if (_timer <= 0f || hasReached)
            {
                _brain.StateMachine.ChangeState(new EnemyMeleeCombatState(_brain));
            }
        }

        public void Exit()
        {
            _brain.Agent.updateRotation = true;
            _brain.Agent.isStopped = true;
        }

        private void LookAtTarget()
        {
            if (_brain.Target == null) return;
            
            Vector3 dir = (_brain.Target.transform.position - _brain.transform.position).normalized;
            dir.y = 0; 
            if (dir != Vector3.zero)
            {
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * TurnSpeed);
            }
        }

        public void OnDamageTaken() 
        { 
        }
    }
}