using UnityEngine;
using TpsShooter.Interactables;

namespace TpsShooter.Player.Core
{
    public class PlayerInteractionSensor
    {
        private const int MaxInteractions = 5;
        
        private readonly Transform _playerTransform;
        private readonly Collider[] _colliders = new Collider[MaxInteractions]; 
        private readonly float _radius;
        private readonly int _interactableMask;

        public PlayerInteractionSensor(Transform playerTransform, float radius, int interactableMask)
        {
            _playerTransform = playerTransform;
            _radius = radius;
            _interactableMask = interactableMask;
        }

        public void Tick()
        {
            int count = Physics.OverlapSphereNonAlloc(
                _playerTransform.position, 
                _radius, 
                _colliders, 
                _interactableMask,
                QueryTriggerInteraction.Collide
            );

            for (int i = 0; i < count; i++)
            {
                if (_colliders[i].TryGetComponent(out IPickable pickup))
                {
                    pickup.TryPickup(_playerTransform.gameObject);
                }
            }
        }
    }
}