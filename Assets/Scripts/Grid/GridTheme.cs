using UnityEngine;

namespace Gameplay.Grid
{
    [CreateAssetMenu(fileName = "NewGridTheme", menuName = "TD/Grid Theme", order = 52)]
    public class GridTheme : ScriptableObject
    {
        [Header("Материалы для типов ячеек")]
        public Material groundMaterial;
        public Material pathMaterial;
        public Material obstacleMaterial;
        public Material spawnMaterial; 
        public Material baseMaterial;

        public Material GetMaterial(NodeType type)
        {
            return type switch
            {
                NodeType.Path => pathMaterial,
                NodeType.Obstacle => obstacleMaterial,
                NodeType.Spawn => spawnMaterial,
                NodeType.Base => baseMaterial,
                _ => groundMaterial
            };
        }
    }
}
