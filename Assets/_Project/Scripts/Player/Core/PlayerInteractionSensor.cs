using UnityEngine;
using TpsShooter.Interactables;
using TpsShooter.Player.Weapons;

namespace TpsShooter.Player.Core
{
    public class PlayerInteractionSensor
    {
        private readonly Transform _playerTransform;
        private readonly WeaponInventory _inventory;
        
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
            // Если инвентарь полон, нет смысла проверять физику
            if (_inventory.IsFull) return;

            int count = Physics.OverlapSphereNonAlloc(
                _playerTransform.position, 
                _radius, 
                _colliders, 
                ~0, 
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < count; i++)
            {
                if (_colliders[i].TryGetComponent(out WeaponPickup pickup))
                {
                    Debug.Log($"[Interaction] Подобран предмет: {pickup.WeaponInstance.Config.WeaponName}");
                    
                    // Передаем ЖИВУЮ пушку в инвентарь
                    _inventory.AddWeapon(pickup.WeaponInstance);
                    pickup.Collect();
                }
            }
        }
    }
}