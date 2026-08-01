using UnityEngine;
using TpsShooter.Player.Configs;

namespace TpsShooter.Player.Core
{
    public class GroundSensor
    {
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
            // Создаем невидимую сферу прямо под ногами персонажа.
            // Смещаем ее центр вверх (на 0.1), чтобы радиус (0.2) 
            // гарантированно цеплял и низ капсулы, и пол под ней.
            Vector3 spherePosition = _transform.position + (Vector3.up * 0.1f);
            
            IsGrounded = Physics.CheckSphere(
                spherePosition, 
                0.2f, // Статичный надежный радиус для проверки
                _config.GroundMask, 
                QueryTriggerInteraction.Ignore
            );
        }
    }
}