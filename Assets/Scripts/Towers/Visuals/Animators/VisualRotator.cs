using UnityEngine;
using Gameplay.Towers.Data.Visuals;

namespace Gameplay.Towers.Visuals.Animators
{
    public class VisualRotator
    {
        private readonly Transform _logicalRotator;
        private readonly Transform _turret;
        private readonly Transform _elevationPivot;
        private readonly RotationVisualData _config;

        // Эти настройки тоже можно вынести в RotationVisualData позже, если потребуется гибкость
        private readonly float _rotationOffset = -90f; 
        private readonly Vector3 _localPitchAxis = new Vector3(0, 0, -1); 

        public VisualRotator(Transform logicalRotator, Transform turret, Transform elevationPivot, RotationVisualData config)
        {
            _logicalRotator = logicalRotator;
            _turret = turret;
            _elevationPivot = elevationPivot;
            _config = config;
        }

        public void Tick(float deltaTime)
        {
            if (_logicalRotator == null || _turret == null) return;

            // Горизонталь
            float targetYAngle = _logicalRotator.eulerAngles.y + _rotationOffset;
            _turret.rotation = Quaternion.Slerp(_turret.rotation, Quaternion.Euler(0, targetYAngle, 0), _config.Speed * deltaTime);

            // Вертикаль
            if (_elevationPivot != null)
            {
                float pitch = _logicalRotator.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                
                _elevationPivot.localRotation = Quaternion.Slerp(
                    _elevationPivot.localRotation, 
                    Quaternion.Euler(_localPitchAxis * pitch), 
                    _config.Speed * deltaTime
                );
            }
        }
    }
}