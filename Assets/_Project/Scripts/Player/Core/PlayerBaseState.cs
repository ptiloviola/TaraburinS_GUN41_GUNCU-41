using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public abstract class PlayerBaseState : IPlayerState
    {

        protected static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        protected static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        protected static readonly int MoveXHash = Animator.StringToHash("MoveX");
        protected static readonly int MoveYHash = Animator.StringToHash("MoveY");
        protected static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");


        protected const float StickToGroundVelocity = -2f; 
        protected const float InputThreshold = 0.01f;
        protected const float AnimDampTime = 0.1f;  

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
        
        public virtual void HandleJump() { } 

        protected void ApplyGravity(float deltaTime)
        {

            Ctx.Velocity.y += Ctx.Config.Gravity * deltaTime;

            if (Ctx.GroundSensor.IsGrounded && Ctx.Velocity.y < 0)
            {
                Ctx.Velocity.y = StickToGroundVelocity; 
            }

            Ctx.Animator.SetBool(IsGroundedHash, Ctx.GroundSensor.IsGrounded);
            
            Ctx.Animator.SetBool(IsMovingHash, Ctx.Input.MoveAxis.sqrMagnitude > InputThreshold);
        }
    }
}