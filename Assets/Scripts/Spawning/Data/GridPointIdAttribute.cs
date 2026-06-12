using UnityEngine;
using Gameplay.Grid; // Чтобы видеть NodeType

namespace Gameplay.Spawning.Data
{
    // Этот атрибут мы будем вешать над строками в конфиге
    public class GridPointIdAttribute : PropertyAttribute
    {
        public NodeType FilterType; // Искать спавны или базы?

        public GridPointIdAttribute(NodeType filterType)
        {
            FilterType = filterType;
        }
    }
}