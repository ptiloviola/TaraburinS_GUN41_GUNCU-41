using UnityEngine;
using TpsShooter.Weapons.Core;
using TpsShooter.Player;
using Zenject;
using TpsShooter.Audio;

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    public class WeaponPickup : MonoBehaviour, IPickable
    {
        private const string DefaultPickupSound = "Item_Pickup";

        public WeaponBase WeaponInstance { get; private set; }
        private bool _isCollected;
        [Inject] private IAudioService _audioService;

        public void Initialize(WeaponBase weaponInstance)
        {
            WeaponInstance = weaponInstance;
        }

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

                if (playerFacade.WeaponInventory.HasWeapon(WeaponInstance.Config.WeaponName))
                {

                    int ammoToGive = WeaponInstance.CurrentAmmoInClip;
                    
                    if (ammoToGive == 0) ammoToGive = WeaponInstance.Config.AmmoPerClip; 

                    if (playerFacade.InventoryModel.TryAddAmmo(WeaponInstance.Config.WeaponAmmoType, ammoToGive))
                    {
                        DevLogger.Log($"<color=green>[Interaction]</color> Оружие уже есть. Извлечены патроны: +{ammoToGive} {WeaponInstance.Config.WeaponAmmoType}");
                        CompletePickup();
                        return true;
                    }
                    else
                    {
                        DevLogger.Log($"<color=yellow>[Interaction]</color> Патроны {WeaponInstance.Config.WeaponAmmoType} на максимуме. Оружие не тронуто.");
                        return false;
                    }
                }


                if (playerFacade.WeaponInventory.IsFull)
                {
                    DevLogger.Log($"<color=yellow>[Interaction]</color> Нет места для нового оружия!");
                    return false; 
                }


                DevLogger.Log($"<color=green>[Interaction]</color> Подобрано новое оружие: {WeaponInstance.Config.WeaponName}");
                playerFacade.WeaponInventory.AddWeapon(WeaponInstance);
                CompletePickup();
                
                return true;
            }

            return false;
        }

        private void CompletePickup()
        {
            _isCollected = true;
            string soundId = string.IsNullOrEmpty(WeaponInstance.Config.PickupSoundId) 
                ? DefaultPickupSound 
                : WeaponInstance.Config.PickupSoundId;
                
            _audioService?.PlaySFX(soundId, transform.position);
            Destroy(gameObject);
        }
    }
}