using UnityEngine;
using TpsShooter.Player;
using TpsShooter.Player.Inventory;
using TpsShooter.Items.Configs;


namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class AmmoPickup : MonoBehaviour, IPickable
    {
        // [Header("Ammo Settings")]
        // [SerializeField] private AmmoType _ammoType = AmmoType.Rifle;
        // [SerializeField] private int _amount = 30;

        [SerializeField] private AmmoItemConfig _config;
        
        private bool _isCollected;

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;
            TryPickup(other.gameObject);
        }

        public bool TryPickup(GameObject collector)
        {
            if (_isCollected) return false;

            if (collector.TryGetComponent(out PlayerFacade playerFacade))
            {
                if (playerFacade.InventoryModel.TryAddAmmo(_config.AmmoType, _config.Amount))
                {
                    _isCollected = true;
                    Debug.Log($"<color=green>[Interaction]</color> Подобраны патроны: {_config.AmmoType} +{_config.Amount}");
                    
                    // ТУТ БУДЕТ ЗВУК И ЭФФЕКТ

                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"<color=yellow>[Interaction]</color> Запас патронов {_config.AmmoType} полон!");
                }
            }

            return false;
        }
    }
}