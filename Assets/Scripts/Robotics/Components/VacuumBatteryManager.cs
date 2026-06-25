using System;
using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Signals;

namespace VacuumSim.Robotics.Components
{
    public class VacuumBatteryManager : IVacuumBattery, IInitializable, ITickable, IDisposable
    {
        private readonly VacuumConfig _config;
        private readonly IVacuumMotor _motor;
        private SignalBus _signalBus;

        private float _currentCharge;
        private float _loadMultiplier = 1.0f;

        public float CurrentCharge => _currentCharge;
        public bool IsEmpty => _currentCharge <= 0;
        public bool IsFull => _currentCharge >= _config.MaxBattery;
        public bool IsLow => (_currentCharge / _config.MaxBattery) <= 0.20f;

        public VacuumBatteryManager(VacuumConfig config, IVacuumMotor motor, 
            SignalBus signalBus)
        {
            _config = config;
            _motor = motor;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _currentCharge = _config.MaxBattery;
            // Подписываемся на сигнал всасывания мусора.
            // Как только кто-то крикнет "мусор собран", выполнится метод OnTrashCollected
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);
            // Отправляем стартовое состояние для UI
            FireStateSignal();
        }

        // Вызывается Zenject-ом каждый кадр (аналог Update)
        public void Tick()
        {
            if (IsEmpty) return;
            // 1. Считаем базовый расход (просто включен)
            float drainThisFrame = _config.IdleDrainRate * Time.deltaTime;

            // 2. Если колеса крутятся, добавляем расход на движение
            if (_motor.IsMoving)
            {
                drainThisFrame += _config.MoveDrainRate * Time.deltaTime;
            }

            // 3. Применяем внешнюю нагрузку (кота)
            drainThisFrame *= _loadMultiplier;

            // 4. Списываем заряд
            Drain(drainThisFrame);

        }

        private void OnTrashCollected(TrashCollectedSignal signal)
        {
            if (IsEmpty) return;
            Drain(_config.SuctionDrainCost * _loadMultiplier);
        }

        private void Drain(float amount)
        {
            float previousCharge = _currentCharge;
            _currentCharge = Mathf.Clamp(_currentCharge - amount, 0, _config.MaxBattery);
            // Чтобы не спамить шину миллионами сигналов, отправляем обновление, 
            // только если заряд реально изменился
            if (Mathf.Abs(previousCharge - _currentCharge) > 0.01f)
            {
                FireStateSignal();
            }
            if (IsEmpty && previousCharge > 0)
            {
                Debug.LogWarning("[Battery] БАТАРЕЯ РАЗРЯЖЕНА! РОБОТ ОТКЛЮЧАЕТСЯ.");
                // Позже Мозг будет слушать этот сигнал и останавливать мотор
            }

        }

        private void FireStateSignal()
        {
            _signalBus.Fire(new BatteryStateSignal
            {
                CurrentCharge = _currentCharge,
                MaxCharge = _config.MaxBattery,
                IsLow = this.IsLow // Передаем состояние
            });
        }

        // Реализация контракта для кота
        public void SetLoadMultiplier(float multiplier)
        {
            _loadMultiplier = Mathf.Max(0.1f, multiplier);
            Debug.Log($"[Battery] Нагрузка изменена. Новый множитель: {_loadMultiplier}");
        }

        // Обязательная очистка памяти при выходе (иначе будет утечка)
        public void Dispose()
        {
            _signalBus.Unsubscribe<TrashCollectedSignal>(OnTrashCollected);
        }

        public void Charge(float amount)
        {
            float previousCharge = _currentCharge;
            _currentCharge = Mathf.Clamp(_currentCharge + amount, 0, _config.MaxBattery);

            // Если раньше мы были пусты, а теперь что-то появилось - сообщаем всем, что мы живы
            if (previousCharge <= 0 && _currentCharge > 0)
            {
                Debug.Log("[Battery] Батарея ожила!");
            }

            if (Mathf.Abs(previousCharge - _currentCharge) > 0.01f)
            {
                FireStateSignal();
            }
        }

    }
}


