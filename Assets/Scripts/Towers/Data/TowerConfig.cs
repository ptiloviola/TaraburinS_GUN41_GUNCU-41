using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Towers.Data
{
    [CreateAssetMenu(fileName = "Config_Tower_", menuName = "TD/Towers/Tower Config")]
    public class TowerConfig : ScriptableObject
    {
        [Header("Идентификация")]
        public string TowerId;
        public string DisplayName;

        [Header("Настройки Магазина")]
        public int BaseCost;
        [Range(0f, 1f)] public float SellRefundMultiplier = 0.5f;
        public Sprite Icon;
        

        [Header("Уровни прокачки (Характеристики)")]
        public List<TowerLevelData> Levels = new List<TowerLevelData>();

        public int MaxLevel => Levels.Count - 1;
    }
}