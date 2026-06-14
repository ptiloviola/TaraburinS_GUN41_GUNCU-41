
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Infrastructure.Signals;


namespace Gameplay.UI
{
    public class WaveUIPresenter : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private Image _timerFillImage; // Кружок (Image Type: Filled)
        [SerializeField] private TextMeshProUGUI _timerText; // Текст "14 сек"
        [SerializeField] private TextMeshProUGUI _waveNumberText; // Текст "Волна 1/5"
        [SerializeField] private Button _forceStartButton; // Кнопка досрочного старта

        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.Subscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _forceStartButton.onClick.AddListener(OnForceStartClicked);
            
        }

        private void OnDisable()
        {
            _signalBus.TryUnsubscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.TryUnsubscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            
            _forceStartButton.onClick.RemoveListener(OnForceStartClicked);
        }

        // Обновляем таймер
        private void OnTimerUpdated(SignalWaveTimerUpdated signal)
        {
            if (_timerFillImage != null)
            {
                _timerFillImage.fillAmount = signal.Progress;
            }
            if (_timerText != null)
            {
                // Показываем текст только если время больше нуля
                if (signal.TimeLeft > 0)
                {
                    _timerText.text = Mathf.CeilToInt(signal.TimeLeft).ToString();
                    // Кнопка активна
                    _forceStartButton.interactable = true;
                }
                else
                {
                    _timerText.text = "Attack";
                    // Выключаем кнопку во время волны
                    _forceStartButton.interactable = false;
                }
            }
        }

        // Обновляем номер волны
        private void OnWaveStateChanged(SignalWaveStateChanged signal)
        {
            if (_waveNumberText != null)
            {
                _waveNumberText.text = $"Волна {signal.CurrentWave}/{signal.TotalWaves}";
            }
        }

        // Клик по кнопке отправляет сигнал Режиссеру
        private void OnForceStartClicked()
        {
            _signalBus.Fire<SignalForceStartWave>();
        }

    }
}


