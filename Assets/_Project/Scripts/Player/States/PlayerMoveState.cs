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

            Vector2 input = Ctx.Input.MoveAxis;
            
            // 1. Применяем ClampMagnitude по ТЗ (ограничиваем диагональную скорость)
            Vector3 inputDir = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

            // 2. Логика бега: определяем целевую скорость
            float targetSpeed = Ctx.Input.IsRunning ? Ctx.Config.RunSpeed : Ctx.Config.MoveSpeed;

            // 3. Приведение к базису камеры (ТЗ: camera.forward без Y, нормализация)
            Vector3 camForward = Ctx.CameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            
            Vector3 camRight = Ctx.CameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();

            // 4. Вычисляем финальный вектор движения в мировых координатах
            // Внимание: inputDir.z содержит значение Y со стика/кнопок
            Vector3 moveDir = camRight * inputDir.x + camForward * inputDir.z;

            // 5. ПОВОРОТ ПЕРСОНАЖА (ТЗ: "поворачивается по направлению движения")
            if (moveDir != Vector3.zero)
            {
                float targetAngle = Mathf.Atan2(moveDir.x, moveDir.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, Ctx.Config.RotationSmoothTime);
                Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }

            // 6. Физическое движение
            Vector3 currentVelocity = moveDir * targetSpeed;
            Ctx.Controller.Move(currentVelocity * deltaTime);

            // --- 7. ВЫЧИСЛЕНИЕ ДЛЯ BLEND TREE (InverseTransformDirection по ТЗ) ---
            
            // Магия математики: переводим мировую скорость в локальную систему координат персонажа.
            // Так как персонаж поворачивается по ходу движения, localVelocity.z всегда будет положительным (мы идем лицом вперед), 
            // а localVelocity.x будет не нулевым только в момент поворота (игрок будет наклоняться вбок).
            Vector3 localVelocity = Ctx.Transform.InverseTransformDirection(currentVelocity);

            // Заменяем твой множитель currentMaxSpeed умной математикой.
            // Допустим MoveSpeed = 2, а RunSpeed = 4. 
            // При беге localVelocity.z будет равен 4. Если разделить 4 на 2, получим ровно 2! 
            // В BlendTree уйдет 2, и включится анимация бега автоматически.
            float animX = localVelocity.x / Ctx.Config.MoveSpeed;
            float animZ = localVelocity.z / Ctx.Config.MoveSpeed;

            Ctx.Animator.SetFloat(MoveXHash, animX, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, animZ, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(TurnHash, 0f, 0.1f, deltaTime);

            // Логика переходов
            if (inputDir.sqrMagnitude < InputThreshold)
            {
                StateMachine.SwitchState<PlayerIdleState>();
                return; // Важно прерывать выполнение после переключения
            }

            if (Ctx.Input.IsAiming)
            {
                StateMachine.SwitchState<PlayerAimState>();
                return;
            }
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