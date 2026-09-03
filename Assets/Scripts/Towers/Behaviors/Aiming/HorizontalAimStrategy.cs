using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class HorizontalAimStrategy : IAimStrategy
    {
        private readonly float _verticalTolerance;

        public HorizontalAimStrategy(float verticalTolerance = 15f)
        {
            _verticalTolerance = verticalTolerance;
        }

        public bool CanAimAt(Transform rotator, Transform target)
        {
            if (rotator == null || target == null) return false;

            Vector3 direction = target.position - rotator.position;
            float distanceXZ = new Vector2(direction.x, direction.z).magnitude;
            float pitchAngle = Mathf.Atan2(direction.y, distanceXZ) * Mathf.Rad2Deg;

            return Mathf.Abs(pitchAngle) <= _verticalTolerance;
        }

        public void AimAtTarget(Transform rotator, Transform target, float turnSpeed)
        {
            Vector3 direction = target.position - rotator.position;
            direction.y = 0f; 

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rotator.rotation = Quaternion.Slerp(rotator.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        public bool IsFacingTarget(Transform rotator, Transform target)
        {
            Vector3 directionToTarget = (target.position - rotator.position).normalized;
            directionToTarget.y = 0f; 
            
            if (directionToTarget == Vector3.zero) return true; 

            return Vector3.Dot(rotator.forward, directionToTarget.normalized) > 0.99f;
        }

        public void DrawAimGizmo(Transform rotator, float range)
        {
            if (rotator == null) return;
            
            Gizmos.color = new Color(0f, 1f, 1f, 1f); 
            Vector3 forward = rotator.forward;
            Vector3 pos = rotator.position;
            
            Vector3 upLimit = Quaternion.AngleAxis(-_verticalTolerance, rotator.right) * forward;
            Vector3 downLimit = Quaternion.AngleAxis(_verticalTolerance, rotator.right) * forward;
            
            Gizmos.DrawSphere(pos, 0.2f);
            Gizmos.DrawRay(pos, upLimit * range);
            Gizmos.DrawRay(pos, downLimit * range);
        }
    }
}