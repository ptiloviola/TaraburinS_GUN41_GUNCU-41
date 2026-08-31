using UnityEngine;

namespace Gameplay.Enemies.Data.Movement
{
    public abstract class MovementConfig : ScriptableObject
    {
        [Header("Общие настройки движения")]
        public float MoveSpeed = 3.5f; 

        // Фабричный метод: конфиг сам решает, какую логику (класс) создать
        public abstract IMovementStrategy CreateStrategy(Vector3 targetPosition);
    }
}