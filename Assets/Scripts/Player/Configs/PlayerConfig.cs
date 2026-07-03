using UnityEngine;

namespace MeatMushrooms.Player.Configs
{
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "MeatMushrooms/Player Config")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Движение")]
        public float WalkSpeed = 2f;
        public float RunSpeed = 5f;
        public float RotationSmoothTime = 0.1f;

        [Header("Шум (Радиус радара)")]
        public float IdleNoise = 0f;
        public float WalkNoise = 3f;
        public float RunNoise = 15f;
    }
}