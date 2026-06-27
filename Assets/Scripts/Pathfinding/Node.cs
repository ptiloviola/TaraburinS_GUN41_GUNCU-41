using UnityEngine;

namespace VacuumSim.Pathfinding
{
    public class Node
    {
        public bool IsWalkable;
        
        public Vector3 WorldPosition;
        
        public int GridX;
        public int GridY;

        public bool IsCleaned;
        public bool HasTrash = false;

        // Данные для алгоритма А*
        public int GCost;
        public int HCost;
        public Node Parent;

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