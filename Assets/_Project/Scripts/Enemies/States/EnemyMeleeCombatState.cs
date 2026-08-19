using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyMeleeCombatState : IEnemyState
    {
        private const float TurnSpeed = 30f;
        
        private readonly EnemyBrain _brain;
        private readonly MeleeEnemyConfig _meleeConfig;
        
        private float _lastAttackTime = 0f;
        private bool _isAttacking = false;
        
        private static int _enemiesInCombat = 0; 

        public Color StateGizmoColor => Color.red;

        public EnemyMeleeCombatState(EnemyBrain brain)
        {
            _brain = brain;
            _meleeConfig = _brain.Config as MeleeEnemyConfig;
        }

        public void Enter()
        {
            DevLogger.Log("<color=red>[MeleeCombat]</color> Начал бой!");
            _brain.Agent.speed = _brain.Config.ChaseSpeed;
            _brain.Agent.stoppingDistance = _brain.Config.AttackRange; 
            _isAttacking = false;
            
            _enemiesInCombat++;
            if (_enemiesInCombat == 1) _brain.AudioService?.SetCombatMusicState(true);
        }

        public void Tick()
        {
            if (!_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemySearchState(_brain));
                return;
            }

            float distance = Vector3.Distance(_brain.transform.position, _brain.Target.transform.position);
            float timeSinceAttack = Time.time - _lastAttackTime;


            if (_isAttacking)
            {
                float currentAnimDuration = _meleeConfig != null ? _meleeConfig.AttackAnimDuration : 1.0f;

                if (timeSinceAttack < currentAnimDuration)
                {
                    _brain.Agent.updateRotation = false; 
                    LookAtTarget(); 
                    return; 
                }
                else
                {
                    _brain.StateMachine.ChangeState(new EnemyBackstepState(_brain));
                    return; 
                }
            }

            if (distance <= _brain.Config.AttackRange && timeSinceAttack >= _brain.Config.AttackCooldown)
            {
                _brain.Agent.isStopped = true;
                
                _isAttacking = true;
                _lastAttackTime = Time.time;
                
                _brain.CombatHandler?.PerformAttack(_brain.Target, _brain.Animator);
            }
            else if (!_isAttacking)
            {
                if (_brain.Agent.isStopped) _brain.Agent.isStopped = false;
                
                _brain.Animator?.PlayRun(); 
                _brain.Agent.SetDestination(_brain.Target.transform.position);
            }
        }

        public void Exit() 
        {
            _brain.Agent.updateRotation = true;
            if (!_brain.Agent.isStopped) _brain.Agent.isStopped = true;
            
            _enemiesInCombat--;
            if (_enemiesInCombat <= 0)
            {
                _enemiesInCombat = 0;
                _brain.AudioService?.SetCombatMusicState(false);
            }
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

        public void OnDamageTaken() {  }
    }
}