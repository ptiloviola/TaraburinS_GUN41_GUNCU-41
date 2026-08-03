using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerAirborneState : PlayerBaseState
    {
        private static readonly int JumpHash = Animator.StringToHash("Jump");

        public PlayerAirborneState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            // Запускаем триггер анимации прыжка только один раз при входе
            Ctx.Animator.SetTrigger(JumpHash);
            // Принудительно говорим аниматору, что мы в воздухе
            Ctx.Animator.SetBool("IsGrounded", false); 
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); // Вызывает гравитацию из PlayerBaseState

            Vector2 input = Ctx.Input.MoveAxis;

            // --- УПРАВЛЕНИЕ В ВОЗДУХЕ ---
            if (input.sqrMagnitude > 0.01f)
            {
                // Позволяем игроку поворачиваться за камерой в прыжке
                float targetAngle = Ctx.CameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, 0.05f);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // Двигаемся в воздухе. Можно умножить на коэффициент (например, 0.8f), 
                // если хочешь, чтобы в воздухе было сложнее маневрировать
                Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
                Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.MoveSpeed * deltaTime));
            }

            // --- ПРОВЕРКА ПРИЗЕМЛЕНИЯ ---
            // Важно: мы переходим обратно только если мы уже падаем (Velocity.y <= 0)
            // Иначе стейт мгновенно прервет прыжок, пока сенсор еще касается земли на старте
            if (Ctx.Velocity.y <= 0f && Ctx.GroundSensor.IsGrounded)
            {
                if (input.sqrMagnitude > 0.01f)
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