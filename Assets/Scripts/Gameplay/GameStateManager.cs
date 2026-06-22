
using UnityEngine;
using Bowling.BowlingPins;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;

namespace Bowling.Gameplay
{
    public class GameStateManager : MonoBehaviour
    {
        
        
        [Header("Настройки таймингов")]
        [SerializeField] private float _maxScoringWaitTime = 12f;
        public event Action<string> OnStrikeOrSpare;

        private BowlingScoreCalculator _scoreCalculator;
        private BowlingGameLoop _gameLoop;

        // Рубильник для отмены асинхронной задачи
        private CancellationTokenSource _scoringCts;

        

        private PinDeckManager _pinDeckManager;

        public event Action<int, int, int> OnGameStateUpdated;
        public event Action OnGameOver;

        private PlayerController _playerController;

        [Inject]
        public void Construct(
            BowlingScoreCalculator scoreCalculator, 
            BowlingGameLoop gameLoop, 
            PlayerController playerController,
            PinDeckManager pinDeckManager)
        {
            _scoreCalculator = scoreCalculator;
            _gameLoop = gameLoop;
            _playerController = playerController;
            _pinDeckManager = pinDeckManager;
            
        }


        private void Start()
        {
            if (_pinDeckManager.ActivePins.Count == 0)
            {
                _pinDeckManager.SpawnPins();
            }
            UpdateUI();
        }

        

        public void ResetRound()
        {
            CancelScoringTask();

            _playerController.ResetPlayerState();

            if (_pinDeckManager.ActivePins != null) _pinDeckManager.ResetDeck();
            
            Debug.Log("Раунд сброшен. Можно бросать снова!");
        }

        public void StartScoringRoutine() 
        {
            CancelScoringTask(); // Отменяем предыдущую задачу, если она была
            
            _scoringCts = new CancellationTokenSource(); // Создаем новый рубильник
            
            // Запускаем асинхронную задачу и передаем ей провод (токен). 
            // Forget() значит "не жди результата здесь"
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

                if (_playerController.IsBallSettled() && _pinDeckManager.AreAllPinsSettled())
                {
                    break;
                }
                // аналог yield return null
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: token);
            }
            
            int totalPinsOnDeck = _pinDeckManager.GetActivePinsCount();
            int fallenCount = _pinDeckManager.GetFallenPinsCount();
            
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
                OnStrikeOrSpare?.Invoke(strikeOrSpareText);
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
                    
                    _playerController.ResetPlayerState();
                    break;
                case TurnResult.GameOver:
                    OnGameOver?.Invoke();
                    break;
            }
        }

        public void ResetFullGame()
        {
            CancelScoringTask();

            _scoreCalculator.ResetGame();
            _gameLoop.ResetLoop();
            _playerController.ResetPlayerState();
            _pinDeckManager.ResetDeck();

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
        }
    }

    
}


