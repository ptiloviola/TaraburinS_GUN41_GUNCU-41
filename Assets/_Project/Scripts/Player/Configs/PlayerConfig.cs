using UnityEngine;

namespace TpsShooter.Player.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "TpsShooter/PlayerConfig")]
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

        [Header("Ground Check (Рейкасты)")]
        public LayerMask GroundMask;
        public float GroundCheckRadius = 0.28f; // Чуть меньше радиуса капсулы (0.3), чтобы не цеплять стены
        public float GroundCheckDistance = 0.15f; // Запас "прилипания" к полу
    }
}