using System;
using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Signals;

namespace VacuumSim.Robotics.Components
{
    public class VacuumDustbinManager : IVacuumDustbin, IInitializable, IDisposable
    {
        private readonly VacuumConfig _config;
        private readonly SignalBus _signalBus;

        private float _currentFill;

        public float CurrentFill => _currentFill;
        public bool IsFull => _currentFill >= _config.MaxDustbinCapacity;

        public VacuumDustbinManager(VacuumConfig config, SignalBus signalBus)
        {
            _config = config;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _currentFill = 0f;
            
            // Подписываемся на тот же самый сигнал, что и Батарея!
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);
            
            FireStateSignal();
        }

        private void OnTrashCollected(TrashCollectedSignal signal)
        {
            if (IsFull) return;

            // Пока считаем, что любая соринка занимает 1 единицу объема.
            // Позже можно будет брать объем прямо из signal.TrashData, если добавим туда такое поле.
            _currentFill += 1f;

            FireStateSignal();

            if (IsFull)
            {
                Debug.LogWarning("[Dustbin] ПЫЛЕСБОРНИК ПЕРЕПОЛНЕН! РОБОТ НЕ МОЖЕТ БОЛЬШЕ ВСАСЫВАТЬ");
            }
        }

        public void EmptyBin()
        {
            _currentFill = 0f;
            FireStateSignal();
            Debug.Log("[Dustbin] Пылесборник успешно очищен.");
        }

        private void FireStateSignal()
        {
            _signalBus.Fire(new DustbinStateSignal
            {
                CurrentFill = _currentFill,
                MaxCapacity = _config.MaxDustbinCapacity
            });
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<TrashCollectedSignal>(OnTrashCollected);
        }

    }
}


