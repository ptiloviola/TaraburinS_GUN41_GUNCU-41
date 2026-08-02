using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerMoveState : PlayerBaseState
    {
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
        private static readonly int TurnHash = Animator.StringToHash("Turn");

        public PlayerMoveState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            Vector2 input = Ctx.Input.MoveAxis;
            
            // Определяем, бежим мы или идем (например, по нажатию Shift в InputService)
            float currentMaxSpeed = Ctx.Input.IsRunning ? 2f : 1f;

            // Вместо input.x передаем input.x * currentMaxSpeed
            // Теперь, если зажат Shift, в Аниматор уйдет 2 или -2, и включится анимация бега.
            Ctx.Animator.SetFloat(MoveXHash, input.x * currentMaxSpeed, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, input.y * currentMaxSpeed, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(TurnHash, 0f, 0.1f, deltaTime);

            // Логика переключения состояний...
            if (input.sqrMagnitude < 0.01f)
            {
                StateMachine.SwitchState<PlayerIdleState>();
                return;
            }
            
            // --- ПОВОРОТ КАПСУЛЫ ЗА КАМЕРОЙ ---
            float targetAngle = Ctx.CameraTransform.eulerAngles.y;
            // Используем очень быстрое сглаживание (0.05f вместо RotationSmoothTime), чтобы модель не "плавала"
            float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, 0.05f);
            Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // --- ФИЗИЧЕСКОЕ ДВИЖЕНИЕ ---
            float speed = Ctx.Input.IsRunning ? Ctx.Config.RunSpeed : Ctx.Config.MoveSpeed;
            Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
            
            // Передаем только горизонтальную скорость (по X и Z), 
            // вертикальная скорость (по Y) рассчитывается в PlayerBaseState.Tick
            Vector3 finalMove = moveDir.normalized * speed;
            Ctx.Controller.Move(finalMove * deltaTime);
        }

        public override void HandleJump()
        {
        #if UNITY_EDITOR
            UnityEngine.Debug.Log($"[Jump Triggered] in {this.GetType().Name}. isGrounded = {Ctx.GroundSensor.IsGrounded}");
        #endif
            if (Ctx.GroundSensor.IsGrounded)
            {
                // 1. Активируем анимацию в Аниматоре
                Ctx.Animator.SetTrigger(JumpTriggerHash);
                
                // 2. Логика физики прыжка
                Ctx.Velocity.y = Mathf.Sqrt(Ctx.Config.JumpHeight * -2f * Ctx.Config.Gravity);
                
                // 3. Переключаем состояние кода
                StateMachine.SwitchState<PlayerAirborneState>();
            }
        }
    }
}