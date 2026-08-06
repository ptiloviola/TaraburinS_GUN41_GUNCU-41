using UnityEngine;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerAimState : PlayerBaseState
    {
        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");

        public PlayerAimState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            Ctx.Animator.SetBool(IsAimingHash, true);
            Ctx.CameraController.SetAiming(true);
            Ctx.WeaponController.SetAiming(true); // Контроллер сам плавно поднимет веса
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); 

            bool isCrouching = Ctx.Input.IsCrouching;
            Ctx.Animator.SetBool(IsCrouchingHash, isCrouching);
            Ctx.CameraController.SetCrouching(isCrouching);

            float targetHeight = isCrouching ? Ctx.Config.CrouchHeight : Ctx.Config.NormalHeight;
            Ctx.Controller.height = targetHeight;
            Ctx.Controller.center = new Vector3(0f, targetHeight / 2f, 0f);
            float currentSpeed = isCrouching ? Ctx.Config.CrouchSpeed : Ctx.Config.AimMoveSpeed;

            Vector2 input = Ctx.Input.MoveAxis;
            Ctx.Animator.SetFloat(MoveXHash, input.x, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, input.y, 0.1f, deltaTime);
            Ctx.Animator.SetBool(IsMovingHash, input.sqrMagnitude > 0.01f);

            // Поворот игрока за камерой
            float targetAngle = Ctx.CameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, 0.02f);
            Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            // Движение
            Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
            Ctx.Controller.Move(moveDir.normalized * (currentSpeed * deltaTime));

            // ЖЕЛЕЗОБЕТОННЫЙ ПРИЦЕЛ: Строго по центру камеры, никаких лучей.
            Ctx.AimTarget.position = Ctx.CameraTransform.position + Ctx.CameraTransform.forward * 50f;

            // Выход из прицеливания
            if (!Ctx.Input.IsAiming)
            {
                if (isCrouching) StateMachine.SwitchState<PlayerCrouchState>();
                else if (input.sqrMagnitude > 0.01f) StateMachine.SwitchState<PlayerMoveState>();
                else StateMachine.SwitchState<PlayerIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            Ctx.Animator.SetBool(IsAimingHash, false);
            Ctx.CameraController.SetAiming(false);
            Ctx.WeaponController.SetAiming(false); // Контроллер сам плавно опустит веса
        }
    }
}