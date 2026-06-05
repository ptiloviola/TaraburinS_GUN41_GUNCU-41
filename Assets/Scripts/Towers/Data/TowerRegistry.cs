using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Towers.Data
{
    [CreateAssetMenu(fileName = "NewTowerRegistry", menuName = "TD/Tower Registry", order = 50)]
    public class TowerRegistry : ScriptableObject
    {
        [Header("Каталог всех доступных башен")]
        public List<TowerShopData> Towers = new List<TowerShopData>();

        // Метод-помощник: Позволяет быстро найти данные башни по её ID.
        // Это понадобится GridInteractor'у, когда UI скажет ему: "Строй башню laser_tower!"
        public TowerShopData GetTowerById(string id)
        {
            // Используем LINQ для поиска в списке первого совпадения
            return Towers.Find(tower => tower.TowerId == id);
        }
    }
}