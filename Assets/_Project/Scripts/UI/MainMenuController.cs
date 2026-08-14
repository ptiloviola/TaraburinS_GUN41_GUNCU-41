using UnityEngine;
using UnityEngine.UI; 
using TMPro; // Подключаем библиотеку TextMeshPro
using Zenject;
using TpsShooter.Services.SceneManagement;
using TpsShooter.Services.Progress;

namespace TpsShooter.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        
        // Заменили обычный Text на TextMeshProUGUI
        [SerializeField] private TextMeshProUGUI _highScoreText; 

        private SceneLoaderService _sceneLoader;
        private GameProgressService _progressService;

        [Inject]
        public void Construct(SceneLoaderService sceneLoader, GameProgressService progressService)
        {
            _sceneLoader = sceneLoader;
            _progressService = progressService;
        }

        private void Start()
        {
            if (_playButton != null)
            {
                _playButton.onClick.AddListener(StartGame);
            }

            if (_highScoreText != null && _progressService != null)
            {
                _highScoreText.text = $"MAX LEVEL: {_progressService.HighScore}";
            }
        }

        private void StartGame()
        {
            _progressService.ResetProgress(); 
            _sceneLoader.LoadScene("SampleScene"); // Убедись, что имя сцены точное!
        }

        private void OnDestroy()
        {
            if (_playButton != null)
            {
                _playButton.onClick.RemoveListener(StartGame);
            }
        }
    }
}