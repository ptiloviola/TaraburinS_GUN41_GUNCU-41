using UnityEngine;
namespace Player
{
    public class PlayerMovement
    {
        private readonly CharacterController _controller;
        private readonly Transform _playerTransform;
        private readonly PlayerConfig _config;
        
        private float _velocityY;

        public PlayerMovement(CharacterController controller, Transform transform, PlayerConfig config)
        {
            _controller = controller;
            _playerTransform = transform;
            _config = config;
        }

        public void Tick(Vector2 inputDirection, float deltaTime)
        {
            // Локальное движение (учитывает, куда повернут игрок)
            Vector3 move = _playerTransform.right * inputDirection.x + _playerTransform.forward * inputDirection.y;
            
            // Гравитация
            if (_controller.isGrounded && _velocityY < 0)
            {
                _velocityY = -2f; // Небольшой прижим к полу
            }
            _velocityY += _config.gravity * deltaTime;

            Vector3 velocity = move * _config.moveSpeed + Vector3.up * _velocityY;
            _controller.Move(velocity * deltaTime);
        }
    }
}