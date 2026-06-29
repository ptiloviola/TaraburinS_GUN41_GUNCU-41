using UnityEngine;

namespace Gameplay.Towers.Visuals.Components
{
    public class VisualRotator : MonoBehaviour
    {
        [Header("Логика")]
        [SerializeField] private Transform _logicalRotator; 

        [Header("Горизонталь (Yaw)")]
        [SerializeField] private Transform _turretTransform;
        [SerializeField] private float _rotationOffset = -90f;

        [Header("Вертикаль (Pitch)")]
        [SerializeField] private bool _usePitch = false;
        [SerializeField] private Transform _elevationPivot;
        [SerializeField] private Vector3 _localPitchAxis = new Vector3(0, 0, -1);

        private void Awake()
        {
            if (_logicalRotator == null && transform.parent != null)
            {
                _logicalRotator = transform.parent.Find("Logical_Rotator");
            }
        }

        private void LateUpdate()
        {
            if (_logicalRotator == null || _turretTransform == null) return;

            // 1. ГОРИЗОНТАЛЬ
            float targetYAngle = _logicalRotator.eulerAngles.y + _rotationOffset;
            _turretTransform.rotation = Quaternion.Euler(0, targetYAngle, 0);

            // 2. ВЕРТИКАЛЬ
            if (_usePitch && _elevationPivot != null)
            {
                float pitch = _logicalRotator.eulerAngles.x;
                if (pitch > 180f) pitch -= 360f;
                _elevationPivot.localRotation = Quaternion.Euler(_localPitchAxis * pitch);
            }
        }
    }
}