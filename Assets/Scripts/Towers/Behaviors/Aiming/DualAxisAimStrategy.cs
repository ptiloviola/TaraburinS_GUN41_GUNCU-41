using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class DualAxisAimStrategy : IAimStrategy
    {
        private readonly Transform _baseTransform;
        private readonly Transform _elevationPivot;
        private readonly Transform _firePoint;
        private readonly LayerMask _envMask;
        private readonly float _minPitch, _maxPitch, _fov;
        private readonly bool _checkLoS;

        public DualAxisAimStrategy(Transform baseTransform, Transform elevationPivot, Transform firePoint, LayerMask envMask, float minPitch, float maxPitch, float fov, bool checkLoS)
        {
            _baseTransform = baseTransform; _elevationPivot = elevationPivot;
            _firePoint = firePoint; _envMask = envMask;
            _minPitch = minPitch; _maxPitch = maxPitch; _fov = fov; _checkLoS = checkLoS;
        }

        public bool CanAimAt(Transform rotator, Transform target)
        {
            if (rotator == null || target == null) return false;
            Vector3 directionToTarget = target.position - rotator.position;
            
            if (_fov < 360f)
            {
                Vector3 flatDir = new Vector3(directionToTarget.x, 0, directionToTarget.z);
                Vector3 flatFwd = new Vector3(_baseTransform.forward.x, 0, _baseTransform.forward.z);
                if (Vector3.Angle(flatFwd, flatDir) > _fov * 0.5f) return false;
            }

            float distanceXZ = new Vector2(directionToTarget.x, directionToTarget.z).magnitude;
            float pitchAngle = Mathf.Atan2(directionToTarget.y, distanceXZ) * Mathf.Rad2Deg;
            if (pitchAngle < _minPitch || pitchAngle > _maxPitch) return false;

            if (_checkLoS && _firePoint != null)
            {
                Vector3 rayDir = target.position - _firePoint.position;
                if (Physics.Raycast(_firePoint.position, rayDir, rayDir.magnitude, _envMask)) return false;
            }
            return true;
        }

        public void AimAtTarget(Transform rotator, Transform target, float turnSpeed)
        {
            if (target == null) return;
            Vector3 direction = target.position - rotator.position;
            direction.y = 0f; 

            if (direction != Vector3.zero)
                rotator.rotation = Quaternion.Slerp(rotator.rotation, Quaternion.LookRotation(direction), turnSpeed * Time.deltaTime);

            if (_elevationPivot != null)
            {
                Vector3 localTarget = rotator.InverseTransformPoint(target.position);
                float distanceXZ = new Vector2(localTarget.x, localTarget.z).magnitude;
                float targetPitchAngle = Mathf.Clamp(Mathf.Atan2(localTarget.y, distanceXZ) * Mathf.Rad2Deg, _minPitch, _maxPitch);
                _elevationPivot.localRotation = Quaternion.Slerp(_elevationPivot.localRotation, Quaternion.Euler(-targetPitchAngle, 0, 0), turnSpeed * Time.deltaTime);
            }
        }

        public bool IsFacingTarget(Transform rotator, Transform target)
        {
            if (target == null) return false;
            Vector3 dirFlat = (target.position - rotator.position).normalized;
            dirFlat.y = 0f;
            bool isYawAligned = dirFlat == Vector3.zero || Vector3.Dot(rotator.forward, dirFlat.normalized) > 0.99f;
            bool isPitchAligned = _elevationPivot == null || Vector3.Dot(_elevationPivot.forward, (target.position - _elevationPivot.position).normalized) > 0.98f;
            return isYawAligned && isPitchAligned;
        }

        public void DrawAimGizmo(Transform rotator, float range) { }
    }
}