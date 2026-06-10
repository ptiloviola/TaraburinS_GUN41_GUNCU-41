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

            // 1. СНАЧАЛА ИЩЕМ НА СЦЕНЕ (Среди специальных объектов)
            var scenePoint = _sceneSpawnPoints.FirstOrDefault(p => p.PointId == pointId);
            if (scenePoint != null)
            {
                position = scenePoint.transform.position;
                return true;
            }
            // 2. ЕСЛИ НЕ НАШЛИ - ИЩЕМ НА СЕТКЕ!
            // (Предполагается, что в GridService мы добавим метод GetNodeBySpawnId, 
            // или просто будем брать первую попавшуюся клетку NodeType.Spawn, если ID совпадает)
            
            /* Раскомментируем, когда обновим GridService:
            GridNode spawnNode = _gridService.GetSpawnNode(pointId);
            if (spawnNode != null)
            {
                position = spawnNode.WorldPosition;
                return true;
            }
            */
            Debug.LogError($"[SpawnLocationService] Точка спавна '{pointId}' не найдена ни на сцене, ни на сетке!");
            return false;
        }

    }
}



