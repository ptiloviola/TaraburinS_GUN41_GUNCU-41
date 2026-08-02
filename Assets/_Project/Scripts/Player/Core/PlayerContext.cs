using UnityEngine;
using TpsShooter.Services.Input;
using TpsShooter.Player.Configs;

namespace TpsShooter.Player.Core
{
    // Хранит все необходимые данные для работы состояний
    public class PlayerContext
    {
        public readonly CharacterController Controller;
        public readonly Transform Transform;
        public readonly Transform CameraTransform;
        public readonly PlayerConfig Config;
        public readonly IInputService Input;

        // Разделяем горизонтальную и вертикальную скорости для удобства расчетов
        public Vector3 Velocity; 
        public float CurrentRotationVelocity;
        public readonly GroundSensor GroundSensor;
        public readonly Animator Animator;

        public PlayerContext(
            CharacterController controller, 
            Transform transform, 
            Transform cameraTransform, 
            PlayerConfig config, 
            IInputService input,
            GroundSensor groundSensor,
            Animator animator)
        {
            Controller = controller;
            Transform = transform;
            CameraTransform = cameraTransform;
            Config = config;
            Input = input;
            GroundSensor = groundSensor;
            Animator = animator;
        }
    }
}