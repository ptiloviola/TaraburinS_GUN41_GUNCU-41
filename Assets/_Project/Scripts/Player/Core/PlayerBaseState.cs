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
            if (Ctx.Controller.isGrounded && Ctx.Velocity.y < 0)
            {
                Ctx.Velocity.y = -2f;
            }

            Ctx.Velocity.y += Ctx.Config.Gravity * deltaTime;
            Ctx.Controller.Move(Vector3.up * (Ctx.Velocity.y * deltaTime));
        }
    }
}