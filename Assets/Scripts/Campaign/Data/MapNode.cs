using System;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Levels.Data;

namespace Gameplay.Campaign.Data
{
    [Serializable]
    public class MapNode
    {
        public string Id;
        public int Depth;
        
        public MapNodeType NodeType;
        public string NodeDisplayName;


        public List<string> NextNodeIds = new List<string>();

        public Vector2 RenderPosition; 

        [Header("Данные для боя (если Combat)")]
        public LevelBlueprintConfig CombatLevel;

        [Header("Данные для магазина (если Shop)")]
        public ShopConfig ShopData;

        [Header("Данные для события (если Event)")]
        public EventConfig EventData;
    }
}