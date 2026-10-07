using UnityEngine;
using Gameplay.Enemies.Movement;
using Gameplay.Enemies.Movement.Strategies;

namespace Gameplay.Enemies.Data.Movement
{
    [CreateAssetMenu(fileName = "DynamicFlightMovement", menuName = "TD/Enemies/Movement/Dynamic Flight")]
    public class DynamicFlightMovementConfig : MovementConfig
    {
        [Header("Настройки маршрута (Воздух)")]
        public MovementType PathingType = MovementType.PathOnly;
        public string PathAreaName = "CustomPath";
        public string GroundAreaName = "CustomGround";

        [Header("Настройки полета (Синусоида)")]
        public float BaseHeight = 5f;
        public float Amplitude = 2f;
        public float Frequency = 1.5f;

        [Header("Взлет и Посадка")]
        [Tooltip("Дистанция от спавна, на которой враг набирает высоту")]
        public float TakeoffDistance = 3f;
        
        [Tooltip("Дистанция до базы, на которой враг начинает снижение")]
        public float LandingDistance = 4f;

        public override IMovementStrategy CreateStrategy(Vector3 targetPosition)
        {
            return new DynamicFlightMovementStrategy(
                targetPosition, PathingType, PathAreaName, GroundAreaName, 
                BaseHeight, Amplitude, Frequency, TakeoffDistance, LandingDistance
            );
        }
    }
}