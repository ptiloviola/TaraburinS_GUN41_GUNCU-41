using System;
using UnityEngine;
using Zenject;
using VacuumSim.Robotics.Signals;
using VacuumSim.Robotics.Contracts;
using VacuumSim.Robotics.Configs;
using VacuumSim.Robotics.Brain;
using VacuumSim.Robotics.Brain.States;
using VacuumSim.Input;
using VacuumSim.Rules;
using VacuumSim.Pathfinding;


namespace VacuumSim.UI
{
    // Не наследует MonoBehaviour! 
    public class VacuumDashboardPresenter : IInitializable, IDisposable, ITickable
    {
        private readonly SignalBus _signalBus;
        private readonly VacuumDashboardView _view;
        private readonly PathfindingGrid _grid;
        private readonly IVacuumDustbin _dustbin;
        private readonly IVacuumBattery _battery;
        private readonly VacuumConfig _config;

        private readonly SmartBrain _brain;
        private readonly ReturnToBaseState _returnState;
        private readonly CleaningState _cleaninState;

        private readonly PlayerInputHandler _inputHandler;



        private int _currentScore;

        public VacuumDashboardPresenter(
            SignalBus signalBus, 
            VacuumDashboardView view, 
            IVacuumDustbin dustbin,
            IVacuumBattery battery,
            VacuumConfig config,
            SmartBrain brain,
            ReturnToBaseState returnState,
            CleaningState cleaninState,
            PlayerInputHandler inputHandler,
            PathfindingGrid grid)
        {
            _signalBus = signalBus;
            _view = view;
            _dustbin = dustbin;
            _battery = battery;
            _config = config;
            _brain = brain;
            _returnState = returnState;
            _cleaninState = cleaninState;
            _inputHandler = inputHandler;
            _grid = grid;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Subscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);

            _signalBus.Subscribe<GameOverSignal>(OnGameOver);

            _view.OnReturnToBaseClicked += HandleReturnToBase;
            _view.OnEmptyBinClicked += HandleStartCleaning;
            _view.OnChoosePointClicked += HandleChoosePoint;
            _view.OnStrategyChanged += HandleStrategyChanged;
            _view.OnRestartClicked += HandleRestart;

            _view.UpdateScore(_currentScore);
            _view.UpdateDustbin(_dustbin.CurrentFill, _config.MaxDustbinCapacity);
            _view.UpdateBattery(_battery.CurrentCharge / _config.MaxBattery);
        }
        public void Tick()
        {
            _view.UpdatePollution(_grid.GetDirtyPercentage());
        }

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
            _currentScore += signal.TrashData.Points; 
            _view.UpdateScore(_currentScore);
        }

        private void HandleReturnToBase()
        {
            Debug.Log("[UI] Вызвана команда возврата на базу!");
            _brain.ChangeState(_returnState);
        }

        private void HandleStartCleaning()
        {
            Debug.Log("[UI] Вызвано возвращение к работе!");
            _brain.ChangeState(_cleaninState);
        }

        private void HandleChoosePoint()
        {
            Debug.Log("[UI] Едем туда!");
            _inputHandler.EnableTargetSelection();
        }

        private void HandleStrategyChanged(int index)
        {
            Debug.Log($"[UI] Игрок переключил стратегию на индекс: {index}");
            
            _cleaninState.SetStrategy(index);
            
            if (_brain.IsActiveState(_cleaninState))
            {
                Debug.Log("[Presenter] Принудительный перезапуск текущей уборки с новой логикой!");
                _brain.ChangeState(_cleaninState, forceRestart: true); 
            }
        }

        private void OnGameOver()
        {
            Time.timeScale = 0f; 

            int bestScore = PlayerPrefs.GetInt("BestVacuumScore", 0);
            bool isNewRecord = _currentScore > bestScore;

            if (isNewRecord)
            {
                Debug.Log($"<color=green>[Score] НОВЫЙ РЕКОРД! {_currentScore} очков!</color>");
                PlayerPrefs.SetInt("BestVacuumScore", _currentScore);
                PlayerPrefs.Save();
            }

            _view.ShowGameOverScreen(_currentScore, isNewRecord);
        }

        private void HandleRestart()
        {
            Debug.Log("[Presenter] Игрок нажал рестарт. Перезапускаю симуляцию...");
            
            Time.timeScale = 1f; 

            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
        }

        public void Dispose()
        {

            _signalBus.Unsubscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Unsubscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Unsubscribe<TrashCollectedSignal>(OnTrashCollected);

            _view.OnReturnToBaseClicked -= HandleReturnToBase;
            _view.OnEmptyBinClicked -= HandleStartCleaning;
            _view.OnChoosePointClicked -= HandleChoosePoint;
            _view.OnStrategyChanged -= HandleStrategyChanged;
            _view.OnRestartClicked -= HandleRestart;
        }
    }
}