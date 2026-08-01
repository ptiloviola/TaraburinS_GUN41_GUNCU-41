using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerMoveState : PlayerBaseState
    {
        public PlayerMoveState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); // Применяем гравитацию

            Vector2 input = Ctx.Input.MoveAxis;

            // Если игрок отпустил кнопки — переходим в Idle
            if (input.sqrMagnitude < 0.01f)
            {
                StateMachine.SwitchState<PlayerIdleState>();
                return;
            }

            // Логика поворота и движения относительно камеры
            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Ctx.CameraTransform.eulerAngles.y;
            
            float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
            Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.MoveSpeed * deltaTime));
        }

        public override void HandleJump()
        {
        #if UNITY_EDITOR
            UnityEngine.Debug.Log($"[Jump Triggered] in {this.GetType().Name}. isGrounded = {Ctx.GroundSensor.IsGrounded}");
        #endif
            if (Ctx.GroundSensor.IsGrounded)
            {
                Ctx.Velocity.y = Mathf.Sqrt(Ctx.Config.JumpHeight * -2f * Ctx.Config.Gravity);
                StateMachine.SwitchState<PlayerAirborneState>();
            }
        }
    }
}