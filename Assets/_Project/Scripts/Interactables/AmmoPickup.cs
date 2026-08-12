using UnityEngine;
using TpsShooter.Player;
using TpsShooter.Player.Inventory;

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class AmmoPickup : MonoBehaviour, IPickable
    {
        [Header("Ammo Settings")]
        [SerializeField] private AmmoType _ammoType = AmmoType.Rifle;
        [SerializeField] private int _amount = 30;
        
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
                if (playerFacade.InventoryModel.TryAddAmmo(_ammoType, _amount))
                {
                    _isCollected = true;
                    Debug.Log($"<color=green>[Interaction]</color> Подобраны патроны: {_ammoType} +{_amount}");
                    
                    // ТУТ БУДЕТ ЗВУК И ЭФФЕКТ

                    Destroy(gameObject);
                    return true;
                }
                else
                {
                    Debug.Log($"<color=yellow>[Interaction]</color> Запас патронов {_ammoType} полон!");
                }
            }

            return false;
        }
    }
}