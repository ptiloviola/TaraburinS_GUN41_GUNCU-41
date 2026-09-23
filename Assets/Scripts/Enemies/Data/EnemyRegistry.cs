using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Enemies.Data
{
    [CreateAssetMenu(fileName = "NewEnemyRegistry", menuName = "TD/Enemies/Enemy Registry", order = 50)]
    public class EnemyRegistry : ScriptableObject
    {
        [Header("Каталог всех врагов")]
        public List<EnemyConfig> Enemies = new List<EnemyConfig>();

        public EnemyConfig GetEnemyById(string enemyId)
        {
            return Enemies.FirstOrDefault(e => e != null && e.EnemyId == enemyId);
        }

        public EnemyConfig GetEnemyByConfig(EnemyConfig configToFind)
        {
            return Enemies.FirstOrDefault(e => e == configToFind);
        }
    }
}