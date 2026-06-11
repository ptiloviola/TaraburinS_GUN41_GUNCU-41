using UnityEngine;
using System.Collections.Generic;

namespace Gameplay.Grid
{
    public interface IGridService
    {
        // Инициализация сетки заданными размерами и картой высот/типов
        void InitializeGrid(int width, int height, int[,] elevationMap, 
        NodeType[,] typeMap, float spacing, float elevationStep);
        
        // Получить данные конкретной ячейки по координатам
        GridNode GetNode(Vector2Int position);

        List<GridNode> GetNodesByType(NodeType type);
        Vector3 GetWorldPosition(GridNode node);
        
        // Быстрая проверка: можно ли в этой точке построить башню?
        bool CanBuildAt(Vector2Int position);
        
        // Размеры сетки (пригодятся для валидации границ)
        int Width { get; }
        int Height { get; }
    }
}