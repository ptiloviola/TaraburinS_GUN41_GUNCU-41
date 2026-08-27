using System.Collections.Generic;
using Gameplay.Grid;
using Gameplay.Base;
using Gameplay.Spawning;
using UnityEngine;
using Zenject;

namespace Infrastructure.Levels
{
    public class LevelEntitySpawner : IInitializable
    {
        private readonly IGridService _gridService;
        private readonly GridSceneReferences _references;
        private readonly BaseCore.Factory _baseFactory;
        private readonly EnemySpawnPoint.Factory _spawnFactory;

        private const string BaseNameFormat = "[MARKER] Base_ID: Base_{0}_{1}";
        private const string SpawnNameFormat = "[MARKER] Spawn_ID: Spawn_{0}_{1}";
        private const string BaseIdFormat = "Base_{0}_{1}";
        private const string SpawnIdFormat = "Spawn_{0}_{1}";

        public LevelEntitySpawner(
            IGridService gridService,
            GridSceneReferences references,
            BaseCore.Factory baseFactory,
            EnemySpawnPoint.Factory spawnFactory)
        {
            _gridService = gridService;
            _references = references;
            _baseFactory = baseFactory;
            _spawnFactory = spawnFactory;
        }

        public void Initialize()
        {
            List<GridNode> baseNodes = _gridService.GetNodesByType(NodeType.Base);
            foreach (GridNode node in baseNodes)
            {
                BaseCore baseInstance = _baseFactory.Create();
                Vector3 surfacePos = _gridService.GetWorldPosition(node);
                surfacePos.y += baseInstance.VerticalOffset;
                
                baseInstance.transform.position = surfacePos;
                baseInstance.transform.SetParent(_references.GridParent);
                baseInstance.BaseId = string.Format(BaseIdFormat, node.Position.x, node.Position.y);
                baseInstance.gameObject.name = string.Format(BaseNameFormat, node.Position.x, node.Position.y);
            }

            List<GridNode> spawnNodes = _gridService.GetNodesByType(NodeType.Spawn);
            foreach (GridNode node in spawnNodes)
            {
                EnemySpawnPoint spawnInstance = _spawnFactory.Create();
                spawnInstance.transform.position = _gridService.GetWorldPosition(node);
                spawnInstance.transform.SetParent(_references.GridParent);
                spawnInstance.PointId = string.Format(SpawnIdFormat, node.Position.x, node.Position.y);
                spawnInstance.gameObject.name = string.Format(SpawnNameFormat, node.Position.x, node.Position.y);
            }
        }
    }
}