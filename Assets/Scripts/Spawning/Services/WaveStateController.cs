using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Spawning.Data;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Enemies.Services;


namespace Gameplay.Spawning.Services
{
    public class WaveStateController : IDisposable
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
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }

        public async UniTaskVoid RunWavesLoopAsync(CancellationToken ct)
        {
            try
            {
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.Log("<color=cyan>[WaveStateController] Режиссер начал работу (UniTask).</color>");
#endif
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);

                await UniTask.WaitUntil(() => _baseRegistry.ActiveBases.Any(), cancellationToken: ct);

                while (_waveProvider.HasNextWave())
                {
                    _currentWaveNumber++;
                    WaveData currentWave = _waveProvider.GetNextWave();
                    
                    UpdateWaveUI(currentWave);

                    await HandleWaveDelayAsync(currentWave, ct);

                    _signalBus.Fire(new SignalWaveTimerUpdated { TimeLeft = 0, Progress = 1f });
#if UNITY_EDITOR
                    Gameplay.Tools.GameLogger.Log($"<color=green>[WaveStateController] СТАРТ ВОЛНЫ {_currentWaveNumber}!</color>");
#endif
                    await _spawnerService.SpawnWaveAsync(currentWave, ct);

                    if (currentWave.ActiveWaveDuration > 0)
                    {
                        await UniTask.WhenAny(
                            UniTask.Delay(TimeSpan.FromSeconds(currentWave.ActiveWaveDuration), cancellationToken: ct),
                            UniTask.WaitUntil(() => _enemyTracker.IsMapClear, cancellationToken: ct)
                        );
                    }
                }

                _signalBus.Fire<SignalAllWavesSpawned>();
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.Log("<color=green>[WaveStateController] ВСЕ ВОЛНЫ СПАВНЕРА ВЫШЛИ!</color>");
#endif

            }
            catch (OperationCanceledException)
            {
#if UNITY_EDITOR
                Gameplay.Tools.GameLogger.Log("<color=orange>[WaveStateController] Цикл волн прерван (Canceled).</color>");
#endif
            }
            catch (Exception ex)
            {
                Gameplay.Tools.GameLogger.LogError($"[WaveStateController] Ошибка: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private async UniTask HandleWaveDelayAsync(WaveData wave, CancellationToken ct)
        {
            if (wave.StartMode == WaveStartMode.TimeAfterPrevious)
            {
                float timeLeft = await _timerService.WaitTimeOrSkipAsync(wave.DelayBeforeWave, ct);
                
                if (timeLeft > 0f)
                {
                    int rewardMoney = Mathf.CeilToInt(timeLeft) * 5;
#if UNITY_EDITOR
                    Gameplay.Tools.GameLogger.Log($"<color=yellow>[WaveStateController] Досрочный старт! Выдана награда: {rewardMoney} монет.</color>");
#endif
                    _bankService.AddMoney(rewardMoney);
                }
            }
            else if (wave.StartMode == WaveStartMode.StrictClear)
            {
                if (_currentWaveNumber > 1)
                {
#if UNITY_EDITOR
                    Gameplay.Tools.GameLogger.Log($"<color=cyan>[WaveStateController] Волна {_currentWaveNumber} ждет зачистки карты...</color>");
#endif
                    bool wasSkipped = await _timerService.WaitConditionOrSkipAsync(() => _enemyTracker.IsMapClear, ct);
                    if (wasSkipped)
                    {
#if UNITY_EDITOR
                        Gameplay.Tools.GameLogger.Log("<color=yellow>[WaveStateController] Игрок не стал ждать зачистки и вызвал волну досрочно!</color>");
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