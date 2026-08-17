using UnityEngine;
using UnityEngine.UI; 
using TMPro; 
using Zenject;
using TpsShooter.Services.SceneManagement;
using TpsShooter.Services.Progress;
using TpsShooter.UI.Settings; // <-- Добавлено
using TpsShooter.Audio;       // <-- Для AudioConfig

namespace TpsShooter.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Main Menu UI")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton; // <--- НОВАЯ КНОПКА ОТКРЫТИЯ НАСТРОЕК
        [SerializeField] private TextMeshProUGUI _highScoreText; 

        [Header("Settings UI")]
        [SerializeField] private SettingsUIView _settingsView; // <--- ССЫЛКА НА ВЬЮШКУ НАСТРОЕК

        private SceneLoaderService _sceneLoader;
        private GameProgressService _progressService;
        private AudioConfig _audioConfig;
        
        private SettingsPresenter _settingsPresenter;

        [Inject]
        public void Construct(
            SceneLoaderService sceneLoader, 
            GameProgressService progressService, 
            AudioConfig audioConfig) // <--- ИНЪЕКЦИЯ АУДИОКОНФИГА
        {
            _sceneLoader = sceneLoader;
            _progressService = progressService;
            _audioConfig = audioConfig;
        }

        private void Start()
        {
            if (_playButton != null)
                _playButton.onClick.AddListener(StartGame);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OpenSettings);

            if (_highScoreText != null && _progressService != null)
                _highScoreText.text = $"MAX LEVEL: {_progressService.HighScore}";

            // Инициализация MVP настроек
            if (_settingsView != null)
            {
                _settingsView.Hide(); // Скрываем при старте
                SettingsModel settingsModel = new SettingsModel();
                _settingsPresenter = new SettingsPresenter(settingsModel, _settingsView, _audioConfig);
            }
        }

        private void StartGame()
        {
            _progressService.ResetProgress(); 
            _sceneLoader.LoadScene("SampleScene"); 
        }

        private void OpenSettings()
        {
            _settingsView?.Show();
        }

        private void OnDestroy()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(StartGame);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OpenSettings);
            
            _settingsPresenter?.Dispose(); // Очищаем подписки
        }
    }
}