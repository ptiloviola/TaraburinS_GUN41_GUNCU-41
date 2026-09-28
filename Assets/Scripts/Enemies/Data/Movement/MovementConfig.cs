using UnityEngine;

namespace Gameplay.Enemies.Data.Movement
{
    public abstract class MovementConfig : ScriptableObject
    {
        public abstract IMovementStrategy CreateStrategy(Vector3 targetPosition);
    }
}