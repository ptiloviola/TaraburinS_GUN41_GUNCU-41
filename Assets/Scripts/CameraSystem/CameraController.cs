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

        public void SetTarget(Transform target)
        {
            _target = target;
            _currentState = CameraState.Follow;

            transform.position = _target.position + Config.FollowOffset;
            transform.rotation = Quaternion.Euler(Config.PitchAngle, 0f, 0f);
        }

        public void TriggerDeathCamera(Transform wolf, Transform player)
        {
            _currentState = CameraState.DeathCinematic;
            Debug.Log("<color=cyan>[Camera]</color> Активирован режим Death Cam!");
        }

        private void LateUpdate()
        {
            if (_currentState != CameraState.Follow || _target == null) return;
            Vector3 targetPosition = _target.position + Config.FollowOffset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * Config.FollowSpeed);
            transform.rotation = Quaternion.Euler(Config.PitchAngle, 0f, 0f);
        }
    }
}