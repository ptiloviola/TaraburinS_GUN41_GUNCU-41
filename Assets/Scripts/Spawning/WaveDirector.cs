using System.Collections;
using System.Collections.Generic;
using System.Linq; // НОВОЕ: Для проверки Any()
using UnityEngine;
using Gameplay.Spawning.Data;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Base;
using Infrastructure.Signals;
using Gameplay.Economy;
using Gameplay.Enemies.Data;

namespace Gameplay.Spawning
{
    public class WaveDirector : MonoBehaviour
    {
        [Header("Настройки уровня")]
        [SerializeField] private LevelWavesConfig _levelConfig;

        private IWaveProvider _waveProvider;
        private SpawnRegistry _spawnRegistry;
        private BaseRegistry _baseRegistry; // ВЕРНУЛИ: Чтобы проверять, есть ли базы на карте
        private BaseLocatorService _baseLocatorService; 
        private SignalBus _signalBus;
        private DiContainer _container; 
        private BankService _bankService;
        private EnemyTrackerService _enemyTracker; 

        private int _currentWaveNumber = 0;
        private bool _isForceStartRequested = false;
        private EnemyRegistry _enemyRegistry;

        [Inject]
        public void Construct(SpawnRegistry spawnRegistry, 
            BaseRegistry baseRegistry, // ДОБАВЛЕНО
            BaseLocatorService baseLocatorService, SignalBus signalBus, DiContainer container,
            BankService bankService, EnemyTrackerService enemyTracker, 
            EnemyRegistry enemyRegistry)
        {
            _spawnRegistry = spawnRegistry;
            _baseRegistry = baseRegistry;
            _baseLocatorService = baseLocatorService;
            _signalBus = signalBus;
            _container = container;
            _bankService = bankService;
            _enemyTracker = enemyTracker;
            _enemyRegistry = enemyRegistry;
        }

        private void Start()
        {
            if (_levelConfig == null) return;
            _waveProvider = new StaticWaveProvider(_levelConfig);
            StartCoroutine(DirectorRoutine());
        }

        private void OnEnable()
        {
            _signalBus.Subscribe<SignalForceStartWave>(OnForceStartRequested);
        }

        private void OnDisable()
        {
            // ИСПРАВЛЕНО: Безопасная отписка, если _signalBus не успел заинжектиться
            _signalBus?.TryUnsubscribe<SignalForceStartWave>(OnForceStartRequested);
        }

        private void OnForceStartRequested()
        {
            _isForceStartRequested = true;
        }

        private IEnumerator DirectorRoutine()
        {
            Debug.Log("<color=cyan>[Director] Режиссер начал работу.</color>");
            
            // ИСПРАВЛЕНО: Ждем, пока в реестре не появится хотя бы одна база
            while (!_baseRegistry.ActiveBases.Any())
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            int totalWaves = _levelConfig.Waves.Count;
            while (_waveProvider.HasNextWave())
            {
                _currentWaveNumber++;
                WaveData currentWave = _waveProvider.GetNextWave();
                Debug.Log($"<color=yellow>[Director] Волна {_currentWaveNumber} начнется через {currentWave.DelayBeforeWave} сек...</color>");
                
                _signalBus.Fire(new SignalWaveStateChanged
                {
                    CurrentWave = _currentWaveNumber,
                    TotalWaves = totalWaves
                });

                Dictionary<string, int> forecast = new Dictionary<string, int>();
                foreach (var squad in currentWave.Squads)
                {
                    if (forecast.ContainsKey(squad.EnemyId))
                        forecast[squad.EnemyId] += squad.Count;
                    else
                        forecast[squad.EnemyId] = squad.Count;
                }
                
                _signalBus.Fire(new SignalWaveForecastUpdated { EnemyCounts = forecast });

                _isForceStartRequested = false;

                if (currentWave.StartMode == WaveStartMode.TimeAfterPrevious)
                {
                    float timer = currentWave.DelayBeforeWave;
                    float totalTime = timer;
                    while (timer > 0 && !_isForceStartRequested)
                    {
                        timer -= Time.deltaTime;
                        _signalBus.Fire(new SignalWaveTimerUpdated
                        {
                            TimeLeft = Mathf.Max(0, timer),
                            Progress = 1f - (timer / totalTime)
                        });
                        yield return null;
                        
                        if (_isForceStartRequested)
                        {
                            int rewardMoney = Mathf.CeilToInt(timer) * 5;
                            Debug.Log($"<color=yellow>[WaveDirector] Досрочный старт! Выдана награда: {rewardMoney} монет.</color>");
                            
                            _bankService.AddMoney(rewardMoney);
                            _signalBus.Fire(new SignalWaveTimerUpdated
                            {
                                TimeLeft = 0,
                                Progress = 1f
                            });
                        }
                    }
                }
                else if (currentWave.StartMode == WaveStartMode.StrictClear)
                {
                    _signalBus.Fire(new SignalWaveTimerUpdated { TimeLeft = 0, Progress = 1f }); 

                    if (_currentWaveNumber > 1)
                    {
                        Debug.Log($"<color=cyan>[Director] Волна {_currentWaveNumber} ждет зачистки карты...</color>");
                        while (!_enemyTracker.IsMapClear && !_isForceStartRequested)
                        {
                            yield return null;
                        }
                    }
                    if (_isForceStartRequested)
                    {
                        Debug.Log("<color=yellow>[Director] Игрок не стал ждать зачистки и вызвал волну досрочно!</color>");
                    }
                }

                _signalBus.Fire(new SignalWaveTimerUpdated { TimeLeft = 0, Progress = 1f }); 
                Debug.Log($"<color=green>[Director] СТАРТ ВОЛНЫ {_currentWaveNumber}!</color>");
                
                yield return StartCoroutine(SpawnWaveRoutine(currentWave));

                if (currentWave.ActiveWaveDuration > 0)
                {
                    yield return new WaitForSeconds(currentWave.ActiveWaveDuration);
                }
            }
            Debug.Log("<color=green>[Director] ВСЕ ВОЛНЫ ПРОЙДЕНЫ! ПОБЕДА!</color>");
        }

