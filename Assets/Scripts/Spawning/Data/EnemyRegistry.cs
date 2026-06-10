using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameplay.Spawning.Data
{
    [CreateAssetMenu(fileName = "NewEnemyRegistry", menuName = "TD/Enemy Registry")]
    public class EnemyRegistry : ScriptableObject
    {
        public List<EnemySpawnData> Enemies = new List<EnemySpawnData>();

        public GameObject GetPrefabById(string enemyId)
        {
            var data = Enemies.FirstOrDefault(e => e.EnemyId == enemyId);
            if (data != null)
            {
                return data.Prefab;
            }
            
            Debug.LogError($"[EnemyRegistry] Враг с ID '{enemyId}' не найден в каталоге!");
            return null;
        }
    }
}
