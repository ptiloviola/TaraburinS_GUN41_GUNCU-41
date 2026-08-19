using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
        private static readonly int TurnHash = Animator.StringToHash("Turn");

        protected const float TurnAngleThreshold = 2f;
        protected const float TurnBlendDivisor = 15f;
        public PlayerIdleState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            if (Ctx.Input.IsRollTriggered)
            {
                StateMachine.SwitchState<PlayerRollState>();
                return;
            }


            Ctx.Animator.SetFloat(MoveXHash, 0f, AnimDampTime, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, 0f, AnimDampTime, deltaTime);


            float cameraYaw = Ctx.CameraTransform.eulerAngles.y;
            float playerYaw = Ctx.Transform.eulerAngles.y;
            

            float deltaAngle = Mathf.DeltaAngle(playerYaw, cameraYaw);
            float turnValue = 0f;


            if (Mathf.Abs(deltaAngle) > TurnAngleThreshold)
            {

                float angle = Mathf.SmoothDampAngle(playerYaw, cameraYaw, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

                turnValue = Mathf.Clamp(deltaAngle / TurnBlendDivisor, -1f, 1f);
            }


            Ctx.Animator.SetFloat(TurnHash, turnValue, AnimDampTime, deltaTime);


            if (!Ctx.GroundSensor.IsGrounded)
            {
                StateMachine.SwitchState<PlayerAirborneState>();
                return;
            }

            if (Ctx.Input.IsAiming && Ctx.WeaponController.IsArmed)
            {
                StateMachine.SwitchState<PlayerAimState>();
                return;
            }

            if (Ctx.Input.MoveAxis.sqrMagnitude > InputThreshold)
            {
                StateMachine.SwitchState<PlayerMoveState>();
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
            DevLogger.Log($"[Jump Triggered] in {this.GetType().Name}. isGrounded = {Ctx.Controller.isGrounded}");
            if (Ctx.GroundSensor.IsGrounded)
            {
                Ctx.Animator.SetTrigger(JumpTriggerHash);

                Ctx.Velocity.y = Mathf.Sqrt(Ctx.Config.JumpHeight * -2f * Ctx.Config.Gravity);
                
                StateMachine.SwitchState<PlayerAirborneState>();
            }
        }
    }
}