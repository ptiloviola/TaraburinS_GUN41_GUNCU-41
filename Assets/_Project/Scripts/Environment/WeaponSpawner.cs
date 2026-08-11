using UnityEngine;
using TpsShooter.Interactables;
using TpsShooter.Weapons.Configs;
using TpsShooter.Weapons.Core;

namespace TpsShooter.Environment
{
    public class WeaponSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private WeaponPickup _basePickupPrefab; 
        
        [Header("Spawn Points")]
        [SerializeField] private Transform[] _spawnPoints;
        
        [Header("Loadout to Spawn")]
        [SerializeField] private WeaponConfig _pistolConfig;
        [SerializeField] private WeaponBase _pistolPrefab;
        
        [SerializeField] private WeaponConfig _rifleConfig;
        [SerializeField] private WeaponBase _riflePrefab;

        private void Start()
        {
            SpawnInitialWeapons();
        }

        private void SpawnInitialWeapons()
        {
            if (_spawnPoints.Length < 2) return;

            SpawnWeapon(_pistolConfig, _pistolPrefab, _spawnPoints[0]);
            SpawnWeapon(_rifleConfig, _riflePrefab, _spawnPoints[1]);
        }

        private void SpawnWeapon(WeaponConfig config, WeaponBase weaponPrefab, Transform spawnPoint)
        {
            // 1. Создаем корневой триггер
            WeaponPickup pickup = Instantiate(_basePickupPrefab, spawnPoint.position, spawnPoint.rotation);

            // 2. Создаем ЖИВОЕ оружие и инициализируем его патронами из конфига
            WeaponBase weaponInstance = Instantiate(weaponPrefab, pickup.transform);
            weaponInstance.Initialize(config);
            
            // 3. Отключаем скрипт, чтобы оружие не стреляло с пола
            weaponInstance.enabled = false;
            
            // 4. Вешаем красивую анимацию левитации на саму пушку
            weaponInstance.gameObject.AddComponent<PickupAnimator>();

            // 5. Передаем инстанс в пикап
            pickup.Initialize(weaponInstance);
        }
    }
}