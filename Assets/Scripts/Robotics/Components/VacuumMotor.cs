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

        [Inject]
        public void Construct(VacuumConfig config)
        {
            _config = config;
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            // Запрещаем физическому движку усыплять этого робота!
            _rb.sleepThreshold = 0.0f;
        }

        public void MoveForward(float speed)
        {
            _currentSpeed = speed;
            _isMoving = true;
            Debug.Log($"[Motor] Двигатель запущен! Скорость: {speed}");
        }

        public void Stop()
        {
            _isMoving = false;
            _currentSpeed = 0f;
            _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
            // Сбрасываем угловую скорость, чтобы пылесос не крутился по инерции после столкновений
            _rb.angularVelocity = Vector3.zero; 
        }

        public async UniTask RotateAsync(float angleDelta, CancellationToken token)
        {
            float rotated = 0f;
            float direction = Mathf.Sign(angleDelta); // 1 (вправо) или -1 (влево)
            float targetAbs = Mathf.Abs(angleDelta);

            // Пока мы не повернулись на нужный угол...
            while (rotated < targetAbs)
            {
                // Если игру остановили или робот сломался - немедленно прерываем цикл
                if (token.IsCancellationRequested) return;

                // Считаем, на сколько градусов мы должны повернуться в этот кадр
                float step = _config.RotationSpeed * Time.fixedDeltaTime;
                
                // Защита от "перелета" (чтобы не повернуться на 92 градуса вместо 90)
                if (rotated + step > targetAbs) 
                {
                    step = targetAbs - rotated; 
                }

                // Вращаем Rigidbody
                Quaternion deltaRotation = Quaternion.Euler(0, step * direction, 0);
                _rb.MoveRotation(_rb.rotation * deltaRotation);

                rotated += step;

                // ВАЖНО: Ждем следующего ФИЗИЧЕСКОГО кадра, чтобы продолжить цикл
                await UniTask.WaitForFixedUpdate(cancellationToken: token);
            }
            
            Debug.Log($"[Motor] Поворот на {angleDelta} градусов завершен.");
        }

        private void FixedUpdate()
        {
            if (_isMoving)
            {
                // Постоянно поддерживаем скорость каждый физический кадр, преодолевая трение.
                // Сохраняем текущую скорость по оси Y (гравитацию), чтобы робот не летал.
                Vector3 targetVelocity = transform.forward * _currentSpeed;
                _rb.velocity = new Vector3(targetVelocity.x, _rb.velocity.y, targetVelocity.z);
                // Добавляем проверку пульса
                // Debug.Log($"[Motor] Цель: {targetVelocity}. Факт RB: {_rb.velocity}");
            }
        }
    }
}