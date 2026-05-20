using UnityEngine;

namespace Gameplay.Grid
{
    public interface IGridService
    {
        // Инициализация сетки заданными размерами и картой высот/типов
        void InitializeGrid(int width, int height, int[,] elevationMap, NodeType[,] typeMap);
        
        // Получить данные конкретной ячейки по координатам
        GridNode GetNode(Vector2Int position);
        
        // Быстрая проверка: можно ли в этой точке построить башню?
        bool CanBuildAt(Vector2Int position);
        
        // Размеры сетки (пригодятся для валидации границ)
        int Width { get; }
        int Height { get; }
    }
}