using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using Gameplay.Spawning.Data;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Enemies;


namespace Gameplay.Spawning.Services
{
    /// <summary>
    /// Дирижер. Знает о фазах игры, награждает за скипы, двигает UI.
    /// Автоматически создается и уничтожается контейнером Zenject.
    /// </summary>
    public class WaveStateController : IInitializable, IDisposable
    {
        private readonly IWaveProvider _waveProvider;
        private readonly WaveTimerService _timerService;
        private readonly WaveSpawnerService _spawnerService;
        
        private readonly BaseRegistry _baseRegistry;
        private readonly EnemyTrackerService _enemyTracker;
        private readonly BankService _bankService;
        private readonly SignalBus _signalBus;

        private CancellationTokenSource _cts;
        private int _currentWaveNumber = 0;
        


        public WaveStateController(
            IWaveProvider waveProvider,
            WaveTimerService timerService,
            WaveSpawnerService spawnerService,
            BaseRegistry baseRegistry,
            EnemyTrackerService enemyTracker,
            BankService bankService,
            SignalBus signalBus)
        {
            _waveProvider = waveProvider;
            _timerService = timerService;
            _spawnerService = spawnerService;
            _baseRegistry = baseRegistry;
            _enemyTracker = enemyTracker;
            _bankService = bankService;
            _signalBus = signalBus;
        }

        // Вызывается автоматически при старте сцены (аналог Start)
        public void Initialize()
        {
            _cts = new CancellationTokenSource();
            
            // Запускаем асинхронный луп и "забываем" (он работает в фоне)
            RunWavesLoopAsync(_cts.Token).Forget();
        }

        // Вызывается автоматически при выгрузке сцены или Game Over (аналог OnDestroy)
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private async UniTaskVoid RunWavesLoopAsync(CancellationToken ct)
        {
            try
            {
#if UNITY_EDITOR
                Debug.Log("<color=cyan>[WaveStateController] Режиссер начал работу (UniTask).</color>");
#endif
                // --- ИСПРАВЛЕНИЕ UI ---
                // Даем Canvas ровно 100 миллисекунд на старте сцены, чтобы он успел инициализировать 
                // все вложенные Vertical/Horizontal Layout Group и ContentSizeFitter.
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);
                // ----------------------
                // 1. Ждем, пока на карте появится хотя бы одна база
                await UniTask.WaitUntil(() => _baseRegistry.ActiveBases.Any(), cancellationToken: ct);

                while (_waveProvider.HasNextWave())
                {
                    _currentWaveNumber++;
                    WaveData currentWave = _waveProvider.GetNextWave();
                    
                    // Обновляем UI волны и прогноз врагов
                    UpdateWaveUI(currentWave);

                    // 2. Фаза ожидания перед волной (Таймер или Зачистка)
                    await HandleWaveDelayAsync(currentWave, ct);

                    // 3. Фаза спавна
                    _signalBus.Fire(new SignalWaveTimerUpdated { TimeLeft = 0, Progress = 1f });
#if UNITY_EDITOR
                    Debug.Log($"<color=green>[WaveStateController] СТАРТ ВОЛНЫ {_currentWaveNumber}!</color>");
#endif
                    await _spawnerService.SpawnWaveAsync(currentWave, ct);

                    // 4. Активная фаза (Пауза, чтобы игроки успели повоевать, прежде чем начнется новый таймер)
                    if (currentWave.ActiveWaveDuration > 0)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(currentWave.ActiveWaveDuration), cancellationToken: ct);
                    }
                }

#if UNITY_EDITOR
                Debug.Log("<color=green>[WaveStateController] ВСЕ ВОЛНЫ ПРОЙДЕНЫ! ПОБЕДА!</color>");
#endif
            }
            catch (OperationCanceledException)
            {
                // Если сработал Dispose (база уничтожена / вышли в меню), код безопасно выпрыгнет сюда
#if UNITY_EDITOR
                Debug.Log("<color=orange>[WaveStateController] Цикл волн прерван (Canceled).</color>");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WaveStateController] Ошибка: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async UniTask HandleWaveDelayAsync(WaveData wave, CancellationToken ct)
        {
            if (wave.StartMode == WaveStartMode.TimeAfterPrevious)
            {
                // Ждем таймер. Если вернулось больше 0, значит нажали Скип.
                float timeLeft = await _timerService.WaitTimeOrSkipAsync(wave.DelayBeforeWave, ct);
                
                if (timeLeft > 0f)
                {
                    int rewardMoney = Mathf.CeilToInt(timeLeft) * 5;
#if UNITY_EDITOR
                    Debug.Log($"<color=yellow>[WaveStateController] Досрочный старт! Выдана награда: {rewardMoney} монет.</color>");
#endif
                    _bankService.AddMoney(rewardMoney);
                }
            }
            else if (wave.StartMode == WaveStartMode.StrictClear)
            {
                if (_currentWaveNumber > 1)
                {
#if UNITY_EDITOR
                    Debug.Log($"<color=cyan>[WaveStateController] Волна {_currentWaveNumber} ждет зачистки карты...</color>");
#endif
                    bool wasSkipped = await _timerService.WaitConditionOrSkipAsync(() => _enemyTracker.IsMapClear, ct);
                    if (wasSkipped)
                    {
#if UNITY_EDITOR
                        Debug.Log("<color=yellow>[WaveStateController] Игрок не стал ждать зачистки и вызвал волну досрочно!</color>");
#endif
                    }
                }
            }
        }

        private void UpdateWaveUI(WaveData wave)
        {
            _signalBus.Fire(new SignalWaveStateChanged
            {
                CurrentWave = _currentWaveNumber,
                TotalWaves = _waveProvider.TotalWaves
            });

            Dictionary<string, int> forecast = new Dictionary<string, int>();
            foreach (var squad in wave.Squads)
            {
                if (forecast.ContainsKey(squad.EnemyId)) forecast[squad.EnemyId] += squad.Count;
                else forecast[squad.EnemyId] = squad.Count;
            }
            
            _signalBus.Fire(new SignalWaveForecastUpdated { EnemyCounts = forecast });
        }
    }
}