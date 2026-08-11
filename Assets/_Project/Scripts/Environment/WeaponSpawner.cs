using UnityEngine;
using Zenject; // <--- Добавили Zenject
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

        private IInstantiator _instantiator;

        // Zenject сам вызовет этот метод при старте сцены и передаст инстанциатор
        [Inject]
        public void Construct(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }

        private void Start()
        {
            SpawnInitialWeapons();
        }

        private void SpawnInitialWeapons()
        {
            if (_spawnPoints == null || _spawnPoints.Length < 2) return;

            SpawnWeapon(_pistolConfig, _pistolPrefab, _spawnPoints[0]);
            SpawnWeapon(_rifleConfig, _riflePrefab, _spawnPoints[1]);
        }

        private void SpawnWeapon(WeaponConfig config, WeaponBase weaponPrefab, Transform spawnPoint)
        {
            // 1. Используем Zenject для создания пикапа
            WeaponPickup pickup = _instantiator.InstantiatePrefabForComponent<WeaponPickup>(
                _basePickupPrefab.gameObject, spawnPoint.position, spawnPoint.rotation, null);

            // 2. ВАЖНО: Используем Zenject для создания пушки! Теперь DecalManager прокинется внутрь.
            WeaponBase weaponInstance = _instantiator.InstantiatePrefabForComponent<WeaponBase>(
                weaponPrefab.gameObject, pickup.transform);
            
            weaponInstance.Initialize(config);
            weaponInstance.enabled = false;
            
            weaponInstance.gameObject.AddComponent<PickupAnimator>();
            pickup.Initialize(weaponInstance);
        }
    }
}