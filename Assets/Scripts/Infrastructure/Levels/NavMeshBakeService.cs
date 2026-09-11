using UnityEngine;
using Unity.AI.Navigation;
using Zenject;
using Gameplay.Grid;

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
                // Ищем компоненты напрямую по строгому типу, никаких строк и GetType()
                var allSurfaces = _references.NavMeshSurface.gameObject.GetComponents<NavMeshSurface>();
                
                int bakedCount = 0;
                foreach (var surface in allSurfaces)
                {
                    // Прямой, быстрый и безопасный вызов метода
                    surface.BuildNavMesh();
                    bakedCount++;
                }
                
#if UNITY_EDITOR
                Debug.Log($"<color=magenta>[NavMeshBakeService] Успешно запечено поверхностей NavMesh: {bakedCount} в Runtime!</color>");
#endif
            }
            else
            {
                // Убрано #if UNITY_EDITOR, критическая ошибка должна быть видна в логах билда
                Debug.LogError("[NavMeshBakeService] Ссылка на NavMeshSurface отсутствует!");
            }
        }
    }
}