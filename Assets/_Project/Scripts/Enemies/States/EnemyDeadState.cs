using UnityEngine;
using TpsShooter.Enemies.Core;

namespace TpsShooter.Enemies.States
{
    public class EnemyDeadState : IEnemyState
    {
        private readonly EnemyBrain _brain;

        public Color StateGizmoColor => Color.gray;

        public EnemyDeadState(EnemyBrain brain)
        {
            _brain = brain;
        }

        public void Enter()
        {
            DevLogger.Log("<color=black>[EnemyState]</color> Переход в DeadState");
            
            // 1. Останавливаем агента навсегда
            _brain.Agent.isStopped = true;
            _brain.Agent.enabled = false;
            _brain.Animator?.PlayDeath();
            
            // ИСПРАВЛЕНИЕ: Вызываем очистку через единый интерфейс боевки
            _brain.CombatHandler?.OnDeath();

            // 2. Отключаем все хитбоксы, чтобы пули пролетали сквозь труп
            var colliders = _brain.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 3. ВЫПАДЕНИЕ ЛУТА
            if (_brain.Config.DropLoot != null)
            {
                if (Random.value <= _brain.Config.DropChance)
                {
                    DevLogger.Log($"<color=cyan>[Loot]</color> Из врага выпал предмет: {_brain.Config.DropLoot.name}");
                    Vector3 dropPosition = _brain.transform.position + Vector3.up * 0.5f;
                    _brain.LootSpawner.SpawnLoot(_brain.Config.DropLoot, dropPosition, Quaternion.identity);
                }
            }
        }

        public void Tick() { }
        public void Exit() { }
        public void OnDamageTaken() { }
    }
}