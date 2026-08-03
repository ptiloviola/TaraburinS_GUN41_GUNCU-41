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
            Ctx.MonoBehaviour.StartCoroutine(LerpLayerWeight(1f, 0.2f));
            
            // МАГИЯ CINEMACHINE: Повышаем приоритет. Движок сам плавно переведет камеру!
            Ctx.AimCam.Priority = 20; 
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); 

            Vector2 input = Ctx.Input.MoveAxis;

            Ctx.Animator.SetFloat(Animator.StringToHash("MoveX"), input.x, 0.1f, deltaTime);
            Ctx.Animator.SetFloat(Animator.StringToHash("MoveY"), input.y, 0.1f, deltaTime);
            Ctx.Animator.SetBool("IsMoving", input.sqrMagnitude > 0.01f);

            // Жесткий поворот капсулы за камерой
            float targetAngle = Ctx.CameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(Ctx.Transform.eulerAngles.y, targetAngle, ref Ctx.CurrentRotationVelocity, 0.02f);
            Ctx.Transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Медленный шаг в прицеле
            Vector3 moveDir = Ctx.Transform.right * input.x + Ctx.Transform.forward * input.y;
            Ctx.Controller.Move(moveDir.normalized * (Ctx.Config.AimMoveSpeed * deltaTime));

            if (!Ctx.Input.IsAiming)
            {
                if (input.sqrMagnitude > 0.01f)
                    StateMachine.SwitchState<PlayerMoveState>();
                else
                    StateMachine.SwitchState<PlayerIdleState>();
            }
        }

        public override void Exit()
        {
            base.Exit();
            Ctx.Animator.SetBool(IsAimingHash, false);
            Ctx.MonoBehaviour.StartCoroutine(LerpLayerWeight(0f, 0.25f));
            
            // Отключаем камеру прицеливания (возвращаем приоритет ниже базовой)
            Ctx.AimCam.Priority = 9; 
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
    }
}