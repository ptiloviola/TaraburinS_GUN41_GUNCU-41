using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerMoveState : PlayerBaseState
    {
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
        private static readonly int TurnHash = Animator.StringToHash("Turn");

        public PlayerMoveState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {

            base.Tick(deltaTime);
            
            if (Ctx.Input.IsRollTriggered)
            {
                StateMachine.SwitchState<PlayerRollState>();
                return;
            }

            Vector2 input = Ctx.Input.MoveAxis;
            Vector3 inputDir = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

            float targetSpeed = Ctx.Input.IsRunning ? Ctx.Config.RunSpeed : Ctx.Config.MoveSpeed;

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


            Vector3 horizontalVelocity = moveDir * targetSpeed;
            Vector3 finalVelocity = horizontalVelocity;
            

            finalVelocity.y = Ctx.Velocity.y; 


            Ctx.Controller.Move(finalVelocity * deltaTime);


            Vector3 localVelocity = Ctx.Transform.InverseTransformDirection(horizontalVelocity);


            float animX = localVelocity.x / Ctx.Config.RunSpeed;
            float animZ = localVelocity.z / Ctx.Config.RunSpeed;

            Ctx.Animator.SetFloat(MoveXHash, animX, AnimDampTime, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, animZ, AnimDampTime, deltaTime);
            Ctx.Animator.SetFloat(TurnHash, 0f, AnimDampTime, deltaTime);


            if (inputDir.sqrMagnitude < InputThreshold)
            {
                StateMachine.SwitchState<PlayerIdleState>();
                return;
            }

            if (Ctx.Input.IsAiming && Ctx.WeaponController.IsArmed)
            {
                StateMachine.SwitchState<PlayerAimState>();
                return;
            }
            if (Ctx.Input.IsCrouching)
            {
                StateMachine.SwitchState<PlayerCrouchState>();
                return;
            }
        }

        public override void HandleJump()
        {
            DevLogger.Log($"[Jump Triggered] in {this.GetType().Name}. isGrounded = {Ctx.GroundSensor.IsGrounded}");

            if (Ctx.GroundSensor.IsGrounded)
            {
                Ctx.Animator.SetTrigger(JumpTriggerHash);
                
                Ctx.Velocity.y = Mathf.Sqrt(Ctx.Config.JumpHeight * -2f * Ctx.Config.Gravity);
                
                StateMachine.SwitchState<PlayerAirborneState>();
            }
        }
    }
}