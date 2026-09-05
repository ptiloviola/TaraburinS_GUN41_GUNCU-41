using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Gameplay.Towers.Data
{
    [CreateAssetMenu(fileName = "NewTowerRegistry", menuName = "TD/Towers/Tower Registry", order = 50)]
    public class TowerRegistry : ScriptableObject
    {
        [Header("Каталог всех доступных башен")]
        public List<TowerConfig> Towers = new List<TowerConfig>();

        public TowerConfig GetTowerById(string towerId)
        {
            return Towers.FirstOrDefault(t => t.TowerId == towerId);
        }
    }
}