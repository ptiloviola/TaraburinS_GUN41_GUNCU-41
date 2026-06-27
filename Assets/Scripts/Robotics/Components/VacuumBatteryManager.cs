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
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);
            FireStateSignal();
        }

        public void Tick()
        {
            if (IsEmpty) return;
            float drainThisFrame = _config.IdleDrainRate * Time.deltaTime;

            if (_motor.IsMoving)
            {
                drainThisFrame += _config.MoveDrainRate * Time.deltaTime;
            }

            drainThisFrame *= _loadMultiplier;

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

            if (Mathf.Abs(previousCharge - _currentCharge) > 0.01f)
            {
                FireStateSignal();
            }
            if (IsEmpty && previousCharge > 0)
            {
                Debug.LogWarning("[Battery] БАТАРЕЯ РАЗРЯЖЕНА! РОБОТ ОТКЛЮЧАЕТСЯ.");
            }

        }

        private void FireStateSignal()
        {
            _signalBus.Fire(new BatteryStateSignal
            {
                CurrentCharge = _currentCharge,
                MaxCharge = _config.MaxBattery,
                IsLow = this.IsLow 
            });
        }

        public void SetLoadMultiplier(float multiplier)
        {
            _loadMultiplier = Mathf.Max(0.1f, multiplier);
            Debug.Log($"[Battery] Нагрузка изменена. Новый множитель: {_loadMultiplier}");
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<TrashCollectedSignal>(OnTrashCollected);
        }

        public void Charge(float amount)
        {
            float previousCharge = _currentCharge;
            _currentCharge = Mathf.Clamp(_currentCharge + amount, 0, _config.MaxBattery);

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


