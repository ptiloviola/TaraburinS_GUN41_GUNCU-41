using System.Collections.Generic;
using UnityEngine;
using System.Linq; // Используем LINQ, как в башнях

namespace Gameplay.Enemies.Data
{
    [CreateAssetMenu(fileName = "NewEnemyRegistry", menuName = "TD/Enemies/Enemy Registry", order = 50)]
    public class EnemyRegistry : ScriptableObject
    {
        [Header("Каталог всех врагов")]
        public List<EnemyConfig> Enemies = new List<EnemyConfig>();

        // Метод-помощник: Позволяет быстро найти конфиг врага по его ID
        public EnemyConfig GetEnemyById(string enemyId)
        {
            return Enemies.FirstOrDefault(e => e != null && e.EnemyId == enemyId);
        }

        // Безопасный поиск по ссылке на сам конфиг
        public EnemyConfig GetEnemyByConfig(EnemyConfig configToFind)
        {
            return Enemies.FirstOrDefault(e => e == configToFind);
        }
    }
}