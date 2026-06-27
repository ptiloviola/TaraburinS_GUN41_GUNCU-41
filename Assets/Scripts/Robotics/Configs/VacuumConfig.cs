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

        [Header("Аккумулятор (Battery)")]
        [Tooltip("Максимальный заряд батареи (в секундах работы или условных единицах)")]
        public float MaxBattery = 100f;
        [Tooltip("Трата энергии в секунду просто за то, что робот включен")]
        public float IdleDrainRate = 0.5f;     
        [Tooltip("Трата энергии в секунду при движении (добавляется к Idle)")]
        public float MoveDrainRate = 2.0f;     
        [Tooltip("Базовая трата энергии за всасывание 1 объекта")]
        public float SuctionDrainCost = 1.5f;  

        [Header("Пылесборник (Dustbin)")]
        [Tooltip("Максимальная вместимость бака (например, 100 единиц объема)")]
        public float MaxDustbinCapacity = 100f;

        public float IntakeRadius => _intakeRadius;

        public float MoveSpeed => _moveSpeed;
        public float RotationSpeed => _rotationSpeed;
        public float RayDistance => _rayDistance;
        public float SphereRadius => _sphereRadius;
        public float SideAngle => _sideAngle;
    }
}
