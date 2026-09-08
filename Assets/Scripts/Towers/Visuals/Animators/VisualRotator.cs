using UnityEngine;
using Gameplay.Towers.Data.Visuals;

namespace Gameplay.Towers.Visuals.Animators
{
    public class VisualRotator
    {
        private readonly Transform _logicalRotator;
        private readonly Transform _visualTurret;
        
        // НОВОЕ: Разделяем логический (математика) и визуальный (рендеринг) стволы
        private readonly Transform _logicalElevation;
        private readonly Transform _visualElevation;
        
        private readonly RotationVisualData _config;

        private readonly float _rotationOffset = -90f; 
        private readonly Vector3 _localPitchAxis = new Vector3(0, 0, -1); 

        public VisualRotator(Transform logicalRotator, Transform visualTurret, Transform logicalElevation, Transform visualElevation, RotationVisualData config)
        {
            _logicalRotator = logicalRotator;
            _visualTurret = visualTurret;
            _logicalElevation = logicalElevation;
            _visualElevation = visualElevation;
            _config = config;
        }

        public void Tick(float deltaTime)
        {
            if (_logicalRotator == null || _visualTurret == null) return;

            // Горизонталь всегда берется с базового логического ротатора
            float targetYAngle = _logicalRotator.eulerAngles.y + _rotationOffset;
            _visualTurret.rotation = Quaternion.Slerp(_visualTurret.rotation, Quaternion.Euler(0, targetYAngle, 0), _config.Speed * deltaTime);

            // Вертикаль
            if (_visualElevation != null)
            {
                float pitch = 0f;

                // Читаем чистый угол наклона из логического механизма (для зенитки)
                if (_logicalElevation != null)
                {
                    pitch = _logicalElevation.localEulerAngles.x;
                }
                // Фолбэк: если отдельного ствола нет, берем наклон из базы (для старых башен)
                else if (_logicalRotator != null)
                {
                    pitch = _logicalRotator.eulerAngles.x;
                }

                if (pitch > 180f) pitch -= 360f;
                
                // Плавно применяем угол к визуальной модели
                _visualElevation.localRotation = Quaternion.Slerp(
                    _visualElevation.localRotation, 
                    Quaternion.Euler(_localPitchAxis * pitch), 
                    _config.Speed * deltaTime
                );
            }
        }
    }
}