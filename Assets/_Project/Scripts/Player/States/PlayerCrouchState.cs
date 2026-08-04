using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerCrouchState : PlayerBaseState
    {
        public PlayerCrouchState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            
            Ctx.Animator.SetBool(IsCrouchingHash, true);
            
            // Сообщаем камере, что мы присели
            Ctx.CameraController.SetCrouching(true);

            Ctx.Controller.height = Ctx.Config.CrouchHeight;
            Ctx.Controller.center = new Vector3(0f, Ctx.Config.CrouchHeight / 2f, 0f);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); // Гравитация
            
            if (Ctx.Input.IsRollTriggered)
            {
                StateMachine.SwitchState<PlayerRollState>();
                return;
            }

            Vector2 input = Ctx.Input.MoveAxis;
            Vector3 inputDir = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

            Vector3 camForward = Ctx.CameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            
            Vector3 camRight = Ctx.CameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            Vector3 moveDir = camRight * inputDir.x + camForward * inputDir.z;

            if (moveDir != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            Vector3 currentVelocity = moveDir * Ctx.Config.CrouchSpeed;
            Ctx.Controller.Move(currentVelocity * deltaTime);

            Vector3 localVelocity = Ctx.Transform.InverseTransformDirection(currentVelocity);
            float animX = localVelocity.x / Ctx.Config.CrouchSpeed;
            float animZ = localVelocity.z / Ctx.Config.CrouchSpeed;

            Ctx.Animator.SetFloat(MoveXHash, animX, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, animZ, 0.1f, deltaTime);

            if (Ctx.Input.IsAiming)
            {
                StateMachine.SwitchState<PlayerAimState>();
                return;
            }

            if (!Ctx.Input.IsCrouching)
            {
                if (CanStandUp())
                {
                    if (inputDir.sqrMagnitude < InputThreshold)
                        StateMachine.SwitchState<PlayerIdleState>();
                    else
                        StateMachine.SwitchState<PlayerMoveState>();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            Ctx.Animator.SetBool(IsCrouchingHash, false);
            
            // Сообщаем камере, что мы встали
            Ctx.CameraController.SetCrouching(false);

            Ctx.Controller.height = Ctx.Config.NormalHeight;
            Ctx.Controller.center = new Vector3(0f, Ctx.Config.NormalHeight / 2f, 0f);
        }

        private bool CanStandUp()
        {
            float castRadius = Ctx.Controller.radius;
            float castDistance = Ctx.Config.NormalHeight - Ctx.Config.CrouchHeight;
            
            Vector3 castOrigin = Ctx.Transform.position + Vector3.up * Ctx.Controller.height;

            bool hitCeiling = Physics.SphereCast(castOrigin, castRadius, Vector3.up, out RaycastHit hit, castDistance);
            
            return !hitCeiling; 
        }
    }
}