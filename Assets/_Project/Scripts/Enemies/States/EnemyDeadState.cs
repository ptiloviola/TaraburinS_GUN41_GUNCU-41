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
            

            _brain.Agent.isStopped = true;
            _brain.Agent.enabled = false;
            _brain.Animator?.PlayDeath();
            

            _brain.CombatHandler?.OnDeath();


            var colliders = _brain.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

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