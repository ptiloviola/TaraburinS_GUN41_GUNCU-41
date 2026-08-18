using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyMeleeCombatState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private float _lastAttackTime = -999f;
        
        private bool _isAttacking = false;
        private float _attackAnimationDuration = 0.9f; 
        private static int _enemiesInCombat = 0; 

        public Color StateGizmoColor => Color.red;

        public EnemyMeleeCombatState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            Debug.Log("<color=red>[MeleeCombat]</color> Начал бой!");
            _brain.Agent.speed = _brain.Config.ChaseSpeed;
            _brain.Agent.stoppingDistance = _brain.Config.AttackRange; // Агент сам будет тормозить у цели
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

            // 1. ФАЗА ЗАМАХА (Блокируем всё остальное)
            if (_isAttacking)
            {
                if (timeSinceAttack < _attackAnimationDuration)
                {
                    _brain.Agent.updateRotation = false; // Запрещаем Агенту крутить врага
                    LookAtTarget(); // Крутим скриптом
                    return; 
                }
                else
                {
                    _isAttacking = false;
                    _brain.Agent.updateRotation = true; // Отдаем руль обратно Агенту
                }
            }

            // 2. ФАЗА БОЯ И БЕГА
            if (distance <= _brain.Config.AttackRange && timeSinceAttack >= _brain.Config.AttackCooldown)
            {
                // Жесткая остановка
                _brain.Agent.isStopped = true;
                _brain.Agent.velocity = Vector3.zero; // Убиваем инерцию
                
                _isAttacking = true;
                _lastAttackTime = Time.time;
                
                Debug.Log("<color=red>[MeleeCombat]</color> БЬЮ!");
                _brain.CombatHandler?.PerformAttack(_brain.Target, _brain.Animator);
            }
            else
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
                _brain.transform.rotation = Quaternion.Slerp(_brain.transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 15f);
            }
        }

        public void OnDamageTaken() { /* В бою игнорируем урон, прем как танк */ }
    }
}