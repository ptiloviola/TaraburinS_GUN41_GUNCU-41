using Gameplay.Economy;
using Gameplay.Grid;
using Gameplay.Towers.Data;
using UnityEngine;
using Zenject;

namespace Gameplay.Towers.Factories
{
    public class TowerFactory
    {
        private readonly IInstantiator _instantiator;
        private readonly BankService _bankService;
        private readonly IGridService _gridService;

        public TowerFactory(IInstantiator instantiator, BankService bankService, IGridService gridService)
        {
            _instantiator = instantiator;
            _bankService = bankService;
            _gridService = gridService;
        }

        public bool TryBuildTower(TowerConfig config, Vector2Int gridPos, Vector3 spawnPosition)
        {
            if (!_gridService.CanBuildAt(gridPos) || _bankService.CurrentBalance < config.BaseCost)
            {
                return false;
            }

            if (!_bankService.SpendMoney(config.BaseCost)) return false;

            GridNode node = _gridService.GetNode(gridPos);
            node.IsOccupied = true;

            ForceSpawnTower(config, 0, gridPos, spawnPosition);
            
            Gameplay.Tools.GameLogger.Log($"<color=green>[TowerFactory] Успешно создана {config.DisplayName} на {gridPos}</color>");
            return true;
        }

        public TowerFacade ForceSpawnTower(TowerConfig config, int level, Vector2Int gridPos, Vector3 spawnPosition)
        {
            GameObject prefab = config.Levels[level].TowerPrefab;
            
            GameObject towerGo = _instantiator.InstantiatePrefab(prefab, spawnPosition, Quaternion.identity, null);
            
            if (towerGo.TryGetComponent(out TowerFacade towerFacade))
            {
                towerFacade.Initialize(config, level, gridPos);
            }
            else
            {
                Gameplay.Tools.GameLogger.LogError($"[TowerFactory] На префабе {prefab.name} отсутствует TowerFacade!");
            }

            return towerFacade;
        }
    }
}