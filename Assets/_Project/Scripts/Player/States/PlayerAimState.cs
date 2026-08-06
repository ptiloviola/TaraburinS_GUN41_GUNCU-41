using UnityEngine;
using System.Collections;
using TpsShooter.Player.Core;

namespace TpsShooter.Player.States
{
    public class PlayerAimState : PlayerBaseState
    {
        private static readonly int IsAimingHash = Animator.StringToHash("IsAiming");
        private const int UpperBodyLayerIndex = 1;

        public PlayerAimState(PlayerContext context, PlayerStateMachine stateMachine) 
            : base(context, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            
            Ctx.Animator.SetBool(IsAimingHash, true);
            
            // Командуем камере перейти в режим прицеливания
            Ctx.CameraController.SetAiming(true);
            
            Ctx.MonoBehaviour.StartCoroutine(LerpLayerWeight(1f, Ctx.Config.AimLayerTransitionDuration));
            Ctx.MonoBehaviour.StartCoroutine(LerpRigWeight(1f, Ctx.Config.AimLayerTransitionDuration));
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); 

            // --- 2. ЛОГИКА ПРИСЕДА В ПРИЦЕЛИВАНИИ ---
            bool isCrouching = Ctx.Input.IsCrouching;
            
            Ctx.Animator.SetBool(IsCrouchingHash, isCrouching);
            
            // Командуем камере опуститься или подняться
            Ctx.CameraController.SetCrouching(isCrouching);

            float targetHeight = isCrouching ? Ctx.Config.CrouchHeight : Ctx.Config.NormalHeight;
            Ctx.Controller.height = targetHeight;
            Ctx.Controller.center = new Vector3(0f, targetHeight / 2f, 0f);

            float currentSpeed = isCrouching ? Ctx.Config.CrouchSpeed : Ctx.Config.AimMoveSpeed;

            // --- 3. МАТЕМАТИКА ДВИЖЕНИЯ ---
            Vector2 input = Ctx.Input.MoveAxis;

            Ctx.Animator.SetFloat(MoveXHash, input.x, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(MoveYHash, input.y, 0.1f, deltaTime);
            Ctx.Animator.SetBool(IsMovingHash, input.sqrMagnitude > 0.01f);

            float targetAngle = Ctx.CameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, 0.02f);
            Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
            Ctx.Controller.Move(moveDir.normalized * (currentSpeed * deltaTime));

            // --- 4. МАГИЯ IK ---
            Vector3 aimPosition = Ctx.CameraTransform.position + Ctx.CameraTransform.forward * 50f;
            Ctx.AimTarget.position = aimPosition;

            // --- 5. ЛОГИКА ВЫХОДА ИЗ ПРИЦЕЛИВАНИЯ ---
            if (!Ctx.Input.IsAiming)
            {
                if (isCrouching)
                {
                    StateMachine.SwitchState<PlayerCrouchState>();
                }
                else
                {
                    if (input.sqrMagnitude > 0.01f)
                        StateMachine.SwitchState<PlayerMoveState>();
                    else
                        StateMachine.SwitchState<PlayerIdleState>();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            
            Ctx.Animator.SetBool(IsAimingHash, false);
            
            // Возвращаем камеру в нормальный режим
            Ctx.CameraController.SetAiming(false);
            
            Ctx.MonoBehaviour.StartCoroutine(LerpLayerWeight(0f, Ctx.Config.AimLayerTransitionDuration));
            Ctx.MonoBehaviour.StartCoroutine(LerpRigWeight(0f, Ctx.Config.AimLayerTransitionDuration));
        }


        private IEnumerator LerpLayerWeight(float targetWeight, float duration)
        {
            float startWeight = Ctx.Animator.GetLayerWeight(UpperBodyLayerIndex);
            float time = 0;
            while (time < duration)
            {
                Ctx.Animator.SetLayerWeight(UpperBodyLayerIndex, Mathf.Lerp(startWeight, targetWeight, time / duration));
                time += Time.deltaTime;
                yield return null; 
            }
            Ctx.Animator.SetLayerWeight(UpperBodyLayerIndex, targetWeight);
        }

        private IEnumerator LerpRigWeight(float targetWeight, float duration)
        {
            float startWeight = Ctx.WeaponRig.weight;
            float time = 0;
            while (time < duration)
            {
                Ctx.WeaponRig.weight = Mathf.Lerp(startWeight, targetWeight, time / duration);
                time += Time.deltaTime;
                yield return null; 
            }
            Ctx.WeaponRig.weight = targetWeight;
        }
    }
}