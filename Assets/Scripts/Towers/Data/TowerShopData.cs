using UnityEngine;
using Gameplay.Towers;

namespace Gameplay.Towers.Data
{
    // Атрибут обязателен, чтобы Unity смогла отрисовать этот класс в Инспекторе
    [System.Serializable]
    public class TowerShopData
    {

        [Tooltip("Цена постройки")]
        public int Cost;

        // НОВОЕ: Настройка процента возврата при продаже
        [Range(0f, 1f)]
        [Tooltip("Доля возврата средств при продаже (0.5 = 50%, 0.9 = 90%)")]
        public float SellRefundMultiplier = 0.5f;

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