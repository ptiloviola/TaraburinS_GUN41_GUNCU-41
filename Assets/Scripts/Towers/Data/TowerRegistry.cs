using System.Collections.Generic;
using UnityEngine;
using Gameplay.Towers.Data;
using System.Linq; // Нужно для удобного поиска

namespace Gameplay.Towers.Data
{
    [CreateAssetMenu(fileName = "NewTowerRegistry", menuName = "TD/Tower Registry", order = 50)]
    public class TowerRegistry : ScriptableObject
    {
        [Header("Каталог всех доступных башен")]
        public List<TowerShopData> Towers = new List<TowerShopData>();

        // Метод-помощник: Позволяет быстро найти данные башни по её ID.
        // Это понадобится GridInteractor'у, когда UI скажет ему: "Строй башню laser_tower!"
        // Поиск по текстовому ID (для кнопок UI)
        public TowerShopData GetTowerById(string towerId)
        {
            // Берем ID прямо из вложенного конфига!
            return Towers.FirstOrDefault(t => t.TowerConfig != null && t.TowerConfig.TowerId == towerId);
        }

        // НОВОЕ: Безопасный поиск по ссылке на сам конфиг (без использования текста!)
        public TowerShopData GetTowerByConfig(TowerConfig configToFind)
        {
            return Towers.FirstOrDefault(t => t.TowerConfig == configToFind);
        }
    }
}