using UnityEngine;
using TpsShooter.Player;
using TpsShooter.Items.Configs;
using Zenject;
using TpsShooter.Audio;

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class HealthPickup : MonoBehaviour, IPickable
    {
        [SerializeField] private HealthItemConfig _config;
        private bool _isCollected = false; // Защита от двойного подбора в одном кадре
        [Inject] private IAudioService _audioService;

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
                // ОБНОВЛЕНО: Обращаемся к новому HealthEngine
                if (playerFacade.Health != null && playerFacade.Health.TryHeal(_config.HealAmount))
                {
                    _isCollected = true;
                    DevLogger.Log($"<color=green>[Interaction]</color> Подобрана аптечка. Восстановлено {_config.HealAmount} ХП. Текущее ХП: {playerFacade.Health.CurrentHealth}");
                    _audioService?.PlaySFX("Item_Pickup", transform.position);
                    Destroy(gameObject); 
                    return true; 
                }
                else
                {
                    DevLogger.Log($"<color=yellow>[Interaction]</color> Здоровье полное (или игрок мертв)! Аптечка не подобрана.");
                }
            }

            return false;
        }
    }
}