using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Gameplay.Grid;
using System.Linq;

namespace Gameplay.Spawning
{
    public class SpawnLocationService : MonoBehaviour
    {
        // Сюда мы можем перетащить те самые "специальные объекты за картой", 
        // если не хотим спавнить врагов на сетке.
        [SerializeField] private List<EnemySpawnPoint> _sceneSpawnPoints = new List<EnemySpawnPoint>();
        private IGridService _gridService;

        [Inject]
        public void Construct(IGridService gridService)
        {
            _gridService = gridService;
        }

        public bool TryGetSpawnPosition(string pointId, out Vector3 position)
        {
            position = Vector3.zero;

            // 1. ИЩЕМ НА СЦЕНЕ (С защитой от удаленных объектов: p != null)
            var scenePoint = _sceneSpawnPoints.FirstOrDefault(p => p != null && p.PointId == pointId);
            if (scenePoint != null)
            {
                position = scenePoint.transform.position;
                return true;
            }

            // 2. ИЩЕМ НА СЕТКЕ (Наши нарисованные пурпурные клетки)
            // Запрашиваем у сервиса все клетки типа Spawn
            List<GridNode> spawnNodes = _gridService.GetNodesByType(NodeType.Spawn);
            
            if (spawnNodes != null && spawnNodes.Count > 0)
            {
                // Пока берем просто первую попавшуюся клетку спавна. 
                // В будущем, если у нас будет много РАЗНЫХ точек спавна на сетке, 
                // мы научимся различать их по ID.
                GridNode spawnNode = spawnNodes[0]; 
                
                // Используем наш новый метод перевода сеточных координат в 3D-мировые!
                position = _gridService.GetWorldPosition(spawnNode);
                return true;
            }
            
            Debug.LogError($"[SpawnLocationService] Точка спавна '{pointId}' не найдена ни на сцене, ни на сетке!");
            return false;
        }

    }
}



