using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class OmniAimStrategy : IAimStrategy
    {
        private readonly float _minPitch;
        private readonly float _maxPitch;

        public OmniAimStrategy(float minPitch = -10f, float maxPitch = 80f)
        {
            _minPitch = minPitch;
            _maxPitch = maxPitch;
        }

        public bool CanAimAt(Transform rotator, Transform target)
        {
            if (rotator == null || target == null) return false;

            Vector3 direction = target.position - rotator.position;
            float distanceXZ = new Vector2(direction.x, direction.z).magnitude;
            
            float pitchAngle = Mathf.Atan2(direction.y, distanceXZ) * Mathf.Rad2Deg;

            return pitchAngle >= _minPitch && pitchAngle <= _maxPitch;
        }

        public void AimAtTarget(Transform rotator, Transform target, float turnSpeed)
        {
            Vector3 direction = target.position - rotator.position;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rotator.rotation = Quaternion.Slerp(rotator.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        public bool IsFacingTarget(Transform rotator, Transform target)
        {
            Vector3 directionToTarget = (target.position - rotator.position).normalized;
            return Vector3.Dot(rotator.forward, directionToTarget) > 0.99f;
        }

        public void DrawAimGizmo(Transform rotator, float range)
        {
            if (rotator == null) return;
            
            Gizmos.color = new Color(1f, 0f, 1f, 1f); 
            Vector3 forward = rotator.forward;
            Vector3 pos = rotator.position;
            
            Vector3 flatForward = new Vector3(forward.x, 0, forward.z).normalized;
            if (flatForward == Vector3.zero) flatForward = rotator.up; 

            Vector3 upLimit = Quaternion.AngleAxis(-_maxPitch, rotator.right) * flatForward;
            Vector3 downLimit = Quaternion.AngleAxis(-_minPitch, rotator.right) * flatForward;
            
            Gizmos.DrawSphere(pos, 0.2f);
            
            Gizmos.DrawRay(pos, upLimit * range);
            Gizmos.DrawRay(pos, downLimit * range);
            
            Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
            Gizmos.DrawRay(pos, flatForward * range);
        }
    }
}