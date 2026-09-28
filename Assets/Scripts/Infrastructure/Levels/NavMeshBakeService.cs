using UnityEngine;
using Unity.AI.Navigation;
using Gameplay.Grid;

namespace Infrastructure.Levels
{
    public class NavMeshBakeService
    {
        private readonly GridSceneReferences _references;

        public NavMeshBakeService(GridSceneReferences references)
        {
            _references = references;
        }

        public void Initialize()
        {
            if (_references.NavMeshSurface != null)
            {
                var allSurfaces = _references.NavMeshSurface.gameObject.GetComponents<NavMeshSurface>();
                
                int bakedCount = 0;
                foreach (var surface in allSurfaces)
                {
                    surface.BuildNavMesh();
                    bakedCount++;
                }
                
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.Log($"<color=magenta>[NavMeshBakeService] Успешно запечено поверхностей NavMesh: {bakedCount} в Runtime!</color>");
#endif
            }
            else
            {
                Gameplay.Tools.GameLogger.LogError("[NavMeshBakeService] Ссылка на NavMeshSurface отсутствует!");
            }
        }
    }
}