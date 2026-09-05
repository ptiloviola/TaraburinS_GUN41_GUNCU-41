using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class OmniAimStrategy : IAimStrategy
    {
        private readonly Transform _baseTransform; // Неподвижное основание башни (для расчета FOV)
        private readonly Transform _firePoint;     // Точка, откуда пускаем Raycast
        private readonly LayerMask _envMask;
        
        private readonly float _minPitch;
        private readonly float _maxPitch;
        private readonly float _fov;
        private readonly bool _checkLoS;

        public OmniAimStrategy(Transform baseTransform, Transform firePoint, LayerMask envMask, 
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
            
            // 1. Проверка горизонтального сектора (Yaw / FOV)
            if (_fov < 360f)
            {
                Vector3 flatDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);
                Vector3 flatForward = new Vector3(_baseTransform.forward.x, 0, _baseTransform.forward.z);
                
                if (Vector3.Angle(flatForward, flatDirection) > _fov * 0.5f) return false;
            }

            // 2. Проверка вертикального угла (Pitch)
            float distanceXZ = new Vector2(directionToTarget.x, directionToTarget.z).magnitude;
            float pitchAngle = Mathf.Atan2(directionToTarget.y, distanceXZ) * Mathf.Rad2Deg;
            
            if (pitchAngle < _minPitch || pitchAngle > _maxPitch) return false;

            // 3. Проверка препятствий (Line of Sight) — Самая тяжелая операция, делаем последней
            if (_checkLoS && _firePoint != null)
            {
                Vector3 rayDirection = target.position - _firePoint.position;
                // Пускаем луч. Если врезались в стену (Environment) — цель не видно
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
            if (rotator == null || _baseTransform == null) return;
            
            Vector3 pos = rotator.position;
            Vector3 baseForward = new Vector3(_baseTransform.forward.x, 0, _baseTransform.forward.z).normalized;
            if (baseForward == Vector3.zero) baseForward = _baseTransform.up; 
            
            // Отрисовка сектора FOV (Голубые линии)
            Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
            if (_fov < 360f)
            {
                Vector3 leftLimit = Quaternion.AngleAxis(-_fov * 0.5f, Vector3.up) * baseForward;
                Vector3 rightLimit = Quaternion.AngleAxis(_fov * 0.5f, Vector3.up) * baseForward;
                Gizmos.DrawRay(pos, leftLimit * range);
                Gizmos.DrawRay(pos, rightLimit * range);
            }

            // Отрисовка Pitch лимитов (Пурпурные линии)
            Gizmos.color = new Color(1f, 0f, 1f, 0.8f);
            // Рисуем наклон относительно направления ротатора (куда он сейчас смотрит)
            Vector3 flatRotatorForward = new Vector3(rotator.forward.x, 0, rotator.forward.z).normalized;
            if (flatRotatorForward != Vector3.zero)
            {
                Vector3 upLimit = Quaternion.AngleAxis(-_maxPitch, rotator.right) * flatRotatorForward;
                Vector3 downLimit = Quaternion.AngleAxis(-_minPitch, rotator.right) * flatRotatorForward;
                
                Gizmos.DrawRay(pos, upLimit * range);
                Gizmos.DrawRay(pos, downLimit * range);
            }
            
            // Если включен LoS, рисуем желтый луч из дула, чтобы видеть, откуда идет проверка
            if (_checkLoS && _firePoint != null)
            {
                Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
                Gizmos.DrawRay(_firePoint.position, rotator.forward * (range * 0.5f));
            }
        }
    }
}