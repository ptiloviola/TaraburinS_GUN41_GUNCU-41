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
        [SerializeField] private GameObject _gameOverPanel; // Объект-родитель всей панели
        [SerializeField] private TextMeshProUGUI _finalScoreText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private Button _restartButton;

        [Header("Индикаторы уровня")]
        [SerializeField] private TextMeshProUGUI _pollutionText;




        
        // Выпадающий список (Dropdown) для режимов добавим позже, 
        // оставим для него место в верстке.

        // События, через которые View сообщает дирижеру (Presenter'у) о действиях игрока.
        // Action - это встроенный делегат C# (по сути, пустой сигнал без параметров).
        public event Action OnReturnToBaseClicked;
        public event Action OnEmptyBinClicked;
        public event Action OnChoosePointClicked;
        public event Action<int> OnStrategyChanged;
        public event Action OnRestartClicked;

        private void Awake()
        {
            // Как только кнопка нажата, мы "выстреливаем" нашим событием
            _returnToBaseButton.onClick.AddListener(() => OnReturnToBaseClicked?.Invoke());
            _emptyBinButton.onClick.AddListener(() => OnEmptyBinClicked?.Invoke());

            if (_choosePointButton != null)
            {
                _choosePointButton.onClick.AddListener(() => OnChoosePointClicked?.Invoke());
            }
            // ПОДПИСЫВАЕМСЯ НА ИЗМЕНЕНИЕ ЗНАЧЕНИЯ В DROPDOWN
            if (_strategyDropdown != null)
                _strategyDropdown.onValueChanged.AddListener((index) => OnStrategyChanged?.Invoke(index));
            
            if (_restartButton != null)
                _restartButton.onClick.AddListener(() => OnRestartClicked?.Invoke());

            // При старте игры обязательно прячем панель Game Over, если забыли выключить в инспекторе
            if (_gameOverPanel != null) 
                _gameOverPanel.SetActive(false);
        
        }

        // =========================================================
        // ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ОБНОВЛЕНИЯ ВИЗУАЛА (Дергает Presenter)
        // =========================================================

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
                // Переводим доли (0.2f) в понятные проценты (20%)
                int percent = Mathf.RoundToInt(pollutionFraction * 100);
                
                // Можно добавить цветовую индикацию: если грязно - краснеет!
                string colorHex = percent > 50 ? "#ff4d4d" : "#ffffff"; 
                
                _pollutionText.text = $"загрязнение комнаты: <color={colorHex}>{percent}%</color>";
            }
        }

        private void OnDestroy()
        {
            // Хороший тон - отписываться от кнопок при уничтожении Canvas
            _returnToBaseButton.onClick.RemoveAllListeners();
            _emptyBinButton.onClick.RemoveAllListeners();
            _choosePointButton.onClick.RemoveAllListeners();
            _strategyDropdown.onValueChanged.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
        }
    }
}