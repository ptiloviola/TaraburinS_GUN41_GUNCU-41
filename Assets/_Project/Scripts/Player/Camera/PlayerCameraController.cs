using UnityEngine;
using Cinemachine;
using TpsShooter.Player.Configs;

namespace TpsShooter.Player.Camera
{
    public class PlayerCameraController
    {
        private const int MiddleRigIndex = 1;
        private readonly PlayerConfig _config;
        private readonly Transform _cameraTarget;
        private readonly CinemachineFreeLook _freeLookCamera;
        private readonly CinemachineComposer _middleRigComposer;

        private readonly float _normalFov;
        private readonly Vector3 _normalOffset;
        private readonly float _normalXSpeed;
        private readonly float _normalYSpeed;

        private bool _isAiming;
        private bool _isCrouching;

        public PlayerCameraController(PlayerConfig config, Transform cameraTarget, CinemachineFreeLook freeLookCamera)
        {
            _config = config;
            _cameraTarget = cameraTarget;
            _freeLookCamera = freeLookCamera;

            _middleRigComposer = _freeLookCamera.GetRig(MiddleRigIndex).GetCinemachineComponent<CinemachineComposer>();

            _normalFov = _freeLookCamera.m_Lens.FieldOfView;
            _normalOffset = _middleRigComposer.m_TrackedObjectOffset;
            _normalXSpeed = _freeLookCamera.m_XAxis.m_MaxSpeed;
            _normalYSpeed = _freeLookCamera.m_YAxis.m_MaxSpeed;
        }

        public void SetAiming(bool isAiming)
        {
            _isAiming = isAiming;
            _freeLookCamera.m_XAxis.m_MaxSpeed = _isAiming ? _normalXSpeed * _config.AimSensitivityMultiplier : _normalXSpeed;
            _freeLookCamera.m_YAxis.m_MaxSpeed = _isAiming ? _normalYSpeed * _config.AimSensitivityMultiplier : _normalYSpeed;
        }

        public void SetCrouching(bool isCrouching)
        {
            _isCrouching = isCrouching;
        }

        public void Tick(float deltaTime)
        {
            HandleCameraTargetHeight(deltaTime);
            HandleAimTransitions(deltaTime);
        }

        private void HandleCameraTargetHeight(float deltaTime)
        {
            float targetHeight = _isCrouching ? _config.CrouchCameraHeight : _config.NormalCameraHeight;
            Vector3 localPos = _cameraTarget.localPosition;
            localPos.y = Mathf.Lerp(localPos.y, targetHeight, deltaTime * _config.CameraTransitionSpeed);
            _cameraTarget.localPosition = localPos;
        }

        private void HandleAimTransitions(float deltaTime)
        {
            float targetFov = _isAiming ? _config.AimFov : _normalFov;
            Vector3 targetOffset = _isAiming ? _config.AimOffset : _normalOffset;

            _freeLookCamera.m_Lens.FieldOfView = Mathf.Lerp(
                _freeLookCamera.m_Lens.FieldOfView, 
                targetFov, 
                deltaTime * _config.CameraTransitionSpeed
            );
            
            _middleRigComposer.m_TrackedObjectOffset = Vector3.Lerp(
                _middleRigComposer.m_TrackedObjectOffset, 
                targetOffset, 
                deltaTime * _config.CameraTransitionSpeed
            );
        }
    }
}