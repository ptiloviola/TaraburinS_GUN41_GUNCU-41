using UnityEngine;
using TpsShooter.Interactables;
using TpsShooter.Player.Weapons;

namespace TpsShooter.Player.Core
{
    public class PlayerInteractionSensor
    {
        private readonly Transform _playerTransform;
        private readonly WeaponInventory _inventory;
        
        // Буфер для результатов физики без аллокации памяти (GC Alloc = 0)
        private readonly Collider[] _colliders = new Collider[5]; 
        private readonly float _radius;

        public PlayerInteractionSensor(Transform playerTransform, WeaponInventory inventory, float radius = 1.5f)
        {
            _playerTransform = playerTransform;
            _inventory = inventory;
            _radius = radius;
        }

        public void Tick()
        {
            // Сканируем сферу вокруг игрока каждый кадр. 
            // QueryTriggerInteraction.Collide позволяет находить наши триггеры WeaponPickup
            int count = Physics.OverlapSphereNonAlloc(
                _playerTransform.position, 
                _radius, 
                _colliders, 
                ~0, // Маска всех слоев. В идеале позже вынесем нужный LayerMask в конфиг
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < count; i++)
            {
                if (_colliders[i].TryGetComponent(out WeaponPickup pickup))
                {
                    Debug.Log($"[Interaction] Подобран предмет: {pickup.Config.WeaponName}");
                    
                    _inventory.AddWeapon(pickup.WeaponPrefab, pickup.Config, pickup.transform.position);
                    pickup.Collect();
                }
            }
        }
    }
}