using UnityEngine;

namespace Gameplay.Units.Data 
{
    [CreateAssetMenu(fileName = "NewDefenderConfig", menuName = "TD/Defender Config")]
    public class DefenderConfig : ScriptableObject
    {
        [Header("Базовая информация")]
        public string DefenderId; // НОВОЕ: Уникальный ID (например, "militia", "knight")
        [Header("Визуал")]
        public GameObject Prefab;

        [Header("Перемещение")]
        public float MoveSpeed = 3.5f;
        public float AngularSpeed = 120f;
        
        [Header("Характеристики (Заготовка)")]
        public float MaxHealth = 100f;
    }
}