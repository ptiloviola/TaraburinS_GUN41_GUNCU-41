using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemyDeadState : IEnemyState
    {
        private readonly EnemyBrain _brain;

        public EnemyDeadState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            Debug.Log("<color=black>[EnemyState]</color> Переход в DeadState");
            
            // 1. Останавливаем агента навсегда
            _brain.Agent.isStopped = true;
            _brain.Agent.enabled = false;

            // 2. Отключаем коллайдер, чтобы пули игрока больше не попадали в труп
            if (_brain.TryGetComponent(out Collider collider))
            {
                collider.enabled = false;
            }

            // Позже мы добавим сюда обращение к LootFactory для выброса пушки/аптечки!
        }

        public void Tick()
        {
            // Мертвые не кусаются. Ничего не делаем.
        }

        public void Exit() { }
    }
}