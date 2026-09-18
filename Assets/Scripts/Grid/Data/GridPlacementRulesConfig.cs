using UnityEngine;

namespace Gameplay.Grid.Data
{
    [CreateAssetMenu(fileName = "GridPlacementRules", menuName = "TD/Grid/Placement Rules")]
    public class GridPlacementRulesConfig : ScriptableObject
    {
        [Header("Правила Разметки (Тактическая фаза)")]
        [Tooltip("Минимальное расстояние до дороги (в клетках). 0 - можно ставить вплотную.")]
        public int MinDistanceToPath = 0;
        
        [Tooltip("Минимальное расстояние между фундаментами. 1 - нельзя ставить в соседнюю клетку.")]
        public int MinDistanceBetweenFoundations = 0;
    }
}