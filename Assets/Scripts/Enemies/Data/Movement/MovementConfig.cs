using UnityEngine;

namespace Gameplay.Enemies.Data.Movement
{
    public abstract class MovementConfig : ScriptableObject
    {
        [Header("Общие настройки движения")]
        public float MoveSpeed = 3.5f; 

        public abstract IMovementStrategy CreateStrategy(Vector3 targetPosition);
    }
}