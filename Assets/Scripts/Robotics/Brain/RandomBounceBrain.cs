using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;

namespace VacuumSim.Robotics.Brain
{
    public class RandomBounceBrain : IVacuumBrain
    {
        private readonly IVacuumMotor _motor;
        private readonly IVacuumSensors _sensors;
        private readonly VacuumConfig _config;

        // Конструкторная инъекция (Zenject передаст реализации сюда)
        public RandomBounceBrain(IVacuumMotor motor, IVacuumSensors sensors, VacuumConfig config)
        {
            _motor = motor;
            _sensors = sensors;
            _config = config;
        }

        public async UniTask StartCleaningAsync(CancellationToken token)
        {
            // Бесконечный цикл работы, пока не сработает токен отмены
            while (!token.IsCancellationRequested)
            {
                _motor.MoveForward(_config.MoveSpeed);

                // Асинхронно ждем (каждый кадр), пока сенсор не увидит препятствие впереди.
                // Передаем токен, чтобы ожидание тоже можно было прервать.
                await UniTask.WaitUntil(() => _sensors.IsObstacleAhead(), cancellationToken: token);

                // Увидели стену -> останавливаемся
                _motor.Stop();

                // Небольшая пауза "на подумать" (имитация работы ИИ)
                await UniTask.Delay(300, cancellationToken: token);

                // Выбираем направление для разворота на основе боковых сенсоров
                float turnAngle = CalculateTurnAngle();
                // ВАЖНО: Вызываем новый асинхронный метод и ждем его полного завершения!
                await _motor.RotateAsync(turnAngle, token);

                // Небольшая пауза после разворота, чтобы пылесос не дергался
                await UniTask.Delay(200, cancellationToken: token);
            }
        }

        private float CalculateTurnAngle()
        {
            // Если справа стена, разворачиваемся влево, и наоборот
            if (_sensors.IsObstacleRight() && !_sensors.IsObstacleLeft())
            {
                return Random.Range(-90f, -135f); // Поворот налево
            }
            if (_sensors.IsObstacleLeft() && !_sensors.IsObstacleRight())
            {
                return Random.Range(90f, 135f); // Поворот направо
            }
            
            // Если тупик или препятствий по бокам нет - крутимся случайно в любую сторону
            return Random.value > 0.5f ? Random.Range(90f, 135f) : Random.Range(-90f, -135f);
        }
    }
}