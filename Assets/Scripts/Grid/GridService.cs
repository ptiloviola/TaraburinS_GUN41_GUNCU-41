using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Grid
{
    public class GridService : IGridService
    {
        private GridNode[,] _nodes;

        // Кешируем физические размеры для перевода координат
        private float _spacing;
        private float _elevationStep;
        
        public int Width { get; private set; }
        public int Height { get; private set; }

        public void InitializeGrid(int width, int height, int[,] elevationMap, 
            NodeType[,] typeMap, float spacing, float elevationStep)
        {
            Width = width;
            Height = height;
            _spacing = spacing;
            _elevationStep = elevationStep;
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

        // --- НОВЫЙ ФУНКЦИОНАЛ ДЛЯ СПАВНЕРА И БАЗЫ ---
        // 1. Поиск всех узлов определенного типа (например, всех Спавнов)
        public List<GridNode> GetNodesByType(NodeType type)
        {
            List<GridNode> result = new List<GridNode>();
            for (int x = 0; x < Width; x++)
            {
                for (int z = 0; z < Height; z++)
                {
                    if(_nodes[x, z].Type == type)
                    {
                        result.Add(_nodes[x, z]);
                    }
                }
            }
            return result;
        }
        // 2. Перевод сеточных координат (x, z) в мировые (Vector3) с учетом высоты рельефа!
        public Vector3 GetWorldPosition(GridNode node)
        {
            float addedHeight = node.Elevation * _elevationStep;
            // Возвращаем точку ровно на поверхности ячейки (центр + половина высоты)
            return new Vector3(node.Position.x * _spacing, addedHeight + 0.1f, node.Position.y * _spacing);
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