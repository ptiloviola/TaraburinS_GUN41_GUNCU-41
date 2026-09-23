using System;
using UnityEngine;
using Gameplay.Levels.Data;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class MapNode
    {
        public MapNodeType NodeType;
        public string NodeDisplayName;

        [Header("Данные для боя (если тип Combat)")]
        public LevelBlueprintConfig CombatLevel;

        [Header("Данные для магазина (если тип Shop)")]
        public ShopConfig ShopData;
    }
}