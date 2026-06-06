using UnityEngine;
using Gameplay.Towers;

namespace Gameplay.Towers.Data
{
    // Атрибут обязателен, чтобы Unity смогла отрисовать этот класс в Инспекторе
    [System.Serializable]
    public class TowerShopData
    {
        [Tooltip("Уникальный строковый ID (например: basic_tower, laser_tower)")]
        public string TowerId;

        [Tooltip("Имя, которое увидит игрок в UI")]
        public string DisplayName;

        [Tooltip("Цена постройки")]
        public int Cost;

        [Tooltip("Иконка для кнопки в UI")]
        public Sprite Icon;

        [Tooltip("Префаб, который реально будет заспавнен на сетке")]
        public GameObject Prefab;

        // В будущем сюда можно добавить:
        // public string Description; // Описание ("Стреляет лазером по площади")

        [Header("Боевые параметры (Для радиуса и UI)")]
        [Tooltip("Ссылка на конфиг характеристик этой башни")]
        public TowerConfig TowerConfig; // Ссылка на боевые характеристики (урон, радиус)
    }
}