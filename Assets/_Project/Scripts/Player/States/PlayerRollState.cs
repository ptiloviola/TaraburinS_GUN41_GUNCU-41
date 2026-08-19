using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerRollState : PlayerBaseState
    {
        private static readonly int RollHash = Animator.StringToHash("Roll");
        
        private float _rollTimer;
        private Vector3 _rollDirection;

        public PlayerRollState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            
            Ctx.Animator.SetTrigger(RollHash);
            
            _rollTimer = Ctx.Config.RollDuration;

            Vector2 input = Ctx.Input.MoveAxis;
            
            if (input.sqrMagnitude > InputThreshold)
            {
                Vector3 camForward = Ctx.CameraTransform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                
                Vector3 camRight = Ctx.CameraTransform.right;
                camRight.y = 0f;
                camRight.Normalize();

                _rollDirection = (camRight * input.x + camForward * input.y).normalized;
                
                Ctx.Transform.forward = _rollDirection;
            }
            else
            {
                _rollDirection = Ctx.Transform.forward;
            }
            
            Ctx.Controller.height = Ctx.Config.CrouchHeight;
            Ctx.Controller.center = new Vector3(0f, Ctx.Config.CrouchHeight / 2f, 0f);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime);

            Vector3 velocity = _rollDirection * Ctx.Config.RollSpeed;
            Ctx.Controller.Move(velocity * deltaTime);

            _rollTimer -= deltaTime;
            
            if (_rollTimer <= 0f)
            {
                if (Ctx.Input.IsCrouching)
                    StateMachine.SwitchState<PlayerCrouchState>();
                else if (Ctx.Input.IsAiming)
                    StateMachine.SwitchState<PlayerAimState>();
                else if (Ctx.Input.MoveAxis.sqrMagnitude > InputThreshold)
                    StateMachine.SwitchState<PlayerMoveState>();
                else
                    StateMachine.SwitchState<PlayerIdleState>();
            }
        }


        public override void Exit()
        {
            base.Exit();
            
            Ctx.Controller.height = Ctx.Config.NormalHeight;
            Ctx.Controller.center = new Vector3(0f, Ctx.Config.NormalHeight / 2f, 0f);
            
            Ctx.Animator.ResetTrigger(RollHash);
        }
    }
}