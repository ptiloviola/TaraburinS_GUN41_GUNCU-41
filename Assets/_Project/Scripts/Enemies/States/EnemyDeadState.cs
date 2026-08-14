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
            _brain.Animator?.PlayDeath();
            _brain.WeaponController?.HideWeapon();

            // 2. Отключаем все хитбоксы, чтобы пули пролетали сквозь труп
            var colliders = _brain.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            // 3. ВЫПАДЕНИЕ ЛУТА (Используем твой точный метод SpawnLoot)
            if (_brain.Config.DropLoot != null)
            {
                if (Random.value <= _brain.Config.DropChance)
                {
                    Debug.Log($"<color=cyan>[Loot]</color> Из врага выпал предмет: {_brain.Config.DropLoot.name}");
                    
                    // Спавним предмет немного приподнятым над землей
                    Vector3 dropPosition = _brain.transform.position + Vector3.up * 0.5f;
                    
                    _brain.LootSpawner.SpawnLoot(_brain.Config.DropLoot, dropPosition, Quaternion.identity);
                }
            }
        }

        public void Tick()
        {
            // Мертвые не кусаются. Ничего не делаем.
        }

        public void Exit() { }
    }
}