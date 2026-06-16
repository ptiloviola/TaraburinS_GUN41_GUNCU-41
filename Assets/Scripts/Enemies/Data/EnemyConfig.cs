using UnityEngine;

namespace Gameplay.Enemies.Data
{
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "TD/Enemies/Enemy Config", order = 51)]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Идентификация")]
        public string EnemyId;        // Например: "goblin"
        public string DisplayName;    // Например: "Гоблин-мародер"

        [Header("Визуал и UI")]
        public Sprite UIIcon;         // Иконка для UI (прогноз волн)
        public EnemyFacade Prefab;    // Префаб для пула Zenject

        [Header("Характеристики")]
        public float MaxHealth = 100f;
        public float MoveSpeed = 3.5f;
        public int DamageToBase = 1;  // Урон по базе
        public int RewardMoney = 15;  // Награда за убийство
    }
}


