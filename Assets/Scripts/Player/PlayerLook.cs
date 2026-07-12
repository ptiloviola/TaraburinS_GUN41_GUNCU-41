using UnityEngine;
using Player.Config;
namespace Player
{

    public class PlayerLook
    {
        private readonly Transform _playerBody;
        private readonly Transform _cameraTransform;
        private readonly MovementConfig _config;
        
        private float _xRotation = 0f;

        public PlayerLook(Transform playerBody, Transform cameraTransform, MovementConfig config)
        {
            _playerBody = playerBody;
            _cameraTransform = cameraTransform;
            _config = config;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void Tick(Vector2 lookInput, float deltaTime)
        {
            float mouseX = lookInput.x * _config.mouseSensitivity * deltaTime;
            float mouseY = lookInput.y * _config.mouseSensitivity * deltaTime;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, -90f, 90f); // Ограничиваем, чтобы не сломать шею

            _cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            
            _playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}