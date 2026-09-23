using UnityEngine;

namespace Gameplay.Grid
{
    public enum NodeType
    {
        Ground,
        Path,
        Obstacle,
        Spawn,
        Base
    }

    public class GridNode
    {
        public Vector2Int Position { get; }
        
        public NodeType Type { get; set; }
        
        public int Elevation { get; }
        
        public bool IsOccupied { get; set; }

        public bool IsClaimed { get; set; }

        public GameObject FoundationVisual { get; set; }

        public GridNode(Vector2Int position, NodeType type, int elevation)
        {
            Position = position;
            Type = type;
            Elevation = elevation;
            IsOccupied = false;
        }
    }
}
