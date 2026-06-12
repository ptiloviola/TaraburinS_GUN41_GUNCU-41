using UnityEngine;
using System.Collections.Generic;


namespace Gameplay.Spawning
{
    public class SpawnRegistry
    {
        // Храним спавны в словаре (Dictionary) для мгновенного поиска по ID
        private readonly Dictionary<string, EnemySpawnPoint> _spawns = new Dictionary<string, EnemySpawnPoint>();
        
        public void Register(EnemySpawnPoint spawn)
        {
            if (!_spawns.ContainsKey(spawn.PointId))
            {
                _spawns.Add(spawn.PointId, spawn);
                Debug.Log($"<color=purple>[SpawnRegistry] Зарегистрирован спавн: {spawn.PointId}</color>");
            }
        }

        public void Unregister(EnemySpawnPoint spawn)
        {
            if (_spawns.ContainsKey(spawn.PointId))
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




    }
}

