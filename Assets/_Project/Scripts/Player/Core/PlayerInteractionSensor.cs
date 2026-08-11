using UnityEngine;
using TpsShooter.Interactables;

namespace TpsShooter.Player.Core
{
    public class PlayerInteractionSensor
    {
        private readonly Transform _playerTransform;
        private readonly Collider[] _colliders = new Collider[5]; 
        private readonly float _radius;

        // Убрали зависимость от WeaponInventory! Сенсор теперь максимально легкий.
        public PlayerInteractionSensor(Transform playerTransform, float radius = 1.5f)
        {
            _playerTransform = playerTransform;
            _radius = radius;
        }

        public void Tick()
        {
            int count = Physics.OverlapSphereNonAlloc(
                _playerTransform.position, 
                _radius, 
                _colliders, 
                ~0, 
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < count; i++)
            {
                // Ищем любой объект, реализующий интерфейс IPickable (оружие, аптечка, патроны)
                if (_colliders[i].TryGetComponent(out IPickable pickup))
                {
                    // Передаем предмету самого игрока. Предмет сам решит, можно ли его подобрать.
                    pickup.TryPickup(_playerTransform.gameObject);
                }
            }
        }
    }
}