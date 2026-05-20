using UnityEngine;

namespace Gameplay.Grid
{
    public class GridService : IGridService
    {
        private GridNode[,] _nodes;
        
        public int Width { get; private set; }
        public int Height { get; private set; }

        public void InitializeGrid(int width, int height, int[,] elevationMap, NodeType[,] typeMap)
        {
            Width = width;
            Height = height;
            _nodes = new GridNode[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector2Int pos = new Vector2Int(x, z);
                    int elevation = elevationMap[x, z];
                    NodeType type = typeMap[x, z];

                    _nodes[x, z] = new GridNode(pos, type, elevation);
                }
            }
            
            Debug.Log($"<color=cyan>[GridService] Математическая сетка {width}x{height} успешно создана!</color>");
        }

        public GridNode GetNode(Vector2Int position)
        {
            if (IsPositionValid(position))
            {
                return _nodes[position.x, position.y];
            }
            
            Debug.LogError($"[GridService] Попытка получить ячейку за границами сетки: {position}");
            return null;
        }

        public bool CanBuildAt(Vector2Int position)
        {
            if (!IsPositionValid(position)) return false;

            GridNode node = _nodes[position.x, position.y];
            
            // Строить можно только на типе Ground и если ячейка еще не занята другой башней
            return node.Type == NodeType.Ground && !node.IsOccupied;
        }

        // Внутренний метод проверки, что координаты не выходят за рамки массива
        private bool IsPositionValid(Vector2Int position)
        {
            return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
        }
    }
}