using UnityEngine;
using TMPro;

namespace Bowling.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] TMP_Text _currentScore;
        [SerializeField] TMP_Text _bestScore;

        [SerializeField] GameStateManager _gameStateManager;

        private int _bestScoreInt;

        private void Start()
        {
            _currentScore.text = "Сбито кеглей: 0";
            _bestScore.text = "Рекорд: 0";
        }

        private void OnEnable()
        {
            _gameStateManager.OnPinsKnockedDown += HandleNewScore;
        }

        private void OnDisable()
        {
            _gameStateManager.OnPinsKnockedDown -= HandleNewScore;
        }

        void HandleNewScore(int score)
        {
            _currentScore.text = $"Сбито кеглей: {score}";
            if (_bestScoreInt < score)
            {
                _bestScoreInt = score;
            }
            _bestScore.text = $"Рекорд: {_bestScoreInt}";
        }

    }
}

