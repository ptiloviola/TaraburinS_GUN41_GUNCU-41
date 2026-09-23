using System.Collections.Generic;
using UnityEngine;
using Gameplay.Combat.Data;

namespace Gameplay.Combat.Data
{
    [CreateAssetMenu(fileName = "NewCampaignConfig", menuName = "TD/Campaign Config")]
    public class CampaignConfig : ScriptableObject
    {
        [Header("Карта кампании (Узлы)")]
        [SerializeField] private List<MapNode> _nodes = new List<MapNode>();

        public IReadOnlyList<MapNode> Nodes => _nodes;
    }
}