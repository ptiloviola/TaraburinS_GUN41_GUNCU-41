using UnityEngine;
using TpsShooter.Services.Input;
using Cinemachine;
using TpsShooter.Player.Configs;
using UnityEngine.Animations.Rigging;
using TpsShooter.Player.Camera;
using TpsShooter.Player.Weapons;

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
        public readonly MonoBehaviour MonoBehaviour;
        public readonly PlayerCameraController CameraController;
        public readonly Transform AimTarget;
        public readonly Rig WeaponRig;

        public readonly PlayerWeaponController WeaponController;
        public readonly WeaponInventory WeaponInventory;
        

        public PlayerContext(
            CharacterController controller, 
            Transform transform, 
            Transform cameraTransform, 
            PlayerConfig config, 
            IInputService input,
            GroundSensor groundSensor,
            Animator animator,
            MonoBehaviour monoBehaviour,
            PlayerCameraController cameraController,
            Transform aimTarget,
            Rig weaponRig,
            PlayerWeaponController weaponController,
            WeaponInventory weaponInventory)
        {
            Controller = controller;
            Transform = transform;
            CameraTransform = cameraTransform;
            Config = config;
            Input = input;
            GroundSensor = groundSensor;
            Animator = animator;
            MonoBehaviour = monoBehaviour;
            CameraController = cameraController;
            AimTarget = aimTarget;
            WeaponRig = weaponRig;
            WeaponController = weaponController;
            WeaponInventory = weaponInventory;
        }
    }
}