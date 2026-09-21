using System.Collections.Generic;
using UnityEngine;
using Gameplay.Levels.Data;

namespace Gameplay.Core.Data
{
    [CreateAssetMenu(fileName = "NewCampaignConfig", menuName = "TD/Campaign Config")]
    public class CampaignConfig : ScriptableObject
    {
        [Header("Линейная последовательность уровней")]
        [Tooltip("Список уровней, которые игрок должен пройти друг за другом")]
        [SerializeField] private List<LevelBlueprintConfig> _levels = new List<LevelBlueprintConfig>();

        public IReadOnlyList<LevelBlueprintConfig> Levels => _levels;
    }
}