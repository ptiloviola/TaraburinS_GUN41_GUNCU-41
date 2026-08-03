using UnityEngine;

namespace TpsShooter.Player.Core
{
    public abstract class PlayerBaseState : IPlayerState
    {
        protected readonly PlayerContext Ctx;
        protected readonly PlayerStateMachine StateMachine;

        protected PlayerBaseState(PlayerContext context, PlayerStateMachine stateMachine)
        {
            Ctx = context;
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        
        public virtual void Tick(float deltaTime) 
        {
            ApplyGravity(deltaTime);
        }
        
        public virtual void Exit() { }
        
        public virtual void HandleJump() { } // По умолчанию ничего не делаем

        protected void ApplyGravity(float deltaTime)
        {
            // 1. Применяем гравитацию
            Ctx.Velocity.y += Ctx.Config.Gravity * deltaTime;

            // 2. Сбрасываем накопленную гравитацию, если мы на земле (чтобы не проваливаться)
            if (Ctx.GroundSensor.IsGrounded && Ctx.Velocity.y < 0)
            {
                Ctx.Velocity.y = -2f; // Небольшой минус нужен, чтобы контроллер всегда "прилипал" к полу
            }

            // 3. Двигаем капсулу по вертикали
            Ctx.Controller.Move(Ctx.Velocity * deltaTime);

            // 4. ЖЕСТКАЯ СИНХРОНИЗАЦИЯ С АНИМАТОРОМ
            Ctx.Animator.SetBool("IsGrounded", Ctx.GroundSensor.IsGrounded);
            // Если игрок жмет WASD (длина вектора ввода больше 0), IsMoving будет true
            Ctx.Animator.SetBool("IsMoving", Ctx.Input.MoveAxis.sqrMagnitude > 0.01f);
        }
    }
}