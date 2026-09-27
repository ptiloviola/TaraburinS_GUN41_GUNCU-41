using System.Collections.Generic;
using UnityEngine;
using Gameplay.Levels.Data;
using Gameplay.Campaign.Rules;
using System.Linq;

namespace Gameplay.Campaign.Data
{
    [CreateAssetMenu(fileName = "NewDeckConfig", menuName = "TD/Campaign/Deck Config")]
    public class DeckCampaignConfig : ScriptableObject
    {
        [Header("Топология графа")]
        public int MaxDepth = 5;
        public int MinNodesPerLayer = 2;
        public int MaxNodesPerLayer = 3;

        [Header("Обязательные узлы")]
        public Sprite StartIcon;
        public LevelBlueprintConfig BossLevel;

        [Header("Состав Колоды")]
        public int ExactCombats = 4;
        public int ExactShops = 2;
        public int ExactEvents = 2;

        [Header("Правила Генерации (Rules)")]
        [SerializeField] private List<NodePlacementRule> _rules = new List<NodePlacementRule>();
        public IEnumerable<IGraphRule> Rules => _rules.Where(r => r != null).Cast<IGraphRule>();

        [Header("Пулы контента")]
        public List<LevelBlueprintConfig> CombatPool;
        public List<ShopConfig> ShopPool;
        public List<EventConfig> EventPool;
    }
}