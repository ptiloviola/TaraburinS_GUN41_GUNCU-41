using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Towers.Data
{
    [CreateAssetMenu(fileName = "NewTowerConfig", menuName = "TD/Towers/Tower Config")]
    public class TowerConfig : ScriptableObject
    {
        [Header("Идентификация")]
        public string TowerId;
        public string DisplayName; // Имя для UI ("Лазерная Башня")
        // public string Description; // Описание для UI магазина

        [Header("Настройки Магазина и Спавна")]
        public int BaseCost; // Цена постройки первого уровня
        [Range(0f, 1f)] public float SellRefundMultiplier = 0.5f;
        public Sprite Icon;
        public GameObject Prefab; // Префаб, который реально будет заспавнен на сетке

        [Header("Уровни прокачки (Характеристики)")]
        public List<TowerLevelData> Levels = new List<TowerLevelData>();

        public int MaxLevel => Levels.Count - 1;
    }
}