using UnityEngine;
using System;
using System.Collections.Generic;

namespace TpsShooter.Items.Configs
{
    [Serializable]
    public struct LootSpawnRequest
    {
        public ItemConfig ItemToSpawn;
        public int Amount; // Сколько таких штук нужно раскидать по уровню
    }

    [CreateAssetMenu(fileName = "LevelLootConfig", menuName = "TpsShooter/Configs/LevelLootConfig")]
    public class LevelLootConfig : ScriptableObject
    {
        [Header("Spawn Settings")]
        public List<LootSpawnRequest> LootToSpawn;
    }
}