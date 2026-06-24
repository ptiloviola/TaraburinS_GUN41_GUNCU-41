using System;
using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Signals;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;

namespace VacuumSim.UI
{
    // Не наследует MonoBehaviour! 
    public class VacuumDashboardPresenter : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly VacuumDashboardView _view;
        private readonly IVacuumDustbin _dustbin;
        private readonly IVacuumBattery _battery;
        private readonly VacuumConfig _config;

        private int _currentScore;

        // Zenject внедряет все зависимости сюда, включая наш View со сцены
        public VacuumDashboardPresenter(
            SignalBus signalBus, 
            VacuumDashboardView view, 
            IVacuumDustbin dustbin,
            IVacuumBattery battery,
            VacuumConfig config)
        {
            _signalBus = signalBus;
            _view = view;
            _dustbin = dustbin;
            _battery = battery;
            _config = config;
        }

        public void Initialize()
        {
            // 1. Подписываемся на Данные от систем пылесоса (Модели)
            _signalBus.Subscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Subscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);

            // 2. Подписываемся на клики игрока из интерфейса (View)
            _view.OnReturnToBaseClicked += HandleReturnToBase;
            _view.OnEmptyBinClicked += HandleEmptyBin;

            // Задаем стартовое значение очков
            _view.UpdateScore(_currentScore);
            _view.UpdateDustbin(_dustbin.CurrentFill, _config.MaxDustbinCapacity);
            _view.UpdateBattery(_battery.CurrentCharge / _config.MaxBattery);
        }

        // --- РЕАКЦИИ НА ДАННЫЕ РОБОТА ---
        private void OnBatteryChanged(BatteryStateSignal signal)
        {
            _view.UpdateBattery(signal.Percentage);
        }

        private void OnDustbinChanged(DustbinStateSignal signal)
        {
            _view.UpdateDustbin(signal.CurrentFill, signal.MaxCapacity);
        }

        private void OnTrashCollected(TrashCollectedSignal signal)
        {
            // Берем очки прямо из типа мусора! (Убедись, что в TrashType есть поле Points)
            // Если поля Points нет, можно пока сделать просто _currentScore += 10;
            _currentScore += signal.TrashData.Points; 
            _view.UpdateScore(_currentScore);
        }

        // --- РЕАКЦИИ НА КЛИКИ ИГРОКА ---
        private void HandleReturnToBase()
        {
            Debug.Log("[UI] Вызвана команда возврата на базу! (Здесь будет переключение стейт-машины)");
            // В будущем мы дернем интерфейс Мозга или выкинем сигнал ReturnToBaseSignal
        }

        private void HandleEmptyBin()
        {
            Debug.Log("[UI] Вызвана очистка бака!");
            // Дирижер напрямую дергает логику бака, потому что у него есть ссылка на интерфейс
            _dustbin.EmptyBin();
        }

        public void Dispose()
        {
            // Отписка от сигналов шины
            _signalBus.Unsubscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Unsubscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Unsubscribe<TrashCollectedSignal>(OnTrashCollected);

            // Отписка от событий View
            _view.OnReturnToBaseClicked -= HandleReturnToBase;
            _view.OnEmptyBinClicked -= HandleEmptyBin;
        }
    }
}