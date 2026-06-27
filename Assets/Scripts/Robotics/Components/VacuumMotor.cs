using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using VacuumSim.Robotics.Contracts;
using Zenject;
using VacuumSim.Robotics.Configs;

namespace VacuumSim.Robotics.Components
{
    [RequireComponent(typeof(Rigidbody))]
    public class VacuumMotor : MonoBehaviour, IVacuumMotor
    {
        private Rigidbody _rb;
        private VacuumConfig _config;
        private float _currentSpeed;
        private bool _isMoving;
        public bool IsMoving => _isMoving;
        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;
        private float _speedMultiplier = 1.0f;

        [Inject]
        public void Construct(VacuumConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _rb.sleepThreshold = 0.0f;
        }

        public void MoveForward(float speed)
        {
            _currentSpeed = speed;
            _isMoving = true;
        }

        public void Stop()
        {
            _isMoving = false;
            _currentSpeed = 0f;

            if (_rb != null)
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                _rb.angularVelocity = Vector3.zero; 
            }
            
        }

        public async UniTask RotateAsync(float angleDelta, CancellationToken token)
        {
            float rotated = 0f;
            float direction = Mathf.Sign(angleDelta);
            float targetAbs = Mathf.Abs(angleDelta);

            while (rotated < targetAbs)
            {
                if (token.IsCancellationRequested) return;

                float step = _config.RotationSpeed * Time.fixedDeltaTime;
                
                if (rotated + step > targetAbs) 
                {
                    step = targetAbs - rotated; 
                }

                Quaternion deltaRotation = Quaternion.Euler(0, step * direction, 0);
                _rb.MoveRotation(_rb.rotation * deltaRotation);

                rotated += step;

                await UniTask.WaitForFixedUpdate(cancellationToken: token);
            }
            
            // Debug.Log($"[Motor] Поворот на {angleDelta} градусов завершен.");
        }

        public void SetSpeedMultiplier(float multiplier)
        {
            _speedMultiplier = Mathf.Max(0.1f, multiplier);
            Debug.Log($"[Motor] Множитель скорости изменен: {_speedMultiplier}");
        }

        private void FixedUpdate()
        {
            if (_isMoving)
            {
                Vector3 targetVelocity = transform.forward * (_currentSpeed * _speedMultiplier);
                _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
                // Debug.Log($"[Motor] Цель: {targetVelocity}. Факт RB: {_rb.velocity}");
            }
        }
    }
}