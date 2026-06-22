using Bowling.Ball;
using UnityEngine;
using Bowling.BowlingPins;
using Bowling.UI;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Bowling.Gameplay
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private BallController _ballController;
        [SerializeField] private BaseThrowMechanic[] _inputMechanics;
        [SerializeField] private PinDeckManager _pinDeckManager;

        [SerializeField] private PhysicsConfig _physicsConfig;

        [Header("Настройки таймингов")]
        [SerializeField] private float _maxScoringWaitTime = 12f;
        
        [SerializeField] private StrikeAndSpareEffect _strikeEffect;

        private BowlingScoreCalculator _scoreCalculator;
        private BowlingGameLoop _gameLoop;

        // Рубильник для отмены асинхронной задачи
        private CancellationTokenSource _scoringCts;

        private BaseThrowMechanic _activeMechanic;
        private bool _isGameOver = false;
        private BowlingInputActions _inputActions;

        public event Action<int, int, int> OnGameStateUpdated;
        public event Action OnGameOver;

        private void Awake()
        {
            _scoreCalculator = new BowlingScoreCalculator();
            _gameLoop = new BowlingGameLoop();
            _inputActions = new BowlingInputActions();
        }


        private void Start()
        {
            SetPhysicsStrategy(0);
            foreach (var mechanic in _inputMechanics)
            {
                if (mechanic != null) 
                {
                    mechanic.Initialize(_inputActions);
                }
            }
            SetInputMechanic(0);  
            if (_pinDeckManager.ActivePins.Count == 0)
            {
                _pinDeckManager.SpawnPins();
            }
            UpdateUI();
        }

        public void SetPhysicsStrategy(int index)
        {
            IThrowStrategy newStrategy = ThrowStrategyFactory.CreateStrategy(index, _physicsConfig);
            
            if (newStrategy != null)
            {
                _ballController.SetStrategy(newStrategy);
            }
            
            ResetFullGame();
        }

        public void SetInputMechanic(int index)
        {
            foreach (var mechanic in _inputMechanics)
            {
                if (mechanic != null)
                {
                    mechanic.OnThrowExecuted -= HandleThrowExecuted;
                    mechanic.DisableMechanic();
                }
            }
            
            if (index >= 0 && index < _inputMechanics.Length)
            {
                _activeMechanic = _inputMechanics[index];
                _activeMechanic.EnableMechanic();
                _activeMechanic.OnThrowExecuted += HandleThrowExecuted;
                Debug.Log($"Механика ввода изменена на: {_activeMechanic.GetType().Name}");
            }
        }

        private void HandleThrowExecuted(Vector3 dir, float force)
        {
            if (_isGameOver) return;
            _ballController.ThrowBall(dir, force);
            StartScoringRoutine();
        }




        public void ResetRound()
        {
            CancelScoringTask();

            _ballController.ResetBall();
            if (_activeMechanic != null) _activeMechanic.ResetMechanic();
            if (_pinDeckManager.ActivePins != null) _pinDeckManager.ResetDeck();
            
            Debug.Log("Раунд сброшен. Можно бросать снова!");
        }

        public void StartScoringRoutine() 
        {
            CancelScoringTask(); // Отменяем предыдущую задачу, если она была
            
            _scoringCts = new CancellationTokenSource(); // Создаем новый рубильник
            
            // Запускаем асинхронную задачу и передаем ей провод (токен). Forget() значит "не жди результата здесь"
            CalculateScoreAfterDelayAsync(_scoringCts.Token).Forget();
        }

        private async UniTaskVoid CalculateScoreAfterDelayAsync(CancellationToken token)
        {
            // 1. Ждем полсекунды (с поддержкой отмены)
            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
            float timer = 0f;
            while (timer < _maxScoringWaitTime)
            {
                timer += Time.deltaTime;

                if (_ballController.IsSettled() && _pinDeckManager.AreAllPinsSettled())
                {
                    break;
                }
                // Ждем следующего кадра (аналог yield return null)
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
            
            int totalPinsOnDeck = _pinDeckManager.ActivePins.Count;
            int fallenCount = 0;
            
            foreach(var pin in _pinDeckManager.ActivePins)
            {
                if(pin != null && pin.IsFallen())
                {
                    fallenCount++;
                }
            }
            Debug.Log($"Бросок завершен! Упало кеглей: {fallenCount} из {totalPinsOnDeck}");
            _scoreCalculator.AddRoll(fallenCount);
            TurnResult result = _gameLoop.RegisterThrow(fallenCount);
            UpdateUI();

            if (fallenCount == totalPinsOnDeck && totalPinsOnDeck > 0)
            {
                string strikeOrSpareText = "STRIKE";
                if (totalPinsOnDeck == _pinDeckManager.FullDeckSize)
                {
                    Debug.Log("СТРАЙК!!!");
                }
                else
                {
                    Debug.Log("СПЭР!!!");  
                    strikeOrSpareText = "SPARE";
                }
                if (_strikeEffect != null) _strikeEffect.PlayStrikeSpareEffect(strikeOrSpareText);
            }
            
            ProcessTurnResult(result);
        }

        private void CancelScoringTask()
        {
            if (_scoringCts != null)
            {
                _scoringCts.Cancel();  // Дергаем рубильник
                _scoringCts.Dispose(); // Освобождаем память
                _scoringCts = null;
            }
        }

        private void ProcessTurnResult(TurnResult result)
        {
            switch(result)
            {
                case TurnResult.NextThrow:
                case TurnResult.NextFrame:
                    if (result == TurnResult.NextThrow) _pinDeckManager.RemoveFallenPins();
                    else _pinDeckManager.ResetDeck();
                    
                    _ballController.ResetBall();
                    if (_activeMechanic != null) _activeMechanic.ResetMechanic();
                    break;
                case TurnResult.GameOver:
                    _isGameOver = true;
                    OnGameOver?.Invoke();
                    break;
            }
        }

        public void ResetFullGame()
        {
            CancelScoringTask();

            _isGameOver = false;
            _scoreCalculator.ResetGame();
            _gameLoop.ResetLoop();
            _ballController.ResetBall();
            _pinDeckManager.ResetDeck();
            if (_activeMechanic != null) _activeMechanic.ResetMechanic();
            UpdateUI();
            Debug.Log("Игра полностью сброшена! Начинаем с 1 фрейма.");
        }

        private void UpdateUI()
        {
            int currentScore = _scoreCalculator.CalculateTotalScore();
            OnGameStateUpdated?.Invoke(_gameLoop.CurrentFrame, _gameLoop.CurrentThrow, currentScore);
        }


        private void OnDestroy()
        {
            CancelScoringTask();
            if (_activeMechanic != null)
            {
                _activeMechanic.OnThrowExecuted -= HandleThrowExecuted;
            }
        }
    }

    
}


