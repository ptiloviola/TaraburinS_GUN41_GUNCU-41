using UnityEngine;
using Gameplay.Grid;

namespace Gameplay.Spawning.Data
{
    public class GridPointIdAttribute : PropertyAttribute
    {
        public NodeType FilterType { get; } 

        public GridPointIdAttribute(NodeType filterType)
        {
            FilterType = filterType;
        }
    }
}