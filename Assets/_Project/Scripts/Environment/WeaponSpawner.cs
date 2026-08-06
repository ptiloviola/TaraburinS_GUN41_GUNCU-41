using UnityEngine;
using TpsShooter.Interactables;
using TpsShooter.Weapons.Configs;
using TpsShooter.Weapons.Core;

namespace TpsShooter.Environment
{
    public class WeaponSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [Tooltip("Пустой префаб с триггером и скриптом WeaponPickup")]
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
            if (_spawnPoints.Length < 2)
            {
                Debug.LogWarning("[WeaponSpawner] Недостаточно точек спавна! Нужно минимум 2.");
                return;
            }

            SpawnWeapon(_pistolConfig, _pistolPrefab, _spawnPoints[0]);
            SpawnWeapon(_rifleConfig, _riflePrefab, _spawnPoints[1]);
        }

        private void SpawnWeapon(WeaponConfig config, WeaponBase weaponPrefab, Transform spawnPoint)
        {
            // 1. Создаем корневой объект-триггер
            WeaponPickup pickup = Instantiate(_basePickupPrefab, spawnPoint.position, spawnPoint.rotation);
            pickup.Initialize(config, weaponPrefab);

            // 2. Создаем визуальную 3D-модель оружия как дочернюю
            WeaponBase visualModel = Instantiate(weaponPrefab, pickup.transform);
            
            // 3. Отключаем боевую логику, так как на земле это просто визуал
            Destroy(visualModel.GetComponent<WeaponBase>());
            
            // 4. Добавляем красивую анимацию левитации
            visualModel.gameObject.AddComponent<PickupAnimator>();
        }
    }
}