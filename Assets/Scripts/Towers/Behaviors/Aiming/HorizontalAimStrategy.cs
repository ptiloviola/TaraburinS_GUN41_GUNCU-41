using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class HorizontalAimStrategy : IAimStrategy
    {
        private readonly Transform _baseTransform;
        private readonly Transform _firePoint;
        private readonly LayerMask _envMask;
        
        private readonly float _minPitch;
        private readonly float _maxPitch;
        private readonly float _fov;
        private readonly bool _checkLoS;

        public HorizontalAimStrategy(Transform baseTransform, Transform firePoint, LayerMask envMask, 
                                     float minPitch, float maxPitch, float fov, bool checkLoS)
        {
            _baseTransform = baseTransform;
            _firePoint = firePoint;
            _envMask = envMask;
            _minPitch = minPitch;
            _maxPitch = maxPitch;
            _fov = fov;
            _checkLoS = checkLoS;
        }

        public bool CanAimAt(Transform rotator, Transform target)
        {
            if (rotator == null || target == null) return false;

            Vector3 directionToTarget = target.position - rotator.position;
            
            if (_fov < 360f)
            {
                Vector3 flatDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);
                Vector3 flatForward = new Vector3(_baseTransform.forward.x, 0, _baseTransform.forward.z);
                
                if (Vector3.Angle(flatForward, flatDirection) > _fov * 0.5f) return false;
            }

            float distanceXZ = new Vector2(directionToTarget.x, directionToTarget.z).magnitude;
            float pitchAngle = Mathf.Atan2(directionToTarget.y, distanceXZ) * Mathf.Rad2Deg;
            
            if (pitchAngle < _minPitch || pitchAngle > _maxPitch) return false;

            if (_checkLoS && _firePoint != null)
            {
                Vector3 rayDirection = target.position - _firePoint.position;
                if (Physics.Raycast(_firePoint.position, rayDirection, rayDirection.magnitude, _envMask))
                {
                    return false;
                }
            }

            return true;
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
            if (rotator == null || _baseTransform == null) return;
            
            Vector3 pos = rotator.position;
            Vector3 baseForward = new Vector3(_baseTransform.forward.x, 0, _baseTransform.forward.z).normalized;
            if (baseForward == Vector3.zero) baseForward = _baseTransform.up; 
            
            // 1. Отрисовка сектора FOV
            if (_fov < 360f)
            {
                Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
                Vector3 leftLimit = Quaternion.AngleAxis(-_fov * 0.5f, Vector3.up) * baseForward;
                Vector3 rightLimit = Quaternion.AngleAxis(_fov * 0.5f, Vector3.up) * baseForward;
                Gizmos.DrawRay(pos, leftLimit * range);
                Gizmos.DrawRay(pos, rightLimit * range);
            }

#if UNITY_EDITOR
            // 2. Чистый 3D-конус прицеливания
            DrawCleanRadarCone(pos, range, _minPitch, _maxPitch);
#endif

            // 3. Проверка LoS
            if (_checkLoS && _firePoint != null)
            {
                Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
                Gizmos.DrawRay(_firePoint.position, rotator.forward * (range * 0.5f));
            }
        }

#if UNITY_EDITOR
        private void DrawCleanRadarCone(Vector3 center, float range, float minPitch, float maxPitch)
        {
            // Бледная сфера, показывающая максимальную границу дистанции (Range)
            Gizmos.color = new Color(1f, 1f, 1f, 0.05f);
            Gizmos.DrawWireSphere(center, range);

            // Верхнее кольцо (Зеленое) - максимальная высота
            float maxRad = maxPitch * Mathf.Deg2Rad;
            Vector3 maxCenter = center + Vector3.up * (Mathf.Sin(maxRad) * range);
            float maxRadius = Mathf.Cos(maxRad) * range;

            UnityEditor.Handles.color = new Color(0f, 1f, 0f, 0.8f);
            UnityEditor.Handles.DrawWireDisc(maxCenter, Vector3.up, maxRadius);

            // Нижнее кольцо (Красное) - минимальная высота
            float minRad = minPitch * Mathf.Deg2Rad;
            Vector3 minCenter = center + Vector3.up * (Mathf.Sin(minRad) * range);
            float minRadius = Mathf.Cos(minRad) * range;

            UnityEditor.Handles.color = new Color(1f, 0f, 0f, 0.8f);
            UnityEditor.Handles.DrawWireDisc(minCenter, Vector3.up, minRadius);

            // 4 тонкие направляющие линии для формирования каркаса конуса
            UnityEditor.Handles.color = new Color(1f, 1f, 0f, 0.2f);
            Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
            
            foreach (var dir in directions)
            {
                UnityEditor.Handles.DrawLine(center, maxCenter + dir * maxRadius);
                UnityEditor.Handles.DrawLine(center, minCenter + dir * minRadius);
            }
        }
#endif
    }
}