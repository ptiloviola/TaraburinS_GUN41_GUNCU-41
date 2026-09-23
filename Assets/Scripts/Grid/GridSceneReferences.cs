using Unity.AI.Navigation;
using UnityEngine;

namespace Gameplay.Grid
{
    public class GridSceneReferences : MonoBehaviour
    {
        [Header("Визуальное оформление")]
        [SerializeField] private GridTheme _theme;
        [SerializeField] private GameObject _cubePrefab;

        [Header("Настройки визуала")]
        [SerializeField] private float _spacing = 1.1f;
        [SerializeField] private float _elevationStep = 0.5f;

        [Header("Навигация")]
        [SerializeField] private NavMeshSurface _navMeshSurface;

        public GridTheme Theme => _theme;
        public GameObject CubePrefab => _cubePrefab;
        public float Spacing => _spacing;
        public float ElevationStep => _elevationStep;
        public NavMeshSurface NavMeshSurface => _navMeshSurface;
        
        public Transform GridParent => transform; 
    }
}