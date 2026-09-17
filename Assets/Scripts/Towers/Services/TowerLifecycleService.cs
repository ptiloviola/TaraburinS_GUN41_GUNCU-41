using UnityEngine;
using Gameplay.Grid;
using Gameplay.Economy;
using Gameplay.Towers.Economy;

namespace Gameplay.Towers.Services
{
    public class TowerLifecycleService
    {
        private readonly BankService _bankService;
        private readonly IGridService _gridService;

        public TowerLifecycleService(BankService bankService, IGridService gridService)
        {
            _bankService = bankService;
            _gridService = gridService;
        }

        public void SellTower(TowerFacade tower)
        {
            if (tower == null) return;

            int sellValue = TowerEconomyCalculator.CalculateSellValue(tower.Config, tower.CurrentLevel);
            _bankService.AddMoney(sellValue);

            GridNode node = _gridService.GetNode(tower.GridPosition);
            if (node != null)
            {
                node.IsOccupied = false;
            }

            Object.Destroy(tower.gameObject);
        }
    }
}