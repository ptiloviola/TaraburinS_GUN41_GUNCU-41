using UnityEngine;
using System.Collections;
using TpsShooter.Player.Core;
using Cinemachine;

namespace TpsShooter.Player.States
{
    public class PlayerAimState : PlayerBaseState
    {
        // --- СТРОГОЕ ТЗ: Никаких строк, только хэши ---
        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        
        // Магическое число заменено на константу (или можно тоже вынести в Config)
        private const int UpperBodyLayerIndex = 1;

        private float _normalFov;
        private Vector3 _normalOffset;
        private float _normalXSpeed;
        private float _normalYSpeed;

        private CinemachineComposer _middleRigComposer;

        public PlayerAimState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            
            Ctx.Animator.SetBool(IsAimingHash, true);
            Ctx.MonoBehaviour.StartCoroutine(LerpLayerWeight(1f, Ctx.Config.AimLayerTransitionDuration));

            if (_middleRigComposer == null)
                _middleRigComposer = Ctx.Camera.GetRig(1).GetCinemachineComponent<CinemachineComposer>();

            _normalFov = Ctx.Camera.m_Lens.FieldOfView;
            _normalOffset = _middleRigComposer.m_TrackedObjectOffset;
            _normalXSpeed = Ctx.Camera.m_XAxis.m_MaxSpeed;
            _normalYSpeed = Ctx.Camera.m_YAxis.m_MaxSpeed;

            // Используем множитель из ScriptableObject
            Ctx.Camera.m_XAxis.m_MaxSpeed = _normalXSpeed * Ctx.Config.AimSensitivityMultiplier;
            Ctx.Camera.m_YAxis.m_MaxSpeed = _normalYSpeed * Ctx.Config.AimSensitivityMultiplier;
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); 

            Vector2 input = Ctx.Input.MoveAxis;

            // Используем закэшированные хэши
            Ctx.Animator.SetFloat(MoveXHash, input.x, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, input.y, 0.1f, deltaTime);
            Ctx.Animator.SetBool(IsMovingHash, input.sqrMagnitude > 0.01f);

            // Плавное изменение камеры через настройки из Config
            Ctx.Camera.m_Lens.FieldOfView = Mathf.Lerp(
                Ctx.Camera.m_Lens.FieldOfView, 
                Ctx.Config.AimFov, 
                deltaTime * Ctx.Config.CameraTransitionSpeed
            );
            
            _middleRigComposer.m_TrackedObjectOffset = Vector3.Lerp(
                _middleRigComposer.m_TrackedObjectOffset, 
                Ctx.Config.AimOffset, 
                deltaTime * Ctx.Config.CameraTransitionSpeed
            );

            float targetAngle = Ctx.CameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, 0.02f);
            Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
            Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.AimMoveSpeed * deltaTime));

            if (!Ctx.Input.IsAiming)
            {
                if (input.sqrMagnitude > 0.01f)
                    StateMachine.SwitchState<PlayerMoveState>();
                else
                    StateMachine.SwitchState<PlayerIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            Ctx.Animator.SetBool(IsAimingHash, false);
            Ctx.MonoBehaviour.StartCoroutine(LerpLayerWeight(0f, Ctx.Config.AimLayerTransitionDuration));
            
            Ctx.Camera.m_XAxis.m_MaxSpeed = _normalXSpeed;
            Ctx.Camera.m_YAxis.m_MaxSpeed = _normalYSpeed;
            
            Ctx.MonoBehaviour.StartCoroutine(ResetCameraLerp());
        }

        private IEnumerator LerpLayerWeight(float targetWeight, float duration)
        {
            float startWeight = Ctx.Animator.GetLayerWeight(UpperBodyLayerIndex);
            float time = 0;
            while (time < duration)
            {
                Ctx.Animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(startWeight, targetWeight, time / duration));
                time += Time.deltaTime;
                yield return null; 
            }
            Ctx.Animator.SetLayerWeight(UpperBodyLayerIndex, targetWeight);
        }

        private IEnumerator ResetCameraLerp()
        {
            float time = 0;
            // Возврат камеры делаем чуть дольше/мягче, опираясь на тот же конфиг
            float duration = Ctx.Config.AimLayerTransitionDuration + 0.05f; 
            
            float startFov = Ctx.Camera.m_Lens.FieldOfView;
            Vector3 startOffset = _middleRigComposer.m_TrackedObjectOffset;

            while (time < duration)
            {
                Ctx.Camera.m_Lens.FieldOfView = Mathf.Lerp(startFov, _normalFov, time / duration);
                _middleRigComposer.m_TrackedObjectOffset = Vector3.Lerp(startOffset, _normalOffset, time / duration);
                
                time += Time.deltaTime;
                yield return null;
            }

            Ctx.Camera.m_Lens.FieldOfView = _normalFov;
            _middleRigComposer.m_TrackedObjectOffset = _normalOffset;
        }
    }
}