using UnityEngine;
using Bowling.Gameplay;


namespace Bowling.UI.MVP
{
    public class ScorePresenter : MonoBehaviour
    {
        [Header("Связи MVP")]
        [SerializeField] private ScoreModel _model;
        [SerializeField] private ScoreView _view;
        
        [Header("Связь с игрой")]
        [SerializeField] private GameStateManager _gameStateManager;

        void Start()
        {
            if (_gameStateManager != null)
            {
                _gameStateManager.OnGameStateUpdated += HandleGameStateUpdated;
                _gameStateManager.OnGameOver += HandleGameOver;
            }
        }

        private void HandleGameStateUpdated(int frame, int throwNum, int score)
        {
            _model.UpdateData(frame, throwNum, score);

            string mainText = $"Фрейм: {_model.CurrentFrame}/10\n" +
                              $"Бросок: {_model.CurrentThrow}\n" +
                              $"Очки: {_model.CurrentScore}";
                              
            string bestText = $"Рекорд: {_model.BestScore}";

            _view.SetMainText(mainText);
            _view.SetBestScoreText(bestText);
        }

        private void HandleGameOver()
        {
            _view.AppendToMainText("\n<color=red>ИГРА ОКОНЧЕНА!</color>");
        }

        private void OnDestroy()
        {
            if (_gameStateManager != null)
            {
                _gameStateManager.OnGameStateUpdated -= HandleGameStateUpdated;
                _gameStateManager.OnGameOver -= HandleGameOver;
            }
        }





    }
}

