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

        // Zenject внедряет все зависимости сюда, включая наш View со сцены
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
            // 1. Подписываемся на Данные от систем пылесоса (Модели)
            _signalBus.Subscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Subscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Subscribe<TrashCollectedSignal>(OnTrashCollected);

            _signalBus.Subscribe<GameOverSignal>(OnGameOver);

            // 2. Подписываемся на клики игрока из интерфейса (View)
            _view.OnReturnToBaseClicked += HandleReturnToBase;
            _view.OnEmptyBinClicked += HandleStartCleaning;
            _view.OnChoosePointClicked += HandleChoosePoint;
            _view.OnStrategyChanged += HandleStrategyChanged;
            _view.OnRestartClicked += HandleRestart;

            // Задаем стартовое значение очков
            _view.UpdateScore(_currentScore);
            _view.UpdateDustbin(_dustbin.CurrentFill, _config.MaxDustbinCapacity);
            _view.UpdateBattery(_battery.CurrentCharge / _config.MaxBattery);
        }
        public void Tick()
        {
            // Плавно и незаметно обновляем UI в реальном времени
            _view.UpdatePollution(_grid.GetDirtyPercentage());
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
            Debug.Log("[UI] Вызвана команда возврата на базу!");
            _brain.ChangeState(_returnState);
        }

        private void HandleStartCleaning()
        {
            Debug.Log("[UI] Вызвано возвращение к работе!");
            // Дирижер напрямую дергает логику бака, потому что у него есть ссылка на интерфейс
            _brain.ChangeState(_cleaninState);
        }

        private void HandleChoosePoint()
        {
            Debug.Log("[UI] Едем туда!");
            // Дирижер напрямую дергает логику бака, потому что у него есть ссылка на интерфейс
            _inputHandler.EnableTargetSelection();
        }

        private void HandleStrategyChanged(int index)
        {
            Debug.Log($"[UI] Игрок переключил стратегию на индекс: {index}");
            
            // 1. Меняем активный индекс
            _cleaninState.SetStrategy(index);
            
            // 2. Если робот УЖЕ убирается прямо сейчас — перезапускаем его
            if (_brain.IsActiveState(_cleaninState))
            {
                Debug.Log("[Presenter] Принудительный перезапуск текущей уборки с новой логикой!");
                _brain.ChangeState(_cleaninState, forceRestart: true); 
            }
            // Если он на базе или в ручном транзите - мы просто запомнили индекс, 
            // и новая стратегия применится сама, когда он начнет уборку!
        }

        private void OnGameOver()
        {
            // Жестко останавливаем всю физику и спавнеры мусора
            Time.timeScale = 0f; 

            // Достаем старый рекорд (если его нет, вернется 0)
            int bestScore = PlayerPrefs.GetInt("BestVacuumScore", 0);
            bool isNewRecord = _currentScore > bestScore;

            if (isNewRecord)
            {
                Debug.Log($"<color=green>[Score] НОВЫЙ РЕКОРД! {_currentScore} очков!</color>");
                PlayerPrefs.SetInt("BestVacuumScore", _currentScore);
                PlayerPrefs.Save();
            }

            // Передаем данные во View, чтобы он показал финальную панель
            _view.ShowGameOverScreen(_currentScore, isNewRecord);
        }

        private void HandleRestart()
        {
            Debug.Log("[Presenter] Игрок нажал рестарт. Перезапускаю симуляцию...");
            
            // КРИТИЧЕСКИ ВАЖНО: возвращаем время в нормальный поток! 
            // Иначе новая сцена загрузится на вечной паузе.
            Time.timeScale = 1f; 

            // Перезагружаем текущую активную сцену
            int currentSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneIndex);
        }

        public void Dispose()
        {
            // Отписка от сигналов шины
            _signalBus.Unsubscribe<BatteryStateSignal>(OnBatteryChanged);
            _signalBus.Unsubscribe<DustbinStateSignal>(OnDustbinChanged);
            _signalBus.Unsubscribe<TrashCollectedSignal>(OnTrashCollected);

            // Отписка от событий View
            _view.OnReturnToBaseClicked -= HandleReturnToBase;
            _view.OnEmptyBinClicked -= HandleStartCleaning;
            _view.OnChoosePointClicked -= HandleChoosePoint;
            _view.OnStrategyChanged -= HandleStrategyChanged;
            _view.OnRestartClicked -= HandleRestart;
        }
    }
}