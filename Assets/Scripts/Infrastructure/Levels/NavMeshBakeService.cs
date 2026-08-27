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
                _references.NavMeshSurface.BuildNavMesh();
#if UNITY_EDITOR
                Debug.Log("<color=magenta>[NavMeshBakeService] NavMesh успешно запечен в Runtime!</color>");
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