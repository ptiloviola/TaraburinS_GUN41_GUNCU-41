using System.Linq;
using UnityEngine;
using Gameplay.Grid.Data;
using Gameplay.Levels.Data;

namespace Gameplay.Grid.Services
{
    public class GridValidationService
    {
        private readonly IGridService _gridService;
        private readonly GridPlacementRulesConfig _rules;
        private readonly LevelRuntimeModel _runtimeModel;


        public GridValidationService(
            IGridService gridService, 
            GridPlacementRulesConfig rules,
            LevelRuntimeModel runtimeModel) 
        {
            _gridService = gridService;
            _rules = rules;
            _runtimeModel = runtimeModel;
        }

        public bool CanClaimFoundation(Vector2Int position)
        {
            GridNode node = _gridService.GetNode(position);
            
            if (node == null || node.Type != NodeType.Ground || node.IsClaimed || node.IsOccupied) 
                return false;

            int currentClaimedCount = _gridService.GetNodesByType(NodeType.Ground).Count(n => n.IsClaimed);
            
            if (currentClaimedCount >= _runtimeModel.FoundationQuota)
            {
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.Log($"<color=yellow>[GridValidationService] Лимит фундаментов исчерпан! Максимум: {_runtimeModel.FoundationQuota}</color>");
#endif
                return false;
            }

  
            if (!CheckProximityToType(position, NodeType.Path, _rules.MinDistanceToPath)) return false;
            if (!CheckProximityToClaimed(position, _rules.MinDistanceBetweenFoundations)) return false;

            return true;
        }

        public bool CanBuildTower(Vector2Int position)
        {
            GridNode node = _gridService.GetNode(position);
            return node != null && node.IsClaimed && !node.IsOccupied;
        }

        private bool CheckProximityToType(Vector2Int center, NodeType targetType, int minDistance)
        {
            if (minDistance <= 0) return true;
            for (int x = -minDistance; x <= minDistance; x++)
            {
                for (int y = -minDistance; y <= minDistance; y++)
                {
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) <= minDistance)
                    {
                        var neighbor = _gridService.GetNode(center + new Vector2Int(x, y));
                        if (neighbor != null && neighbor.Type == targetType) return false;
                    }
                }
            }
            return true;
        }

        private bool CheckProximityToClaimed(Vector2Int center, int minDistance)
        {
            if (minDistance <= 0) return true;
            for (int x = -minDistance; x <= minDistance; x++)
            {
                for (int y = -minDistance; y <= minDistance; y++)
                {
                    if (x == 0 && y == 0) continue; 
                    if (Mathf.Max(Mathf.Abs(x), Mathf.Abs(y)) <= minDistance)
                    {
                        var neighbor = _gridService.GetNode(center + new Vector2Int(x, y));
                        if (neighbor != null && neighbor.IsClaimed) return false;
                    }
                }
            }
            return true;
        }
    }
}