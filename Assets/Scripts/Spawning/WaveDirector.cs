using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Spawning.Data;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Base;
using Infrastructure.Signals;
using Gameplay.Economy;

namespace Gameplay.Spawning
{
    public class WaveDirector : MonoBehaviour
    {
        [Header("Настройки уровня")]
        [SerializeField] private LevelWavesConfig _levelConfig;


        private IWaveProvider _waveProvider;
        // Заменяем старый SpawnLocationService на новый SpawnRegistry
        private SpawnRegistry _spawnRegistry;
        private BaseRegistry _baseRegistry; // ИСПРАВЛЕНО: Вместо BaseCore внедряем реестр
        private SignalBus _signalBus;
        private DiContainer _container; // Нужен, чтобы доставать пулы по ID
        private BankService _bankService;

        private int _currentWaveNumber = 0;

        private bool _isForceStartRequested = false;


        [Inject]
        public void Construct(SpawnRegistry spawnRegistry, 
            BaseRegistry baseRegistry, SignalBus signalBus, DiContainer container,
            BankService bankService)
        {
            _spawnRegistry = spawnRegistry;
            _baseRegistry = baseRegistry;
            _signalBus = signalBus;
            _container = container;
            _bankService = bankService;
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
            _signalBus.TryUnsubscribe<SignalForceStartWave>(OnForceStartRequested);
        }

        private void OnForceStartRequested()
        {
            _isForceStartRequested = true;
        }

        // Главный цикл (State Machine на базе корутины)
        private IEnumerator DirectorRoutine()
        {
            Debug.Log("<color=cyan>[Director] Режиссер начал работу.</color>");
            // (проверяем каждые 0.1 сек, чтобы не вешать игру)
            while (_baseRegistry.GetBaseById("", Vector3.zero) == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            // Получаем реальное количество волн из конфига!
            int totalWaves = _levelConfig.Waves.Count;
            while (_waveProvider.HasNextWave())
            {
                _currentWaveNumber++;
                WaveData currentWave = _waveProvider.GetNextWave();
                // Состояние 1: Ожидание начала волны
                Debug.Log($"<color=yellow>[Director] Волна {_currentWaveNumber} начнется через {currentWave.DelayBeforeWave} сек...</color>");
                // yield return new WaitForSeconds(currentWave.DelayBeforeWave);
                
                // Обновляем текст в UI (Волна 1 из 5)
                _signalBus.Fire(new SignalWaveStateChanged
                {
                    CurrentWave = _currentWaveNumber,
                    // Пока заглушка или можно брать из конфига
                    TotalWaves = totalWaves
                });
                // --- УМНЫЙ ТАЙМЕР ОЖИДАНИЯ ---
                float timer = currentWave.DelayBeforeWave;
                float totalTime = timer;
                // Сбрасываем флаг перед каждой волной
                _isForceStartRequested = false;
                // Крутимся в цикле, пока есть время И игрок не нажал кнопку досрочного старта
                while (timer > 0 && !_isForceStartRequested)
                {
                    timer -= Time.deltaTime;
                    // Сообщаем UI, сколько времени осталось
                    _signalBus.Fire(new SignalWaveTimerUpdated
                    {
                        TimeLeft = Mathf.Max(0, timer),
                        Progress = 1f - (timer / totalTime)
                    });
                    // Ждем один кадр
                    yield return null;
                }

                // --- ЛОГИКА НАГРАДЫ ЗА ДОСРОЧНЫЙ СТАРТ ---
                if (_isForceStartRequested)
                {
                    // Например, 5 золота за каждую сэкономленную секунду
                    int rewardMoney = Mathf.CeilToInt(timer) * 5;
                    Debug.Log($"<color=yellow>[WaveDirector] Досрочный старт! Выдана награда: {rewardMoney} монет.</color>");
                    
                    _bankService.AddMoney(rewardMoney);
                    // Сбрасываем таймер в UI на 0
                    _signalBus.Fire(new SignalWaveTimerUpdated
                    {
                        TimeLeft = 0,
                        Progress = 1f
                    });
                }
                
                // Вызываем сигнал начала волны
                // _signalBus.Fire(new SignalWaveStarted { ... });
                Debug.Log($"<color=green>[Director] СТАРТ ВОЛНЫ {_currentWaveNumber}!</color>");
                yield return StartCoroutine(SpawnWaveRoutine(currentWave));
                
                // Состояние 3: Ожидание зачистки
                // Пока просто имитируем, что игрок убил всех за 3 секунды
                Debug.Log($"<color=orange>[Director] Все враги выпущены. Ждем зачистки карты...</color>");
                // Заглушка до реализации учета живых врагов
                yield return new WaitForSeconds(3f);

                Debug.Log($"<color=cyan>[Director] Волна {_currentWaveNumber} зачищена! Награда: {currentWave.ClearReward}</color>");

            }
            Debug.Log("<color=green>[Director] ВСЕ ВОЛНЫ ПРОЙДЕНЫ! ПОБЕДА!</color>");
        }

        private IEnumerator SpawnWaveRoutine(WaveData wave)
        {
            // Распаковываем отряды в очередь (чтобы в будущем можно было добавлять их на лету)
            Queue<SquadData> squadQueue = new Queue<SquadData>(wave.Squads);

            while (squadQueue.Count > 0)
            {
                SquadData currentSquad = squadQueue.Dequeue();
                Debug.Log($"[Director] Выходит отряд: {currentSquad.Count}x {currentSquad.EnemyId} (Точка: {currentSquad.SpawnPointId})");
                for (int i = 0; i < currentSquad.Count; i++)
                {
                    // ЗДЕСЬ БУДЕТ РЕАЛЬНЫЙ СПАВН ИЗ ПУЛА
                    Debug.Log($"   -> Спавн {currentSquad.EnemyId} ({i + 1}/{currentSquad.Count})");
                    // Вызываем наш новый умный метод физического спавна
                    SpawnPhysicalEnemy(currentSquad.EnemyId, currentSquad.SpawnPointId, currentSquad.TargetBaseId);
                    yield return new WaitForSeconds(currentSquad.SpawnInterval);
                }
            }
        }

        private void SpawnPhysicalEnemy(string enemyId, string spawnPointId, string targetBaseId)
        {
            // 1. Ищем спавн в реестре
            if (!_spawnRegistry.TryGetSpawnPosition(spawnPointId, out Vector3 spawnPos))
            {
                Debug.LogWarning($"[Director] Спавн '{spawnPointId}' не найден, кидаем в 0,0,0");
                spawnPos = Vector3.zero; // Если точка не найдена, кидаем в центр
            }
            try
            {
                // 2. Узнаем КОГО спавнить (ищем пул с нужным ID)
                EnemyFacade.Pool specificPool = _container.ResolveId<EnemyFacade.Pool>(enemyId);
                EnemyFacade enemy = specificPool.Spawn();
                
                // НОВОЕ: Передаем врагу ЕГО ЛИЧНЫЙ ПУЛ!
                enemy.SetPool(specificPool);
                // 3. Ставим на точку и даем пинок в сторону базы (твой идеальный код!)
                var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                {
                    agent.Warp(spawnPos);
                }
                // 2. Ищем КОНКРЕТНУЮ базу из отряда
                // ИСПРАВЛЕНИЕ: Передаем координаты спавна (spawnPos) для расчета дистанции!
                BaseCore targetBase = _baseRegistry.GetBaseById(targetBaseId, spawnPos);
                if (targetBase != null)
                {
                    // Временно создаем стратегию здесь. В идеале база должна сама отдавать свои координаты.
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

