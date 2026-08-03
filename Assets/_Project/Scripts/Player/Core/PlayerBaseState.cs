using UnityEngine;

namespace TpsShooter.Player.Core
{
    public abstract class PlayerBaseState : IPlayerState
    {
        // --- Кэшируем хэши параметров аниматора (вычисляются 1 раз при запуске) ---
        protected static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        protected static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        protected static readonly int MoveXHash = Animator.StringToHash("MoveX");
        protected static readonly int MoveYHash = Animator.StringToHash("MoveY");

        // --- Избавляемся от магических чисел ---
        protected const float StickToGroundVelocity = -2f; // Скорость прилипания к полу
        protected const float InputThreshold = 0.01f;      // Порог срабатывания стика/клавиш

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

            // 2. Сбрасываем накопленную гравитацию, если мы на земле
            if (Ctx.GroundSensor.IsGrounded && Ctx.Velocity.y < 0)
            {
                Ctx.Velocity.y = StickToGroundVelocity; 
            }

            // 3. Двигаем капсулу по вертикали
            Ctx.Controller.Move(Ctx.Velocity * deltaTime);

            // 4. Синхронизация с Аниматором через хэши
            Ctx.Animator.SetBool(IsGroundedHash, Ctx.GroundSensor.IsGrounded);
            
            // Проверка ввода через константу
            Ctx.Animator.SetBool(IsMovingHash, Ctx.Input.MoveAxis.sqrMagnitude > InputThreshold);
        }
    }
}