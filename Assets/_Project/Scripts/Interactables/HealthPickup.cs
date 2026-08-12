using UnityEngine;
using TpsShooter.Player;

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class HealthPickup : MonoBehaviour, IPickable
    {
        // Пока оставим хил в инспекторе, позже можем вынести в HealthItemConfig
        [SerializeField] private float _healAmount = 25f; 
        private bool _isCollected = false; // Защита от двойного подбора в одном кадре

        // 1. АВТОМАТИЧЕСКИЙ ПОДБОР ПРИ КАСАНИИ
        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            // Передаем того, кто в нас наступил, в наш же метод подбора
            TryPickup(other.gameObject);
        }

        // 2. УНИВЕРСАЛЬНЫЙ КОНТРАКТ
        public bool TryPickup(GameObject collector)
        {
            if (_isCollected) return false;

            if (collector.TryGetComponent(out PlayerFacade playerFacade))
            {
                if (playerFacade.InventoryModel.TryHeal(_healAmount))
                {
                    _isCollected = true;
                    Debug.Log($"<color=green>[Interaction]</color> Подобрана аптечка. Восстановлено {_healAmount} ХП. Текущее ХП: {playerFacade.InventoryModel.CurrentHealth}");
                    
                    Destroy(gameObject); 
                    return true; 
                }
                else
                {
                    Debug.Log($"<color=yellow>[Interaction]</color> Здоровье полное! Аптечка не подобрана.");
                }
            }

            return false;
        }
    }
}