using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public interface IAimStrategy
    {
        void AimAtTarget(Transform rotator, Transform target, float turnSpeed);
        
        bool IsFacingTarget(Transform rotator, Transform target);

        bool CanAimAt(Transform rotator, Transform target);

        void DrawAimGizmo(Transform rotator, float range);
    }
}

