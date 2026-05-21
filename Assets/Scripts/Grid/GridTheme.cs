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
    }
}
