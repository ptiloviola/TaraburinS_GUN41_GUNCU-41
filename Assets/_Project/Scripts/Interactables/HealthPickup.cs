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
        private const string DefaultPickupSound = "Item_Pickup";
        [SerializeField] private HealthItemConfig _config;
        private bool _isCollected = false;
        [Inject] private IAudioService _audioService;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;

            TryPickup(other.gameObject);
        }

        public bool TryPickup(GameObject collector)
        {
            if (_isCollected || _config == null) return false;

            if (collector.TryGetComponent(out PlayerFacade playerFacade))
            {
                if (playerFacade.Health != null && playerFacade.Health.TryHeal(_config.HealAmount))
                {
                    _isCollected = true;
                    DevLogger.Log($"<color=green>[Interaction]</color> Подобрана аптечка. Восстановлено {_config.HealAmount} ХП. Текущее ХП: {playerFacade.Health.CurrentHealth}");
                    string soundId = string.IsNullOrEmpty(_config.PickupSoundId) ? DefaultPickupSound : _config.PickupSoundId;
                    _audioService?.PlaySFX(soundId, transform.position);
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