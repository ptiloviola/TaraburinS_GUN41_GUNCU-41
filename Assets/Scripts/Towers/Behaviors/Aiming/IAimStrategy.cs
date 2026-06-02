using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public interface IAimStrategy
    {
        // Поворачивает ротатор к цели с заданной скоростью
        void AimAtTarget(Transform rotator, Transform target, float turnSpeed);
        
        // Возвращает true, если угол отклонения достаточно мал для выстрела
        bool IsFacingTarget(Transform rotator, Transform target);

        // НОВОЕ: Может ли эта стратегия физически навестись на эту цель?
        bool CanAimAt(Transform rotator, Transform target);

        // НОВОЕ: Каждая стратегия сама рисует свою форму радара!
        void DrawAimGizmo(Transform rotator, float range);
    }
}

