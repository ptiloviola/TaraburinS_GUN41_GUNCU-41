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

        public void Initialize(WeaponBase weaponInstance)
        {
            WeaponInstance = weaponInstance;
        }

        // Новая логика подбора
        public bool TryPickup(GameObject collector)
        {
            // Проверяем, что нас подбирает именно игрок (у него есть фасад)
            if (collector.TryGetComponent(out PlayerFacade playerFacade))
            {
                // Проверяем, есть ли место для оружия
                if (playerFacade.WeaponInventory.IsFull)
                {
                    // Оружие УЖЕ есть в руках/на спине. 
                    // ТУТ МЫ ПОЗЖЕ НАПИШЕМ ЛОГИКУ ДОБАВЛЕНИЯ ПАТРОНОВ ИЗ ПУШКИ В ИНВЕНТАРЬ
                    return false; 
                }

                Debug.Log($"[Interaction] Подобран предмет: {WeaponInstance.Config.WeaponName}");
                
                // Передаем пушку в инвентарь игрока
                playerFacade.WeaponInventory.AddWeapon(WeaponInstance);
                
                // Уничтожаем объект-контейнер
                Destroy(gameObject);
                
                return true;
            }

            return false;
        }
    }
}