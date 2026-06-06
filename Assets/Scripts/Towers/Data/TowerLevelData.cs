using Gameplay.Towers.Data.Modules;
using UnityEngine;

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

    }
}