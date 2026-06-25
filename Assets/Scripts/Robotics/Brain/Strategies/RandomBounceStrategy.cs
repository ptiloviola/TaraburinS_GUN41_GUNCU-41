using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;

namespace VacuumSim.Robotics.Brain.Strategies
{
    public class RandomBounceStrategy : ICleaningStrategy
    {
        private readonly IVacuumMotor _motor;
        private readonly IVacuumSensors _sensors;
        private readonly VacuumConfig _config;

        public RandomBounceStrategy(IVacuumMotor motor, IVacuumSensors sensors, VacuumConfig config)
        {
            _motor = motor;
            _sensors = sensors;
            _config = config;
        }

        public async UniTask ExecuteAsync(CancellationToken token)
        {
            // Этот цикл работает, пока внешняя система (Мозг) не отменит токен
            while (!token.IsCancellationRequested)
            {
                Debug.Log("[Strategy] Хаотичный режим: Еду прямо.");
                _motor.MoveForward(_config.MoveSpeed);

                await UniTask.WaitUntil(() => _sensors.IsObstacleAhead(), PlayerLoopTiming.FixedUpdate, token);
                
                if (token.IsCancellationRequested) break;

                _motor.Stop();
                await UniTask.Delay(300, cancellationToken: token);

                float turnAngle = CalculateTurnAngle();

                try
                {
                    await _motor.RotateAsync(turnAngle, token).Timeout(System.TimeSpan.FromSeconds(2));
                }
                catch (System.TimeoutException)
                {
                    Debug.LogWarning("[Strategy] Робот застрял при развороте! Сброс шага.");
                }

                await UniTask.Delay(200, cancellationToken: token);
            }
        }

        private float CalculateTurnAngle()
        {
            if (_sensors.IsObstacleRight() && !_sensors.IsObstacleLeft()) return Random.Range(-90f, -135f);
            if (_sensors.IsObstacleLeft() && !_sensors.IsObstacleRight()) return Random.Range(90f, 135f);
            return Random.value > 0.5f ? Random.Range(90f, 135f) : Random.Range(-90f, -135f);
        }
    }
}