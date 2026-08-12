using UnityEngine;
using Zenject;
using TpsShooter.Items.Configs;
using TpsShooter.Interactables;
using TpsShooter.Weapons.Core;

namespace TpsShooter.Environment
{
    public class LootFactory
    {
        private readonly IInstantiator _instantiator;

        public LootFactory(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        // 1. УНИВЕРСАЛЬНЫЙ СПАВН (Для уровня и врагов)
        public void SpawnLoot(ItemConfig config, Vector3 position, Quaternion rotation)
        {
            if (config.Prefab == null)
            {
                Debug.LogError($"[LootFactory] В конфиге {config.name} не назначен базовый Prefab (контейнер)!");
                return;
            }

            GameObject pickupObj = _instantiator.InstantiatePrefab(config.Prefab, position, rotation, null);

            if (config is WeaponItemConfig weaponItemConfig)
            {
                // ЗАЩИТА ОТ ДУРАКА
                if (weaponItemConfig.LiveWeaponPrefab == null)
                {
                    Debug.LogError($"[LootFactory] В конфиге {config.name} не назначен LiveWeaponPrefab! Пушка не появится.");
                    return;
                }

                if (pickupObj.TryGetComponent(out WeaponPickup weaponPickup))
                {
                    WeaponBase liveWeapon = _instantiator.InstantiatePrefabForComponent<WeaponBase>(
                        weaponItemConfig.LiveWeaponPrefab.gameObject, pickupObj.transform);
                    
                    liveWeapon.Initialize(weaponItemConfig.WeaponConfig);
                    liveWeapon.enabled = false; 
                    
                    if (liveWeapon.GetComponent<PickupAnimator>() == null)
                        liveWeapon.gameObject.AddComponent<PickupAnimator>();

                    weaponPickup.Initialize(liveWeapon);
                }
            }
        }

        // 2. ВЫБРОС ОРУЖИЯ ИГРОКОМ (Сохраняем текущие патроны)
        public void DropLiveWeapon(WeaponBase weapon, Vector3 position, Quaternion rotation)
        {
            // Создаем контейнер на лету
            GameObject pickupObj = new GameObject($"Pickup_{weapon.Config.WeaponName}");
            pickupObj.transform.position = position;
            pickupObj.transform.rotation = rotation;

            SphereCollider col = pickupObj.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.5f;

            WeaponPickup pickup = pickupObj.AddComponent<WeaponPickup>();
            pickup.Initialize(weapon);

            // Прячем пушку в контейнер
            weapon.enabled = false;
            weapon.transform.SetParent(pickup.transform);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            if (weapon.GetComponent<PickupAnimator>() == null)
                weapon.gameObject.AddComponent<PickupAnimator>();
        }
    }
}