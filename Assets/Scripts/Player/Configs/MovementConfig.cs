using UnityEngine;
namespace Player.Config
{
    [CreateAssetMenu(fileName = "NewMovementData", menuName = "Configs/Movement Data")]
    public class MovementConfig : ScriptableObject
    {
        [Header("Locomotion")]
        public float moveSpeed = 5f;
        public float gravity = -9.81f;

        [Header("Camera")]
        public float mouseSensitivity = 100f;
    }
}