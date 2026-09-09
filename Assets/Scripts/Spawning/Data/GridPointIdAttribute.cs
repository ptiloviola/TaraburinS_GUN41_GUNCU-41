using UnityEngine;
using Gameplay.Grid;

namespace Gameplay.Spawning.Data
{
    public class GridPointIdAttribute : PropertyAttribute
    {
        // Доступно только для чтения после создания атрибута
        public NodeType FilterType { get; } 

        public GridPointIdAttribute(NodeType filterType)
        {
            FilterType = filterType;
        }
    }
}