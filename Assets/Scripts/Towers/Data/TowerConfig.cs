using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Towers.Data
{
    [CreateAssetMenu(fileName = "NewTowerConfig", menuName = "TD/Towers/Tower Config")]
    public class TowerConfig : ScriptableObject
    {
        [Header("Базовая информация")]
        public string TowerId; 
        public string DisplayName;

        [Header("Уровни прокачки")]
        public List<TowerLevelData> Levels;

        public int MaxLevel => Levels.Count - 1; 
    }
}