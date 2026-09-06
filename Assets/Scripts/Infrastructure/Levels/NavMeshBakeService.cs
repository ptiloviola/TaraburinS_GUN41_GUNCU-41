using Gameplay.Grid;
using UnityEngine;
using Zenject;

namespace Infrastructure.Levels
{
    public class NavMeshBakeService : IInitializable
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
                var surfaceType = _references.NavMeshSurface.GetType();
                var allSurfaces = _references.NavMeshSurface.gameObject.GetComponents(surfaceType);
                
                int bakedCount = 0;
                foreach (var component in allSurfaces)
                {
                    // Вызываем метод BuildNavMesh напрямую и надежно
                    var buildMethod = surfaceType.GetMethod("BuildNavMesh");
                    if (buildMethod != null)
                    {
                        buildMethod.Invoke(component, null);
                        bakedCount++;
                    }
                }
                
#if UNITY_EDITOR
                Debug.Log($"<color=magenta>[NavMeshBakeService] Успешно запечено поверхностей NavMesh: {bakedCount} в Runtime!</color>");
#endif
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError("[NavMeshBakeService] Ссылка на NavMeshSurface отсутствует!");
#endif
            }
        }
    }
}