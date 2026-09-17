using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Grid
{
    public interface IGridService
    {
        void InitializeGrid(int width, int height, int[,] elevationMap, 
        NodeType[,] typeMap, float spacing, float elevationStep);
        
        GridNode GetNode(Vector2Int position);

        List<GridNode> GetNodesByType(NodeType type);
        Vector3 GetWorldPosition(GridNode node);
        
        bool CanBuildAt(Vector2Int position);
        
        int Width { get; }
        int Height { get; }
    }
}