using UnityEngine;

namespace VacuumSim.Robotics.Configs
{
    [CreateAssetMenu(fileName = "VacuumConfig", menuName = "VacuumSim/Vacuum Config")]
    public class VacuumConfig : ScriptableObject
    {
        [Header("Настройки движения")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _rotationSpeed = 120f;

        [Header("Настройки сенсоров (Зрения)")]
        [SerializeField] private float _rayDistance = 1.5f;
        [SerializeField] private float _sphereRadius = 0.4f;
        [SerializeField] private float _sideAngle = 45f;

        [Header("Настройки всасывания (Коллектор)")]
        [SerializeField] private float _intakeRadius = 0.5f;
        public float IntakeRadius => _intakeRadius;
        

        // Публичные свойства только для чтения (геттеры)
        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float RayDistance => _rayDistance;
        public float SphereRadius => _sphereRadius;
        public float SideAngle => _sideAngle;
    }
}
