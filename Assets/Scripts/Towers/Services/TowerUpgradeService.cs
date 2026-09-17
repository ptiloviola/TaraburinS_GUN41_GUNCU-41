using System;
using UnityEngine;
using Gameplay.Economy;
using Gameplay.Towers.Data;
using Gameplay.Towers.Factories;

namespace Gameplay.Towers.Services
{
    public class TowerUpgradeService
    {
        private readonly BankService _bankService;
        private readonly TowerFactory _towerFactory;

        public event Action<TowerFacade, TowerFacade> OnTowerUpgraded; 

        public TowerUpgradeService(BankService bankService, TowerFactory towerFactory)
        {
            _bankService = bankService;
            _towerFactory = towerFactory;
        }

        public bool TryUpgradeTower(TowerFacade oldTower)
        {
            if (!oldTower.CanUpgrade()) return false;

            int nextLevel = oldTower.CurrentLevel + 1;
            TowerLevelData nextLevelData = oldTower.Config.Levels[nextLevel];

            if (_bankService.CurrentBalance < nextLevelData.UpgradeCost) return false;

            if (!_bankService.SpendMoney(nextLevelData.UpgradeCost)) return false;

            TowerConfig config = oldTower.Config;
            Vector2Int gridPos = oldTower.GridPosition;
            Vector3 worldPos = oldTower.transform.position;

            UnityEngine.Object.Destroy(oldTower.gameObject);

            TowerFacade newTower = _towerFactory.ForceSpawnTower(config, nextLevel, gridPos, worldPos);

            Debug.Log($"<color=cyan>[TowerUpgradeService] Башня {config.DisplayName} улучшена до уровня {nextLevel}</color>");
            
            OnTowerUpgraded?.Invoke(oldTower, newTower);
            
            return true;
        }
    }
}