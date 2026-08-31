using UnityEngine;

namespace Gameplay.Enemies.Data.Movement
{
    [CreateAssetMenu(fileName = "ContinuousMovement", menuName = "TD/Enemies/Movement/Continuous")]
    public class ContinuousMovementConfig : MovementConfig
    {
        [Header("Настройки NavMesh")]
        public MovementType PathingType = MovementType.PathOnly;

        public override IMovementStrategy CreateStrategy(Vector3 targetPosition)
        {
            // Возвращаем нашу старую добрую стратегию
            return new NavMeshMovement(targetPosition, PathingType);
        }
    }
}