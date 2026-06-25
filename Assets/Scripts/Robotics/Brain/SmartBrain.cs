using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Signals;
using VacuumSim.Robotics.Brain.States;
using VacuumSim.Robotics.Components;

namespace VacuumSim.Robotics.Brain
{
    public class SmartBrain : IVacuumBrain, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IVacuumMotor _motor;
        
        // Ссылки на доступные состояния
        private readonly CleaningState _cleaningState;
        private readonly ReturnToBaseState _returnToBaseState;

        private IVacuumState _currentState;
        private CancellationTokenSource _stateCts;
        private readonly DockedState _dockedState;
        private readonly VacuumCollector _collector;

        public SmartBrain(
            SignalBus signalBus, 
            IVacuumMotor motor,
            CleaningState cleaningState,
            ReturnToBaseState returnToBaseState,
            DockedState dockedState,
            VacuumCollector collector)
        {
            _signalBus = signalBus;
            _motor = motor;
            _cleaningState = cleaningState;
            _returnToBaseState = returnToBaseState;
            _dockedState = dockedState;
            _collector = collector;
        }

        public void Initialize()
        {
            // Мозг подписывается на сигналы критических изменений
            _signalBus.Subscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Subscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Subscribe<ArrivedAtBaseSignal>(OnArrivedAtBase);
        }

        public async UniTask StartCleaningAsync(CancellationToken token)
        {
            // Стартуем с режима уборки
            ChangeState(_cleaningState);

            // Держим мозг активным, пока работает вся симуляция
            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            StopCurrentState();
        }

        public void ChangeState(IVacuumState newState)
        {
            if (_currentState == newState) return;

            StopCurrentState();

            _currentState = newState;
            _stateCts = new CancellationTokenSource();

            Debug.Log($"[SmartBrain] Смена режима на: {newState.GetType().Name}");

            // Включаем коллектор всегда, КРОМЕ состояния парковки.
            // Таким образом, по пути на базу он будет собирать мусор, а на зарядке - нет!
            _collector.enabled = (newState != _dockedState);

            _currentState.ExecuteAsync(_stateCts.Token).Forget();
        }

        private void StopCurrentState()
        {
            if (_stateCts != null)
            {
                _stateCts.Cancel();
                _stateCts.Dispose();
                _stateCts = null;
            }
            _motor.Stop();
        }

        // --- РЕАКТИВНЫЕ ТРИГГЕРЫ ПЕРЕКЛЮЧЕНИЯ ---

        private void OnBatteryChanged(BatteryStateSignal signal)
        {
            // Если мы уже заряжаемся — игнорируем панику батареи!
            if (_currentState is DockedState) return; 

            if (signal.IsLow) 
            {
                ChangeState(_returnToBaseState);
            }
        }

        private void OnDustbinChanged(DustbinStateSignal signal)
        {
            if (signal.IsFull)
            {
                ChangeState(_returnToBaseState);
            }
        }
        private void OnArrivedAtBase(ArrivedAtBaseSignal signal)
        {
            ChangeState(_dockedState);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Unsubscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Unsubscribe<ArrivedAtBaseSignal>(OnArrivedAtBase);
            StopCurrentState();
        }
    }
}