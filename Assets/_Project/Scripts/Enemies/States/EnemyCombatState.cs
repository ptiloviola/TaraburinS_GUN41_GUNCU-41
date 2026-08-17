using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyCombatState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private float _lastAttackTime;
        
        // <--- СТАТИЧЕСКИЙ СЧЕТЧИК (ОБЩИЙ ДЛЯ ВСЕХ ВРАГОВ) --->
        private static int _enemiesInCombat = 0; 

        public EnemyCombatState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            Debug.Log("<color=red>[EnemyState]</color> Переход в CombatState");
            _brain.Agent.speed = _brain.Config.ChaseSpeed;
            
            // Если это первый враг, который нас заметил — включаем экшен
            _enemiesInCombat++;
            if (_enemiesInCombat == 1)
            {
                _brain.AudioService?.SetCombatMusicState(true);
            }
        }

        public void Tick()
        {
            if (!_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemySearchState(_brain));
                return;
            }

            float distance = Vector3.Distance(_brain.transform.position, _brain.Target.transform.position);

            if (distance > _brain.Config.AttackRange)
            {
                if (_brain.Agent.isStopped) 
                {
                    _brain.Agent.isStopped = false;
                    _brain.Animator?.PlayRun(); 
                }
                _brain.Agent.SetDestination(_brain.Target.transform.position);
            }
            else
            {
                _brain.Agent.isStopped = true;
                LookAtTarget();

                if (Time.time - _lastAttackTime >= _brain.Config.AttackCooldown)
                {
                    PerformAttack();
                }
            }
        }

        public void Exit() 
        {
            // Враг умер или потерял нас из виду
            _enemiesInCombat--;
            
            // Если больше ни один враг нас не видит — возвращаем спокойную музыку
            if (_enemiesInCombat <= 0)
            {
                _enemiesInCombat = 0; // Защита от ухода в минус
                _brain.AudioService?.SetCombatMusicState(false);
            }
        }

        private void LookAtTarget()
        {
            Vector3 dir = (_brain.Target.transform.position - _brain.transform.position).normalized;
            dir.y = 0; 
            if (dir != Vector3.zero)
            {
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
            }
        }

        private void PerformAttack()
        {
            _lastAttackTime = Time.time;
            
            if (_brain.Config.Type == EnemyType.Ranged)
            {
                _brain.Animator?.PlayShoot(); 
                _brain.WeaponController.TryFire(_brain.Target);
            }
            else
            {
                _brain.Animator?.PlayMeleeAttack(); 
            }
        }
    }
}