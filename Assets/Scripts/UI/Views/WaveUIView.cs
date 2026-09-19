using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Gameplay.UI.Views
{
    public class WaveUIView : MonoBehaviour
    {
        [Header("Таймер")]
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private Button _forceStartButton;

        [Header("Волны")]
        [SerializeField] private TextMeshProUGUI _waveNumberText;
        
        [Header("Прогноз")]
        [SerializeField] private Transform _forecastContainer;

        [Header("Блокировка UI")]
        [SerializeField] private CanvasGroup _canvasGroup;

        public Transform ForecastContainer => _forecastContainer;
        
        public event Action OnForceStartClicked;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            
            _forceStartButton.onClick.AddListener(() => OnForceStartClicked?.Invoke());
        }

        public void SetInteractable(bool isInteractable)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.interactable = isInteractable;
                _canvasGroup.blocksRaycasts = isInteractable; 
            }
        }

        public void UpdateTimer(float progress, string timeText, bool canForceStart)
        {
            if (_timerFillImage != null) _timerFillImage.fillAmount = progress;
            if (_timerText != null) _timerText.text = timeText;
            
            _forceStartButton.interactable = canForceStart;
        }

        public void UpdateWaveNumber(string text)
        {
            if (_waveNumberText != null) _waveNumberText.text = text;
        }

        private void OnDestroy()
        {
            _forceStartButton.onClick.RemoveAllListeners();
        }
    }
}