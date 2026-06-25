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
        
        // Выпадающий список (Dropdown) для режимов добавим позже, 
        // оставим для него место в верстке.

        // События, через которые View сообщает дирижеру (Presenter'у) о действиях игрока.
        // Action - это встроенный делегат C# (по сути, пустой сигнал без параметров).
        public event Action OnReturnToBaseClicked;
        public event Action OnEmptyBinClicked;
        public event Action OnChoosePointClicked;

        private void Awake()
        {
            // Как только кнопка нажата, мы "выстреливаем" нашим событием
            _returnToBaseButton.onClick.AddListener(() => OnReturnToBaseClicked?.Invoke());
            _emptyBinButton.onClick.AddListener(() => OnEmptyBinClicked?.Invoke());

            if (_choosePointButton != null)
            {
                _choosePointButton.onClick.AddListener(() => OnChoosePointClicked?.Invoke());
            }
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

        private void OnDestroy()
        {
            // Хороший тон - отписываться от кнопок при уничтожении Canvas
            _returnToBaseButton.onClick.RemoveAllListeners();
            _emptyBinButton.onClick.RemoveAllListeners();
        }
    }
}