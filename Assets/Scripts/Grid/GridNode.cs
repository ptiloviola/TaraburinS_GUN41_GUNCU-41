using UnityEngine;

namespace Gameplay.Grid
{
    public enum NodeType
    {
        Ground,   // Обычная земля (можно строить башни)
        Path,     // Дорога, по которой идут враги (строить нельзя)
        Obstacle,  // Препятствие (строить нельзя, враги обходят)
        Spawn,  // НОВОЕ: Отсюда выходят враги
        Base    // НОВОЕ: Сюда они идут (Финиш)
    }

    public class GridNode
    {
        // Позиция ячейки в координатах сетки (например, x: 2, z: 4)
        public Vector2Int Position { get; }
        
        // Тип ячейки (Земля, Дорога, Препятствие)
        public NodeType Type { get; set; }
        
        // Уровень высоты (0 — низина, 1 — холм, 2 — гора и т.д.)
        public int Elevation { get; }
        
        // Флаг: занята ли уже ячейка башней
        public bool IsOccupied { get; set; }

        // Конструктор для создания ячейки
        public GridNode(Vector2Int position, NodeType type, int elevation)
        {
            Position = position;
            Type = type;
            Elevation = elevation;
            IsOccupied = false;
        }
    }
}
