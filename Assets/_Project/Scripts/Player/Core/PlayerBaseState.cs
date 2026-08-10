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
        protected static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

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
            // Эта логика теперь выполняется КАЖДЫЙ кадр в ЛЮБОМ состоянии, 
            // которое вызывает base.Tick(deltaTime);
            HandleShooting();
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

        // Делаем метод виртуальным, чтобы наследники могли его запретить
        protected virtual void HandleShooting()
        {
            // 1. Проверяем инпут и наличие оружия
            if (!Ctx.Input.IsFiring || Ctx.WeaponInventory.CurrentWeapon == null) 
                return;

            // 2. Ищем точку прицеливания от камеры
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray cameraRay = UnityEngine.Camera.main.ScreenPointToRay(screenCenter);
            Vector3 targetPoint;

            // ВАЖНО: Камера не должна попадать лучом в самого игрока (иначе мы будем стрелять себе в затылок).
            // В идеале тут нужен LayerMask, исключающий слой Player. 
            // Пока берем все слои кроме IgnoreRaycast.
            int mask = ~LayerMask.GetMask("Ignore Raycast", "Player");

            if (Physics.Raycast(cameraRay, out RaycastHit hit, 1000f, mask))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = cameraRay.GetPoint(1000f);
            }

            // 3. Отдаем команду пушке
            Ctx.WeaponInventory.CurrentWeapon.TryFire(targetPoint);
        }
    }
}