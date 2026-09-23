using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Spawning
{

    public class SpawnRegistry
    {
        private readonly Dictionary<string, EnemySpawnPoint> _spawns = new Dictionary<string, EnemySpawnPoint>();
        
        public void Register(EnemySpawnPoint spawn)
        {

            if (spawn == null || string.IsNullOrEmpty(spawn.PointId)) return;

            if (_spawns.TryAdd(spawn.PointId, spawn))
            {
#if UNITY_EDITOR
                Debug.Log($"<color=purple>[SpawnRegistry] Зарегистрирован спавн: {spawn.PointId}</color>");
#endif
            }
        }

        public void Unregister(EnemySpawnPoint spawn)
        {
            if (spawn != null && !string.IsNullOrEmpty(spawn.PointId))
            {
                _spawns.Remove(spawn.PointId);
            }
        }

        public bool TryGetSpawnPosition(string id, out Vector3 position)
        {
            if (_spawns.TryGetValue(id, out var spawn))
            {
                position = spawn.transform.position;
                return true;
            }
            
            position = Vector3.zero;
            return false;
        }

        public void TriggerWarning(string spawnId, float duration)
        {
            if (_spawns.TryGetValue(spawnId, out EnemySpawnPoint point))
            {
                point.TriggerWarning(duration);
            }
        }
    }
}