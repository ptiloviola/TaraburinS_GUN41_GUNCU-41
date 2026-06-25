using UnityEngine;

namespace VacuumSim.Pathfinding
{
    public class Node
    {
        // Можно ли здесь проехать? (Нет ли шкафа или кубика)
        public bool IsWalkable;
        
        // Физическая координата центра этой ячейки в 3D мире
        public Vector3 WorldPosition;
        
        // Индексы ячеек в двумерном массиве (для алгоритма А*)
        public int GridX;
        public int GridY;

        // Память об уборке
        public bool IsCleaned; // Добавили это!
        public bool HasTrash = false;

        // Данные для алгоритма А*
        public int GCost;
        public int HCost;
        public Node Parent; // Ссылка на ячейку, из которой мы пришли в эту

        // F-Cost вычисляется на лету
        public int FCost => GCost + HCost;

        public Node(bool isWalkable, Vector3 worldPosition, int gridX, int gridY)
        {
            IsWalkable = isWalkable;
            WorldPosition = worldPosition;
            GridX = gridX;
            GridY = gridY;
            IsCleaned = false;
        }
    }
}