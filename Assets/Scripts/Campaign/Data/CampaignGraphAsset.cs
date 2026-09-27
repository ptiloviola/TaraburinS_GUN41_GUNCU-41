using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Campaign.Data
{
    [CreateAssetMenu(fileName = "NewCampaignGraph", menuName = "TD/Campaign/Graph Asset")]
    public class CampaignGraphAsset : ScriptableObject
    {
        [Header("Настройки генерации (только для редактора)")]
        public DeckCampaignConfig GeneratorConfig;

        [Space(10)]
        [Tooltip("Список всех узлов на карте")]
        public List<MapNode> Nodes = new List<MapNode>();
        
        [Tooltip("Точки входа на карту")]
        public List<string> StartingNodeIds = new List<string>();

        public void Clear()
        {
            Nodes.Clear();
            StartingNodeIds.Clear();
        }
    }
}