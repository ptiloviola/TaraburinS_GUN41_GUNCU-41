using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); // Применяем гравитацию

            // Если под ногами нет земли, переходим в состояние полета/падения
            if (!Ctx.GroundSensor.IsGrounded)
            {
                StateMachine.SwitchState<PlayerAirborneState>();
                return;
            }

            // Переход в прицеливание
            if (Ctx.Input.IsAiming)
            {
                StateMachine.SwitchState<PlayerAimState>();
                return;
            }

            // Переход в движение
            if (Ctx.Input.MoveAxis.sqrMagnitude > 0.01f)
            {
                StateMachine.SwitchState<PlayerMoveState>();
                return;
            }
        }

        public override void HandleJump()
        {
        #if UNITY_EDITOR
            UnityEngine.Debug.Log($"[Jump Triggered] in {this.GetType().Name}. isGrounded = {Ctx.Controller.isGrounded}");
        #endif
            if (Ctx.GroundSensor.IsGrounded)
            {
                Ctx.Velocity.y = Mathf.Sqrt(Ctx.Config.JumpHeight * -2f * Ctx.Config.Gravity);
                StateMachine.SwitchState<PlayerAirborneState>();
            }
        }
    }
}