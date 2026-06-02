using UnityEngine;

namespace Gameplay.Towers.Behaviors.Aiming
{
    public class OmniAimStrategy : MonoBehaviour, IAimStrategy
    {
        [Header("Ограничения углов (Pitch)")]
        [Range(-90f, 0f)] [SerializeField] private float _minPitch = -10f; // Насколько низко может опустить ствол
        [Range(0f, 90f)]  [SerializeField] private float _maxPitch = 80f;  // Насколько высоко может поднять ствол

        public bool CanAimAt(Transform rotator, Transform target)
        {
            if (rotator == null || target == null) return false;

            Vector3 direction = target.position - rotator.position;
            float distanceXZ = new Vector2(direction.x, direction.z).magnitude;
            
            // Вычисляем угол наклона (отрицательный - смотрит вниз, положительный - смотрит вверх)
            float pitchAngle = Mathf.Atan2(direction.y, distanceXZ) * Mathf.Rad2Deg;

            // Цель валидна, только если угол попадает в заданный диапазон
            bool isAllowed = pitchAngle >= _minPitch && pitchAngle <= _maxPitch;

            // ЛОГ (Можно закомментировать после тестов)
            // Debug.Log($"<color=magenta>[OMNI TEST]</color> Зенитка: {gameObject.name} | Цель Y: {target.position.y:F2} | Угол: {pitchAngle:F1}° | Лимиты: [{_minPitch}°, {_maxPitch}°] | Разрешено: {isAllowed}");

            return isAllowed;
        }

        public void AimAtTarget(Transform rotator, Transform target, float turnSpeed)
        {
            // Здесь мы НЕ обнуляем Y. Ротатор крутится свободно во все 3 стороны
            Vector3 direction = target.position - rotator.position;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                rotator.rotation = Quaternion.Slerp(rotator.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        public bool IsFacingTarget(Transform rotator, Transform target)
        {
            // Сверяем векторы в полном 3D-пространстве
            Vector3 directionToTarget = (target.position - rotator.position).normalized;
            return Vector3.Dot(rotator.forward, directionToTarget) > 0.99f;
        }

        // Отрисовка ограничителей для зенитки
        public void DrawAimGizmo(Transform rotator, float range)
        {
            if (rotator == null) return;
            
            Gizmos.color = new Color(1f, 0f, 1f, 1f); // Пурпурный цвет для зенитки
            Vector3 forward = rotator.forward;
            Vector3 pos = rotator.position;
            
            // Рисуем конусы минимального и максимального угла (относительно земли, а не ствола!)
            // Для этого берем за основу глобальный вектор Forward, спроецированный на текущий поворот башни
            Vector3 flatForward = new Vector3(forward.x, 0, forward.z).normalized;
            if (flatForward == Vector3.zero) flatForward = rotator.up; // Защита от деления на ноль, если ствол смотрит ровно в зенит

            Vector3 upLimit = Quaternion.AngleAxis(-_maxPitch, rotator.right) * flatForward;
            Vector3 downLimit = Quaternion.AngleAxis(-_minPitch, rotator.right) * flatForward;
            
            Gizmos.DrawSphere(pos, 0.2f);
            
            // Рисуем лучи лимитов
            Gizmos.DrawRay(pos, upLimit * range);
            Gizmos.DrawRay(pos, downLimit * range);
            
            // Добавим полупрозрачную линию горизонта для ориентира
            Gizmos.color = new Color(1f, 1f, 1f, 0.3f);
            Gizmos.DrawRay(pos, flatForward * range);
        }
    }
}