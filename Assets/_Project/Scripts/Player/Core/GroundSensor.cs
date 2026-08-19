using UnityEngine;
using TpsShooter.Player.Configs;

namespace TpsShooter.Player.Core
{
    public class GroundSensor
    {
        private const float SphereOffsetY = 0.1f;
        private const float SphereRadius = 0.2f;
        private readonly Transform _transform;
        private readonly PlayerConfig _config;

        public bool IsGrounded { get; private set; }

        public GroundSensor(Transform transform, PlayerConfig config)
        {
            _transform = transform;
            _config = config;
        }

        public void Tick()
        {
            Vector3 spherePosition = _transform.position + (Vector3.up * SphereOffsetY);
            
            IsGrounded = Physics.CheckSphere(
                spherePosition, 
                SphereRadius,
                _config.GroundMask, 
                QueryTriggerInteraction.Ignore
            );
        }
    }
}