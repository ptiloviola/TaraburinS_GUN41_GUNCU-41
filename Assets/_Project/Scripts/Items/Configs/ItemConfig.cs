using UnityEngine;

namespace TpsShooter.Items.Configs
{
    // Базовый класс для всех предметов
    public abstract class ItemConfig : ScriptableObject
    {
        public string ItemName;
        public Sprite Icon; // Пригодится для UI инвентаря
        public GameObject Prefab; // Для фабрики
    }
}