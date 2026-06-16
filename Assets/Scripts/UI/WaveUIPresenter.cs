
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;
using Infrastructure.Signals;
using System.Collections.Generic;
using Gameplay.Enemies.Data;



namespace Gameplay.UI
{
    public class WaveUIPresenter : MonoBehaviour
    {
        [Header("UI Элементы")]
        [SerializeField] private Image _timerFillImage; // Кружок (Image Type: Filled)
        [SerializeField] private TextMeshProUGUI _timerText; // Текст "14 сек"
        [SerializeField] private TextMeshProUGUI _waveNumberText; // Текст "Волна 1/5"
        [SerializeField] private Button _forceStartButton; // Кнопка досрочного старта

        [Header("Прогноз волны")]
        // Ссылка на наш пустой контейнер
        [SerializeField] private Transform _forecastContainer;
        // Внедряем фабрику иконок
        private ForecastIconView.Factory _iconFactory;

        private SignalBus _signalBus;
        private EnemyRegistry _enemyRegistry;

        [Inject]
        public void Construct(SignalBus signalBus, ForecastIconView.Factory iconFactory, 
            EnemyRegistry enemyRegistry)
        {
            _signalBus = signalBus;
            _iconFactory = iconFactory;
            _enemyRegistry = enemyRegistry;
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.Subscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _signalBus.Subscribe<SignalWaveForecastUpdated>(OnForecastUpdated); // НОВОЕ

            _forceStartButton.onClick.AddListener(OnForceStartClicked);
            
        }

        private void OnDisable()
        {
            _signalBus.TryUnsubscribe<SignalWaveTimerUpdated>(OnTimerUpdated);
            _signalBus.TryUnsubscribe<SignalWaveStateChanged>(OnWaveStateChanged);
            _signalBus.TryUnsubscribe<SignalWaveForecastUpdated>(OnForecastUpdated); // НОВОЕ
            
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

        // НОВОЕ: Метод отрисовки прогноза
        private void OnForecastUpdated(SignalWaveForecastUpdated signal)
        {
            if (_forecastContainer == null) return;

            // 1. Очищаем контейнер от старых иконок прошлой волны
            foreach (Transform child in _forecastContainer)
            {
                Destroy(child.gameObject);
            }

            // 2. Создаем новые карточки
            foreach (KeyValuePair<string, int> kvp in signal.EnemyCounts)
            {
                ForecastIconView iconObj = _iconFactory.Create();
                
                // Обязательно false во втором параметре, чтобы UI масштаб не сломался
                iconObj.transform.SetParent(_forecastContainer, false); 
                // --- МАГИЯ ЗДЕСЬ ---
                // Запрашиваем конфиг врага по его строковому ID
                EnemyConfig config = _enemyRegistry.GetEnemyById(kvp.Key);
                // Достаем иконку (если конфиг или иконка не найдены, передастся null)
                Sprite iconSprite = config != null ? config.UIIcon : null;
                
                iconObj.Setup(kvp.Key, kvp.Value, iconSprite);
            }
        }

    }
}


