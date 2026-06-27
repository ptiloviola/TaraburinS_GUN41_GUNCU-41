using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VacuumSim.UI
{
    public class VacuumDashboardView : MonoBehaviour
    {
        [Header("Индикаторы (Слайдеры и Тексты)")]
        [SerializeField] private Slider _batterySlider;
        [SerializeField] private TextMeshProUGUI _batteryText;
        [SerializeField] private TextMeshProUGUI _dustbinText;
        [SerializeField] private TextMeshProUGUI _scoreText;

        [Header("Интерактивные элементы (Кнопки)")]
        [SerializeField] private Button _returnToBaseButton;
        [SerializeField] private Button _emptyBinButton;
        [SerializeField] private Button _choosePointButton;
        [SerializeField] private TMP_Dropdown _strategyDropdown;

        [Header("Панель Game Over")]
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private Button _restartButton;

        [Header("Индикаторы уровня")]
        [SerializeField] private TextMeshProUGUI _pollutionText;


        public event Action OnReturnToBaseClicked;
        public event Action OnEmptyBinClicked;
        public event Action OnChoosePointClicked;
        public event Action<int> OnStrategyChanged;
        public event Action OnRestartClicked;

        private void Awake()
        {
            _returnToBaseButton.onClick.AddListener(() => OnReturnToBaseClicked?.Invoke());
            _emptyBinButton.onClick.AddListener(() => OnEmptyBinClicked?.Invoke());

            if (_choosePointButton != null)
            {
                _choosePointButton.onClick.AddListener(() => OnChoosePointClicked?.Invoke());
            }

            if (_strategyDropdown != null)
                _strategyDropdown.onValueChanged.AddListener((index) => OnStrategyChanged?.Invoke(index));
            
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());

            if (_gameOverPanel != null) 
                _gameOverPanel.SetActive(false);
        
        }

        public void UpdateBattery(float percentage)
        {
            if (_batterySlider != null) _batterySlider.value = percentage;
            if (_batteryText != null) _batteryText.text = $"{Mathf.RoundToInt(percentage * 100)}%";
        }

        public void UpdateDustbin(float current, float max)
        {
            if (_dustbinText != null) _dustbinText.text = $"Бак: {current}/{max}";
        }

        public void UpdateScore(int score)
        {
            if (_scoreText != null) _scoreText.text = $"Очки: {score}";
        }

        public void ShowGameOverScreen(int finalScore, bool isNewRecord)
        {
            if (_gameOverPanel != null) _gameOverPanel.SetActive(true);
            if (_finalScoreText != null) _finalScoreText.text = $"Собрано мусора на: {finalScore} очков";

            if (_bestScoreText != null)
            {
                if (isNewRecord)
                {
                    _bestScoreText.text = "<color=green>новый рекорд комнаты!</color>";
                }
                else
                {
                    int topScore = PlayerPrefs.GetInt("BestVacuumScore", 0);
                    _bestScoreText.text = $"Лучший результат: {topScore}";
                }
            }
        }

        public void UpdatePollution(float pollutionFraction)
        {
            if (_pollutionText != null)
            {
                int percent = Mathf.RoundToInt(pollutionFraction * 100);
                
                string colorHex = percent > 50 ? "#ff4d4d" : "#ffffff"; 
                
                _pollutionText.text = $"загрязнение комнаты: <color={colorHex}>{percent}%</color>";
            }
        }

        private void OnDestroy()
        {
            _returnToBaseButton.onClick.RemoveAllListeners();
            _emptyBinButton.onClick.RemoveAllListeners();
            _choosePointButton.onClick.RemoveAllListeners();
            _strategyDropdown.onValueChanged.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
        }
    }
}