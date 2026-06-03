using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class HorizontalAimStrategy : MonoBehaviour, IAimStrategy
    {
        [Header("Ограничения прицела")]
        [SerializeField] private float _verticalTolerance = 15f; 

        public bool CanAimAt(Transform rotator, Transform target)
        {
            if (rotator == null || target == null) return false;

            // Считаем математику
            Vector3 direction = target.position - rotator.position;
            float distanceXZ = new Vector2(direction.x, direction.z).magnitude;
            float pitchAngle = Mathf.Atan2(direction.y, distanceXZ) * Mathf.Rad2Deg;

            // Проверяем допуск
            bool isAllowed = Mathf.Abs(pitchAngle) <= _verticalTolerance;

            // === ЖЕСТКИЙ ЛОГ ===
            //Debug.Log($"<color=orange>[MATH TEST]</color> Башня: {gameObject.name} | Цель: {target.name} | Y цели: {target.position.y:F2} | Y ствола: {rotator.position.y:F2} | Угол: {pitchAngle:F1}° | Допуск: {_verticalTolerance}° | Разрешено: {isAllowed}");

            return isAllowed;
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
            
            // Делаем лучи непрозрачными и очень яркими
            Gizmos.color = new Color(0f, 1f, 1f, 1f); 
            Vector3 forward = rotator.forward;
            Vector3 pos = rotator.position;
            
            Vector3 upLimit = Quaternion.AngleAxis(-_verticalTolerance, rotator.right) * forward;
            Vector3 downLimit = Quaternion.AngleAxis(_verticalTolerance, rotator.right) * forward;
            
            // Рисуем маленькую сферу в точке отсчета
            Gizmos.DrawSphere(pos, 0.2f);

            // Рисуем лучи
            Gizmos.DrawRay(pos, upLimit * range);
            Gizmos.DrawRay(pos, downLimit * range);
        }
    }
}