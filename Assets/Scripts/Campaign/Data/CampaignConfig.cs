using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Campaign.Data
{
    [CreateAssetMenu(fileName = "NewCampaignConfig", menuName = "TD/Campaign Config")]
    public class CampaignConfig : ScriptableObject
    {
        [Header("Карта кампании (Узлы)")]
        [SerializeField] private List<MapNode> _nodes = new List<MapNode>();

        public IReadOnlyList<MapNode> Nodes => _nodes;
    }
}