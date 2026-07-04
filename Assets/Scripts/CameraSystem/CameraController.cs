using UnityEngine;

namespace MeatMushrooms.CameraSystem
{
    public class CameraController : MonoBehaviour
    {
        public CameraConfig Config;
        private Transform _target;

        // Состояния камеры для будущих кинематографичных эффектов
        private enum CameraState { Idle, Follow, DeathCinematic, VictoryCinematic }
        private CameraState _currentState = CameraState.Idle;

        // Метод, который мы вызовем при спавне Шапочки
        public void SetTarget(Transform target)
        {
            _target = target;
            _currentState = CameraState.Follow;

            // Мгновенно прыгаем за спину при старте, чтобы не было долгого перелета
            transform.position = _target.position + Config.FollowOffset;
            transform.rotation = Quaternion.Euler(Config.PitchAngle, 0f, 0f);
        }

        // ЗАДЕЛ НА БУДУЩЕЕ: Метод для вызова эффекта смерти
        public void TriggerDeathCamera(Transform wolf, Transform player)
        {
            _currentState = CameraState.DeathCinematic;
            Debug.Log("<color=cyan>[Camera]</color> Активирован режим Death Cam!");
            // Позже здесь напишем логику зума, слоу-мо и смены ракурса
        }

        // Камеру ВСЕГДА нужно двигать в LateUpdate, после того как отработала физика и анимации игрока
        private void LateUpdate()
        {
            if (_currentState != CameraState.Follow || _target == null) return;

            // Вычисляем идеальную точку, где должна быть камера
            Vector3 targetPosition = _target.position + Config.FollowOffset;
            
            // Плавно летим к этой точке (Vector3.Lerp)
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * Config.FollowSpeed);

            // Жестко фиксируем угол наклона (чтобы управление Шапочкой не сбивалось)
            transform.rotation = Quaternion.Euler(Config.PitchAngle, 0f, 0f);
        }
    }
}