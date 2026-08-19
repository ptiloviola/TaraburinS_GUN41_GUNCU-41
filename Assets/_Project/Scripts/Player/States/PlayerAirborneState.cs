using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerAirborneState : PlayerBaseState
    {

        public PlayerAirborneState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            Ctx.Animator.SetBool(IsGroundedHash, false);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            Vector2 input = Ctx.Input.MoveAxis;

            if (input.sqrMagnitude > InputThreshold)
            {
                float targetAngle = Ctx.CameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);
                Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
                Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.MoveSpeed * deltaTime));
            }

            if (Ctx.Velocity.y <= 0f && Ctx.GroundSensor.IsGrounded)
            {
                if (input.sqrMagnitude > InputThreshold)
                {
                    StateMachine.SwitchState<PlayerMoveState>();
                }
                else
                {
                    StateMachine.SwitchState<PlayerIdleState>();
                }
            }
        }
    }
}