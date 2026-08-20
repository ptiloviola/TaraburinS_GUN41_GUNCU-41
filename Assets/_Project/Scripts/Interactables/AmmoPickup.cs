using UnityEngine;
using TpsShooter.Player;
using TpsShooter.Items.Configs;
using Zenject;
using TpsShooter.Audio;


namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class AmmoPickup : MonoBehaviour, IPickable
    {
        private const string DefaultPickupSound = "Item_Pickup";

        [SerializeField] private AmmoItemConfig _config;
        
        private bool _isCollected;

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
                if (playerFacade.InventoryModel.TryAddAmmo(_config.AmmoType, _config.Amount))
                {
                    _isCollected = true;
                    DevLogger.Log($"<color=green>[Interaction]</color> Подобраны патроны: {_config.AmmoType} +{_config.Amount}");
                    
                    string soundId = string.IsNullOrEmpty(_config.PickupSoundId) ? DefaultPickupSound : _config.PickupSoundId;
                    _audioService?.PlaySFX(soundId, transform.position);

                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    DevLogger.Log($"<color=yellow>[Interaction]</color> Запас патронов {_config.AmmoType} полон!");
                }
            }

            return false;
        }
    }
}