        private IEnumerator SpawnWaveRoutine(WaveData wave)
        {
            Queue<SquadData> squadQueue = new Queue<SquadData>(wave.Squads);

            while (squadQueue.Count > 0)
            {
                SquadData currentSquad = squadQueue.Dequeue();
                float warningTime = 2f; 
                _spawnRegistry.TriggerWarning(currentSquad.SpawnPointId, warningTime);
                
                yield return new WaitForSeconds(warningTime);
                
                Debug.Log($"[Director] Выходит отряд: {currentSquad.Count}x {currentSquad.EnemyId} (Точка: {currentSquad.SpawnPointId})");
                for (int i = 0; i < currentSquad.Count; i++)
                {
                    Debug.Log($"   -> Спавн {currentSquad.EnemyId} ({i + 1}/{currentSquad.Count})");
                    SpawnPhysicalEnemy(currentSquad.EnemyId, currentSquad.SpawnPointId, currentSquad.TargetBaseId);
                    yield return new WaitForSeconds(currentSquad.SpawnInterval);
                }
            }
        }

        private void SpawnPhysicalEnemy(string enemyId, string spawnPointId, string targetBaseId)
        {
            if (!_spawnRegistry.TryGetSpawnPosition(spawnPointId, out Vector3 spawnPos))
            {
                Debug.LogWarning($"[Director] Спавн '{spawnPointId}' не найден, кидаем в 0,0,0");
                spawnPos = Vector3.zero; 
            }
            try
            {
                EnemyConfig config = _enemyRegistry.GetEnemyById(enemyId);
                if (config == null)
                {
                    Debug.LogError($"[Director] Враг '{enemyId}' не найден в EnemyRegistry!");
                    return;
                }
                
                EnemyFacade.Pool specificPool = _container.ResolveId<EnemyFacade.Pool>(enemyId);
                EnemyFacade enemy = specificPool.Spawn();
                
                enemy.SetPool(specificPool);
                enemy.InitConfig(config);
                _signalBus.Fire<SignalEnemySpawned>();
                
                var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(spawnPos);
                }
                
                // ИСПРАВЛЕНО: Запрашиваем базу у нового локатора!
                BaseCore targetBase = _baseLocatorService.LocateTargetBase(targetBaseId, spawnPos);
                
                if (targetBase != null)
                {
                    IMovementStrategy movement = new NavMeshMovement(targetBase.transform.position);
                    enemy.InitializeMovement(movement);
                }
                else
                {
                    Debug.LogError("[Director] Ошибка! Враг заспавнен, но в реестре BaseRegistry нет ни одной активной базы для атаки!");
                }
            }
            catch (ZenjectException)
            {
                Debug.LogError($"[Director] Ошибка спавна! Пул для врага '{enemyId}' не найден. Проверь EnemyRegistry и Installer!");
            }
        }
    }
}