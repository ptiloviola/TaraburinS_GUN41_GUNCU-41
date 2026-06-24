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

        private readonly IVacuumBattery _battery;
        private readonly IVacuumDustbin _dustbin;

        // Конструкторная инъекция (Zenject передаст реализации сюда)
        public RandomBounceBrain(IVacuumMotor motor, IVacuumSensors sensors, 
            VacuumConfig config, IVacuumBattery battery, IVacuumDustbin dustbin)
        {
            _motor = motor;
            _sensors = sensors;
            _config = config;
            _battery = battery;
            _dustbin = dustbin;
        }

        public async UniTask StartCleaningAsync(CancellationToken token)
        {
            // Бесконечный цикл работы, пока не сработает токен отмены, батарея жива и бак не полон!
            while (!token.IsCancellationRequested && !_battery.IsEmpty && !_dustbin.IsFull)
            {
                Debug.Log("[Brain] Путь свободен. Еду прямо.");
                _motor.MoveForward(_config.MoveSpeed);

                // Асинхронно ждем (каждый кадр), пока сенсор не увидит препятствие впереди.
                // Передаем токен, чтобы ожидание тоже можно было прервать.
                // ИЗМЕНЕНИЕ: Добавлен PlayerLoopTiming.FixedUpdate. 
                // Теперь мозг опрашивает лучи строго синхронно с физикой!
                await UniTask.WaitUntil(() => _sensors.IsObstacleAhead(), PlayerLoopTiming.FixedUpdate, token);
                // Если во время движения по прямой села батарея - прерываем логику
                if (_battery.IsEmpty || _dustbin.IsFull) break;
                // Увидели стену -> останавливаемся
                _motor.Stop();

                // Небольшая пауза "на подумать" (имитация работы ИИ)
                await UniTask.Delay(300, cancellationToken: token);

                // Выбираем направление для разворота на основе боковых сенсоров
                float turnAngle = CalculateTurnAngle();

                // ЗАЩИТА ОТ ЗАВИСАНИЯ: Если мотор упрется в стену и не сможет повернуться,
                // UniTask принудительно прервет этот поворот через 2 секунды (Timeout),
                // и цикл начнется заново, спасая робота от вечного зависания.
                try
                {
                    await _motor.RotateAsync(turnAngle, token).Timeout(System.TimeSpan.FromSeconds(2));
                }
                catch (System.TimeoutException)
                {
                    Debug.LogWarning("[Brain] Мотор застрял при повороте! Сбрасываем цикл.");
                }

                // Небольшая пауза после разворота, чтобы пылесос не дергался
                await UniTask.Delay(200, cancellationToken: token);
            }
            // Если мы выпали из цикла while, значит наступило критическое состояние (или мы вышли из игры)
            _motor.Stop();
            Debug.Log($"[Brain] Уборка остановлена. Батарея: {_battery.CurrentCharge:F1}, Бак: {_dustbin.CurrentFill}");
        }

        private float CalculateTurnAngle()
        {
            // Если справа стена, разворачиваемся влево, и наоборот
            if (_sensors.IsObstacleRight() && !_sensors.IsObstacleLeft())
            {
                Debug.Log("[Brain] Вижу препятствие! Начинаю поворот.");
                return Random.Range(-90f, -135f); // Поворот налево
            }
            if (_sensors.IsObstacleLeft() && !_sensors.IsObstacleRight())
            {
                Debug.Log("[Brain] Вижу препятствие! Начинаю поворот.");
                return Random.Range(90f, 135f); // Поворот направо
            }
            
            // Если тупик или препятствий по бокам нет - крутимся случайно в любую сторону
            return Random.value > 0.5f ? Random.Range(90f, 135f) : Random.Range(-90f, -135f);
        }

    }

    
}