using UnityEngine;
using TpsShooter.Weapons.Core;
using TpsShooter.Player; // Для доступа к PlayerFacade

namespace TpsShooter.Interactables
{
    [RequireComponent(typeof(Collider))]
    // Реализуем интерфейс IPickable
    public class WeaponPickup : MonoBehaviour, IPickable
    {
        public WeaponBase WeaponInstance { get; private set; }
        private bool _isCollected;

        public void Initialize(WeaponBase weaponInstance)
        {
            WeaponInstance = weaponInstance;
        }

        // Подбор по касанию
        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;
            TryPickup(other.gameObject);
        }

        // Новая логика подбора
        public bool TryPickup(GameObject collector)
        {
            if (_isCollected) return false;

            if (collector.TryGetComponent(out PlayerFacade playerFacade))
            {
                // 1. ПРОВЕРКА: ЕСТЬ ЛИ УЖЕ ТАКАЯ ПУШКА?
                if (playerFacade.WeaponInventory.HasWeapon(WeaponInstance.Config.WeaponName))
                {
                    // Пушка есть! Пытаемся забрать из нее патроны.
                    int ammoToGive = WeaponInstance.TotalAmmo;
                    // Если пушка только что заспавнилась (TotalAmmo == 0), даем хотя бы один магазин
                    if (ammoToGive == 0) ammoToGive = WeaponInstance.Config.AmmoPerClip; 

                    if (playerFacade.InventoryModel.TryAddAmmo(WeaponInstance.Config.WeaponAmmoType, ammoToGive))
                    {
                        Debug.Log($"<color=green>[Interaction]</color> Оружие уже есть. Извлечены патроны: +{ammoToGive} {WeaponInstance.Config.WeaponAmmoType}");
                        _isCollected = true;
                        Destroy(gameObject); // Уничтожаем лежащую пушку
                        return true;
                    }
                    else
                    {
                        Debug.Log($"<color=yellow>[Interaction]</color> Патроны {WeaponInstance.Config.WeaponAmmoType} на максимуме. Оружие не тронуто.");
                        return false;
                    }
                }

                // 2. ПУШКИ НЕТ. ПРОВЕРКА: ЕСТЬ ЛИ МЕСТО В РУКАХ/НА СПИНЕ?
                if (playerFacade.WeaponInventory.IsFull)
                {
                    Debug.Log($"<color=yellow>[Interaction]</color> Нет места для нового оружия!");
                    return false; 
                }

                // 3. МЕСТО ЕСТЬ. ПОДБИРАЕМ ПУШКУ.
                Debug.Log($"<color=green>[Interaction]</color> Подобрано новое оружие: {WeaponInstance.Config.WeaponName}");
                _isCollected = true;
                playerFacade.WeaponInventory.AddWeapon(WeaponInstance);
                Destroy(gameObject);
                
                return true;
            }

            return false;
        }
    }
}