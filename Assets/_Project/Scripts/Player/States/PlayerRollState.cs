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
            
            // 1. Запускаем анимацию через Триггер
            Ctx.Animator.SetTrigger(RollHash);
            
            // 2. Взводим таймер
            _rollTimer = Ctx.Config.RollDuration;

            // 3. Вычисляем направление переката
            Vector2 input = Ctx.Input.MoveAxis;
            
            // Если игрок двигался, кувыркаемся по направлению ввода относительно камеры
            if (input.sqrMagnitude > 0.01f)
            {
                Vector3 camForward = Ctx.CameraTransform.forward;
                camForward.y = 0f;
                camForward.Normalize();
                
                Vector3 camRight = Ctx.CameraTransform.right;
                camRight.y = 0f;
                camRight.Normalize();

                _rollDirection = (camRight * input.x + camForward * input.y).normalized;
                
                // Мгновенно поворачиваем модельку лицом в сторону кувырка
                Ctx.Transform.forward = _rollDirection;
            }
            else
            {
                // Если стоял на месте — кувыркаемся туда, куда смотрел
                _rollDirection = Ctx.Transform.forward;
            }
            
            // Если нужно, чтобы капсула уменьшалась во время переката, как в приседе:
            Ctx.Controller.height = Ctx.Config.CrouchHeight;
            Ctx.Controller.center = new Vector3(0f, Ctx.Config.CrouchHeight / 2f, 0f);
        }

        public override void Tick(float deltaTime)
        {
            base.Tick(deltaTime); // Применяем гравитацию

            // Двигаем капсулу
            Vector3 velocity = _rollDirection * Ctx.Config.RollSpeed;
            Ctx.Controller.Move(velocity * deltaTime);

            // Обновляем таймер
            _rollTimer -= deltaTime;
            
            // --- ЛОГИКА ВЫХОДА ---
            if (_rollTimer <= 0f)
            {
                // Проверяем, в какое состояние нужно вернуться
                if (Ctx.Input.IsCrouching)
                    StateMachine.SwitchState<PlayerCrouchState>();
                else if (Ctx.Input.IsAiming)
                    StateMachine.SwitchState<PlayerAimState>();
                else if (Ctx.Input.MoveAxis.sqrMagnitude > 0.01f)
                    StateMachine.SwitchState<PlayerMoveState>();
                else
                    StateMachine.SwitchState<PlayerIdleState>();
            }
        }

        protected override void HandleShooting()
        {
            // Игрок кувыркается, стрелять нельзя!
        }



        public override void Exit()
        {
            base.Exit();
            
            // Возвращаем нормальный размер капсулы
            Ctx.Controller.height = Ctx.Config.NormalHeight;
            Ctx.Controller.center = new Vector3(0f, Ctx.Config.NormalHeight / 2f, 0f);
            
            // Сбрасываем триггер на всякий случай, чтобы анимация не "залипла"
            Ctx.Animator.ResetTrigger(RollHash);
        }
    }
}