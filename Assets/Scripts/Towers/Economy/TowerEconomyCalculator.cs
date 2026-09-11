using UnityEngine;
using Gameplay.Towers.Data;

namespace Gameplay.Towers.Economy
{
    public static class TowerEconomyCalculator
    {
        public static int CalculateSellValue(TowerConfig config, int currentLevel)
        {
            if (config == null) return 0;

            int totalInvested = config.BaseCost;

            for (int i = 1; i <= currentLevel; i++)
            {
                if (i < config.Levels.Count)
                {
                    totalInvested += config.Levels[i].UpgradeCost;
                }
            }

            return Mathf.RoundToInt(totalInvested * config.SellRefundMultiplier);
        }

        public static int GetUpgradeCost(TowerConfig config, int currentLevel)
        {
            if (config == null || currentLevel + 1 >= config.Levels.Count) return 0;
            return config.Levels[currentLevel + 1].UpgradeCost;
        }
    }
}