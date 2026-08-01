using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerAirborneState : PlayerBaseState
    {
        public PlayerAirborneState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            // Как только коснулись земли и вектор скорости направлен вниз
            if (Ctx.GroundSensor.IsGrounded && Ctx.Velocity.y < 0)
            {
                StateMachine.SwitchState<PlayerIdleState>();
                return;
            }

            // Опционально: контроль в воздухе (Air Control). 
            // Если он не нужен, можно просто оставить base.Tick и ждать приземления.
            Vector2 input = Ctx.Input.MoveAxis;
            if (input.sqrMagnitude > 0.01f)
            {
                Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Ctx.CameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.MoveSpeed * deltaTime));
            }
        }
    }
}