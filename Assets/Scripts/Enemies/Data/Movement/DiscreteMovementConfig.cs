using UnityEngine;

namespace Gameplay.Enemies.Data.Movement
{
    [CreateAssetMenu(fileName = "DiscreteMovement", menuName = "TD/Enemies/Movement/Discrete")]
    public class DiscreteMovementConfig : MovementConfig
    {
        [Header("Настройки NavMesh")]
        public MovementType PathingType = MovementType.PathOnly;

        [Header("Тайминги прыжка (Геймплей)")]
        public float JumpDuration = 0.6f;
        public float PauseDuration = 0.4f;

        public override IMovementStrategy CreateStrategy(Vector3 targetPosition)
        {
            return new DiscreteMovementStrategy(targetPosition, this);
        }
    }
}