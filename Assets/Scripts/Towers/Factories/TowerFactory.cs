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

        // ИЗМЕНЕНО: Принимаем TowerConfig
        public bool TryBuildTower(TowerConfig config, Vector2Int gridPos, Vector3 spawnPosition)
        {
            if (!_gridService.CanBuildAt(gridPos) || _bankService.CurrentBalance < config.BaseCost)
            {
                return false;
            }

            if (!_bankService.SpendMoney(config.BaseCost)) return false;

            GridNode node = _gridService.GetNode(gridPos);
            node.IsOccupied = true;

            // Спавним башню из конфига
            GameObject towerGo = _instantiator.InstantiatePrefab(config.Prefab, spawnPosition, Quaternion.identity, null);
            
            if (towerGo.TryGetComponent(out TowerFacade towerFacade))
            {
                // Передаем сам конфиг
                towerFacade.Initialize(config, gridPos);
            }

            Debug.Log($"<color=green>[TowerFactory] Успешно создана {config.DisplayName} на {gridPos}</color>");
            return true;
        }
    }
}