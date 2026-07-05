using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using MeatMushrooms.Player;
using MeatMushrooms.Environment;
using MeatMushrooms.Core;
using Zenject;

namespace MeatMushrooms.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Inject] private PlayerRegistry _playerRegistry;

        [Header("Панели")]
        public GameObject DeathPanel;
        public GameObject VictoryPanel;

        [Header("Тексты на главном экране")]
        public TextMeshProUGUI LevelText;
        public TextMeshProUGUI HighScoreText;

        private void Start()
        {
            if (LevelText != null) LevelText.text = $"Поляна: {GameSession.CurrentLevel}";
            if (HighScoreText != null) HighScoreText.text = $"Рекорд: {PlayerPrefs.GetInt("HighScore", 1)}";

            _playerRegistry.OnPlayerSpawned += HookUpPlayerEvents;
            if (_playerRegistry.Health != null) 
            {
                HookUpPlayerEvents();
            }

            ExitZone.OnLevelCompleted += ShowVictory;
        }

        private void HookUpPlayerEvents() => _playerRegistry.Health.OnDeath += ShowGameOver;

        private void ShowGameOver()
        {
            if (DeathPanel != null) DeathPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        private void ShowVictory()
        {
            if (VictoryPanel != null) VictoryPanel.SetActive(true);
            Time.timeScale = 0f;
            GameSession.CompleteLevel();
        }


        public void RestartGame()
        {
            Time.timeScale = 1f;
            GameSession.Reset();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void NextLevel()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            ExitZone.OnLevelCompleted -= ShowVictory;
            if (_playerRegistry != null)
            {
                _playerRegistry.OnPlayerSpawned -= HookUpPlayerEvents;
                if (_playerRegistry.Health != null) 
                {
                    _playerRegistry.Health.OnDeath -= ShowGameOver;
                }
            }
        }
    }
}
