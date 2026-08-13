using UnityEngine;

namespace TpsShooter.Player.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "TpsShooter/PlayerConfigs/PlayerConfig")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        public float MoveSpeed = 5f;
        public float RunSpeed = 8f; // <-- ДОБАВЛЕНО: Скорость бега
        public float AimMoveSpeed = 2.5f; // При прицеливании ходим медленнее
        public float RotationSmoothTime = 0.1f;
        
        [Header("Physics")]
        public float Gravity = -15f;
        public float JumpHeight = 1.2f;

        [Header("Stats")]
        public float MaxHealth = 100f;

        [Header("Ground Check (Рейкасты)")]
        public LayerMask GroundMask;
        public float GroundCheckRadius = 0.28f; // Чуть меньше радиуса капсулы (0.3), чтобы не цеплять стены
        public float GroundCheckDistance = 0.15f; // Запас "прилипания" к полу

        [Header("Camera Aim Settings")]
        [Tooltip("FOV при прицеливании")]
        public float AimFov = 25f;
        
        [Tooltip("Смещение камеры за плечо (X, Y, Z)")]
        public Vector3 AimOffset = new Vector3(0.6f, 0.1f, 0f);
        
        [Tooltip("Множитель чувствительности мыши в прицеле (от 0 до 1)")]
        [Range(0.1f, 1f)] 
        public float AimSensitivityMultiplier = 0.4f;
        
        [Tooltip("Скорость перехода камеры (FOV и Offset)")]
        public float CameraTransitionSpeed = 10f;

        [Header("Animation Settings")]
        [Tooltip("Время плавного поднятия/опускания рук с оружием")]
        public float AimLayerTransitionDuration = 0.2f;

        [Header("Crouch Settings")]
        [Tooltip("Скорость передвижения в приседе")]
        public float CrouchSpeed = 2f;

        [Tooltip("Высота CharacterController в приседе")]
        public float CrouchHeight = 1f;

        [Tooltip("Обычная высота CharacterController")]
        public float NormalHeight = 2f;

        [Header("Camera Crouch Settings")]
        [Tooltip("Нормальная высота объекта CameraTarget (по оси Y)")]
        public float NormalCameraHeight = 1.5f; 

        [Tooltip("Высота CameraTarget в приседе")]
        public float CrouchCameraHeight = 0.9f;

        [Header("Roll Settings")]
        [Tooltip("Скорость смещения капсулы во время переката")]
        public float RollSpeed = 7f;

        [Tooltip("Длительность переката в секундах (подгоняется под длину анимации)")]
        public float RollDuration = 0.75f;


    }
}