using UnityEngine;
using TpsShooter.Enemies.Core;
using TpsShooter.Enemies.Configs;

namespace TpsShooter.Enemies.States
{
    public class EnemyCombatState : IEnemyState
    {
        private readonly EnemyBrain _brain;
        private float _lastAttackTime;

        public EnemyCombatState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            Debug.Log("<color=red>[EnemyState]</color> Переход в CombatState");
            _brain.Agent.speed = _brain.Config.ChaseSpeed;
        }

        public void Tick()
        {
            // Условие выхода: потеряли игрока из виду (забежал за стену/убежал)
            if (!_brain.Sensor.IsTargetVisible)
            {
                _brain.StateMachine.ChangeState(new EnemySearchState(_brain));
                return;
            }

            float distance = Vector3.Distance(_brain.transform.position, _brain.Target.transform.position);

            if (distance > _brain.Config.AttackRange)
            {
                // Игрок далеко - догоняем
                _brain.Agent.isStopped = false;
                _brain.Agent.SetDestination(_brain.Target.transform.position);
            }
            else
            {
                // Игрок близко - стоим и атакуем
                _brain.Agent.isStopped = true;
                LookAtTarget();

                if (Time.time - _lastAttackTime >= _brain.Config.AttackCooldown)
                {
                    PerformAttack();
                }
            }
        }

        public void Exit() { }

        private void LookAtTarget()
        {
            Vector3 dir = (_brain.Target.transform.position - _brain.transform.position).normalized;
            dir.y = 0; // Не наклоняем капсулу
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
                _brain.WeaponController.TryFire(_brain.Target);
            }
            else
            {
                Debug.Log($"<color=red>[EnemyCombat]</color> Удар ближнего боя на {_brain.Config.MeleeDamage} урона!");
                // Позже передадим урон через IDamageable игроку
            }
        }
    }
}