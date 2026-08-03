using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerIdleState : PlayerBaseState
    {
        private static readonly int JumpTriggerHash = Animator.StringToHash("Jump");
        private static readonly int TurnHash = Animator.StringToHash("Turn");
        public PlayerIdleState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            // 1. Плавный сброс скорости движения
            Ctx.Animator.SetFloat(MoveXHash, 0f, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, 0f, 0.1f, deltaTime);

            // --- НОВАЯ ЛОГИКА: Поворот за камерой на месте ---
            float cameraYaw = Ctx.CameraTransform.eulerAngles.y;
            float playerYaw = Ctx.Transform.eulerAngles.y;
            
            // Вычисляем кратчайший угол между взглядом персонажа и камеры (от -180 до 180 градусов)
            float deltaAngle = Mathf.DeltaAngle(playerYaw, cameraYaw);
            float turnValue = 0f;

            // Если камера отклонилась больше чем на 2 градуса, начинаем переступать ногами
            if (Mathf.Abs(deltaAngle) > 2f)
            {
                // Физически вращаем капсулу
                float angle = Mathf.SmoothDampAngle(playerYaw, cameraYaw, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // Превращаем угол в значение от -1 до 1 для Аниматора. 
                // Делим на 90, чтобы при отклонении камеры на 90 градусов анимация играла на максимальной скорости.
                turnValue = Mathf.Clamp(deltaAngle / 15f, -1f, 1f);
            }

            // Передаем значение поворота в Аниматор с небольшим сглаживанием
            Ctx.Animator.SetFloat(TurnHash, turnValue, 0.1f, deltaTime);
            // -------------------------------------------------

            // Проверки переходов
            if (!Ctx.GroundSensor.IsGrounded)
            {
                StateMachine.SwitchState<PlayerAirborneState>();
                return;
            }

            if (Ctx.Input.IsAiming)
            {
                StateMachine.SwitchState<PlayerAimState>();
                return;
            }

            if (Ctx.Input.MoveAxis.sqrMagnitude > InputThreshold)
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