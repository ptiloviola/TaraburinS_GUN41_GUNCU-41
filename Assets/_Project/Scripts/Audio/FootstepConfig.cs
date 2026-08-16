using UnityEngine;

namespace TpsShooter.Audio
{
    [CreateAssetMenu(fileName = "FootstepConfig", menuName = "TpsShooter/Audio/Footstep Config")]
    public class FootstepConfig : ScriptableObject
    {
        [Header("Raycast Settings")]
        [Tooltip("Откуда пускаем луч (смещение вверх от Root)")]
        public float RaycastOffset = 0.5f;
        [Tooltip("Длина луча")]
        public float RaycastDistance = 1.0f;
        public LayerMask GroundMask;

        [Header("Sound IDs (from MainAudioConfig)")]
        public string DefaultStep = "Step_Concrete";
        public string MetalStep = "Step_Metal";
        public string DirtStep = "Step_Dirt";
        public string WoodStep = "Step_Wood";
    }
}