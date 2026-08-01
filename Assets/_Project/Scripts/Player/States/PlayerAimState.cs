using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerAimState : PlayerBaseState
    {
        public PlayerAimState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            // Если отпустили кнопку прицеливания — возвращаемся в Idle
            if (!Ctx.Input.IsAiming)
            {
                StateMachine.SwitchState<PlayerIdleState>();
                return;
            }

            // Жестко привязываем поворот персонажа к камере
            float targetAngle = Ctx.CameraTransform.eulerAngles.y;
            Ctx.Transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

            Vector2 input = Ctx.Input.MoveAxis;
            if (input.sqrMagnitude > 0.01f)
            {
                // Движение стрейфами (вправо/влево, вперед/назад)
                Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
                Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.AimMoveSpeed * deltaTime));
            }
        }
        public override void HandleJump()
        {
            if (Ctx.GroundSensor.IsGrounded)
            {
                Ctx.Velocity.y = Mathf.Sqrt(Ctx.Config.JumpHeight * -2f * Ctx.Config.Gravity);
                StateMachine.SwitchState<PlayerAirborneState>();
            }
        }
    }
    
}