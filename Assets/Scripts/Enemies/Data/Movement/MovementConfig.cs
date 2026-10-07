using UnityEngine;
using Gameplay.Enemies.Movement;

namespace Gameplay.Enemies.Data.Movement
{
    public abstract class MovementConfig : ScriptableObject
    {
        public abstract IMovementStrategy CreateStrategy(Vector3 targetPosition);
    }
}