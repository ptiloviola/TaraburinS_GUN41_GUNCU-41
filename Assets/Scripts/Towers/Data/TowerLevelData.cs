using Gameplay.Towers.Data.Modules;
using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Towers.Data
{
    [System.Serializable]
    public class TowerLevelData
    {
        public int UpgradeCost;
        public GameObject VisualPrefab;

        [Header("Модули поведения")]
        public AttackStats Attack;
        public AuraStats Aura;
        // В будущем новые модули (TrapStats, SpawnerStats) будешь добавлять сюда

        // НОВЫЙ МЕТОД: Собираем все модули, которые реализуют интерфейс IModuleDescriptor.
        // Используем yield return - это классная фича C#, которая создает итератор на лету,
        // не выделяя память под новый список List<T>!
        public IEnumerable<IModuleDescriptor> GetActiveModules()
        {
            if (Attack != null) yield return Attack;
            if (Aura != null) yield return Aura;
            
            // В будущем, когда добавишь новые модули, просто допишешь сюда одну строчку:
            // if (Trap != null) yield return Trap;
        }

    }
}