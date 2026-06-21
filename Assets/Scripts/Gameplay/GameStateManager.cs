using Bowling.Ball;
using UnityEngine;
using Bowling.BowlingPins;
using System.Collections;
using Bowling.UI;
using TMPro;

namespace Bowling.Gameplay
{
    public class GameStateManager : MonoBehaviour
    {
        [SerializeField] private BallController _ballController;
        [SerializeField] private BaseThrowMechanic[] _inputMechanics;
        [SerializeField] private PinDeckManager _pinDeckManager;

        [SerializeField] private PhysicsConfig _physicsConfig;
        
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _bestScoreText;
        private int _bestScore = 0;
        [SerializeField] private StrikeAndSpareEffect _strikeEffect;

        private BowlingScoreCalculator _scoreCalculator;
        private BowlingGameLoop _gameLoop;

        private Coroutine _scoringCoroutine;
        private BaseThrowMechanic _activeMechanic;
        private bool _isGameOver = false;
        private BowlingInputActions _inputActions;

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
            if (_scoringCoroutine != null) StopCoroutine(_scoringCoroutine);
            _scoringCoroutine = StartCoroutine(CalculateScoreAfterDelay());
        }



        public void ResetRound()
        {
            if (_scoringCoroutine != null)
            {
                StopCoroutine(_scoringCoroutine);
                _scoringCoroutine = null;
            }

            _ballController.ResetBall();
            
            if (_activeMechanic != null)
            {
                _activeMechanic.ResetMechanic();
            }

            if (_pinDeckManager.ActivePins != null)
            {
                _pinDeckManager.ResetDeck();
            }


            _pinDeckManager.ResetDeck();
            Debug.Log("Раунд сброшен. Можно бросать снова!");
        }

        public void StartScoringRoutine() 
        {
            if (_scoringCoroutine != null)
            {
                StopCoroutine(_scoringCoroutine);
            }
            _scoringCoroutine = StartCoroutine(CalculateScoreAfterDelay());
        }

        private IEnumerator CalculateScoreAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            float maxWaitTime = 7f;
            float timer = 0f;
            while (timer < maxWaitTime)
            {
                timer += Time.deltaTime;

                if (_ballController.IsSettled() && _pinDeckManager.AreAllPinsSettled())
                {
                    break;
                }

                yield return null; // Ждем следующий кадр
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
            int currentScore = _scoreCalculator.CalculateTotalScore();
            if (currentScore > _bestScore)
            {
                _bestScore = currentScore;
            }
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

        private void ProcessTurnResult(TurnResult result)
        {
            switch(result)
            {
                case TurnResult.NextThrow:
                    _pinDeckManager.RemoveFallenPins();
                    _ballController.ResetBall();
                    if (_activeMechanic != null) _activeMechanic.ResetMechanic();
                    break;
                case TurnResult.NextFrame:
                    _pinDeckManager.ResetDeck();
                    _ballController.ResetBall();
                    if (_activeMechanic != null) _activeMechanic.ResetMechanic();
                    break;
                case TurnResult.GameOver:
                    _isGameOver = true;
                    _scoreText.text += "\nИГРА ОКОНЧЕНА!";
                    break;
            }
        }

        public void ResetFullGame()
        {
            if (_scoringCoroutine != null)
            {
                StopCoroutine(_scoringCoroutine);
                _scoringCoroutine = null;
            }
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
            if (_scoreText != null)
            {
                _scoreText.text = $"Фрейм: {_gameLoop.CurrentFrame}/10\n" +
                                  $"Бросок: {_gameLoop.CurrentThrow}\n" +
                                  $"Очки: {_scoreCalculator.CalculateTotalScore()}";
            }
            if (_bestScoreText != null)
            {
                _bestScoreText.text = $"Рекорд: {_bestScore}";
            }
        }


        private void OnDestroy()
        {
            if (_activeMechanic != null)
            {
                _activeMechanic.OnThrowExecuted -= HandleThrowExecuted;
            }
        }
    }

    
}


