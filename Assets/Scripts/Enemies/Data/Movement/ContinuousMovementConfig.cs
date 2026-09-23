using UnityEngine;

namespace Gameplay.Enemies.Data.Movement
{
    [CreateAssetMenu(fileName = "ContinuousMovement", menuName = "TD/Enemies/Movement/Continuous")]
    public class ContinuousMovementConfig : MovementConfig
    {
        [Header("Настройки маршрута")]
        public MovementType PathingType = MovementType.PathOnly;

        [Header("Слои NavMesh")]
        public string PathAreaName = "CustomPath";
        public string GroundAreaName = "CustomGround";

        public override IMovementStrategy CreateStrategy(Vector3 targetPosition)
        {
            return new ContinuousMovementStrategy(targetPosition, PathingType, PathAreaName, GroundAreaName);
        }
    }
}