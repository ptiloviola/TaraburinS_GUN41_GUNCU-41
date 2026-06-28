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

        public bool TryBuildTower(TowerShopData shopData, Vector2Int gridPos, Vector3 spawnPosition)
        {
            // Проверяем экономику и сетку в одном безопасном месте
            if (!_gridService.CanBuildAt(gridPos) || _bankService.CurrentBalance < shopData.Cost)
            {
                return false;
            }

            // Списываем деньги
            if (!_bankService.SpendMoney(shopData.Cost)) return false;

            // Регистрируем занятость клетки
            GridNode node = _gridService.GetNode(gridPos);
            node.IsOccupied = true;

            // Спавним башню
            GameObject towerGo = _instantiator.InstantiatePrefab(shopData.Prefab, spawnPosition, Quaternion.identity, null);
            
            if (towerGo.TryGetComponent(out TowerFacade towerFacade))
            {
                towerFacade.Initialize(shopData.TowerConfig, gridPos);
            }

            Debug.Log($"<color=green>[TowerFactory] Успешно создана {shopData.TowerConfig.DisplayName} на {gridPos}</color>");
            return true;
        }
    }
}