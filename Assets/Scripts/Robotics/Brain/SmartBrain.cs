using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Signals;
using VacuumSim.Robotics.Brain.States;
using VacuumSim.Robotics.Components;
using VacuumSim.Pathfinding;

namespace VacuumSim.Robotics.Brain
{
    public class SmartBrain : IVacuumBrain, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly IVacuumMotor _motor;
        
        private readonly CleaningState _cleaningState;
        private readonly ReturnToBaseState _returnToBaseState;
        private readonly ManualTransitState _manualTransitState;

        private IVacuumState _currentState;
        private CancellationTokenSource _stateCts;
        private readonly DockedState _dockedState;
        private readonly VacuumCollector _collector;
        private readonly PathfindingGrid _grid;



        public SmartBrain(
            SignalBus signalBus, 
            IVacuumMotor motor,
            CleaningState cleaningState,
            ReturnToBaseState returnToBaseState,
            DockedState dockedState,
            VacuumCollector collector,
            ManualTransitState manualTransitState,
            PathfindingGrid grid)
        {
            _signalBus = signalBus;
            _motor = motor;
            _cleaningState = cleaningState;
            _returnToBaseState = returnToBaseState;
            _dockedState = dockedState;
            _collector = collector;
            _manualTransitState = manualTransitState;
            _grid = grid;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Subscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Subscribe<ArrivedAtBaseSignal>(OnArrivedAtBase);
            _signalBus.Subscribe<TargetPointSelectedSignal>(OnTargetPointSelected);
            _signalBus.Subscribe<TransitCompletedSignal>(OnManualTransitCompleted);
        }

        public async UniTask StartCleaningAsync(CancellationToken token)
        {
            ChangeState(_cleaningState);

            while (!token.IsCancellationRequested)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            StopCurrentState();
        }

        public void ChangeState(IVacuumState newState, bool forceRestart = false)
        {
            if (!forceRestart && _currentState == newState) return;

            StopCurrentState();

            _currentState = newState;
            _stateCts = new CancellationTokenSource();

            Debug.Log($"[SmartBrain] Смена режима на: {newState.GetType().Name}");

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

        private void OnBatteryChanged(BatteryStateSignal signal)
        {
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

        private void OnTargetPointSelected(TargetPointSelectedSignal signal)
        {
            _grid.ResetCleaningMemory();
            
            _manualTransitState.TargetPoint = signal.Point;
            ChangeState(_manualTransitState);
        }

        private void OnManualTransitCompleted()
        {
            ChangeState(_cleaningState);
        }

        public bool IsActiveState(IVacuumState state)
        {
            return _currentState == state;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Unsubscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Unsubscribe<ArrivedAtBaseSignal>(OnArrivedAtBase);
            _signalBus.Unsubscribe<TargetPointSelectedSignal>(OnTargetPointSelected);
            _signalBus.Unsubscribe<TransitCompletedSignal>(OnManualTransitCompleted);
            StopCurrentState();
        }
    }
}