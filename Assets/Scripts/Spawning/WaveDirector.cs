using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Spawning.Data;
using Zenject;
using Gameplay.Enemies;
using Gameplay.Base;
using Infrastructure.Signals;

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

        private int _currentWaveNumber = 0;


        [Inject]
        public void Construct(SpawnRegistry spawnRegistry, 
            BaseRegistry baseRegistry, SignalBus signalBus, DiContainer container)
        {
            _spawnRegistry = spawnRegistry;
            _baseRegistry = baseRegistry;
            _signalBus = signalBus;
            _container = container;
        }



        private void Start()
        {
            if (_levelConfig == null) return;
            _waveProvider = new StaticWaveProvider(_levelConfig);
            StartCoroutine(DirectorRoutine());
        }

        // Главный цикл (State Machine на базе корутины)
        private IEnumerator DirectorRoutine()
        {
            Debug.Log("<color=cyan>[Director] Режиссер начал работу.</color>");
            while (_waveProvider.HasNextWave())
            {
                _currentWaveNumber++;
                WaveData currentWave = _waveProvider.GetNextWave();
                // Состояние 1: Ожидание начала волны
                Debug.Log($"<color=yellow>[Director] Волна {_currentWaveNumber} начнется через {currentWave.DelayBeforeWave} сек...</color>");
                yield return new WaitForSeconds(currentWave.DelayBeforeWave);
                
                // Вызываем сигнал начала волны
                // _signalBus.Fire(new SignalWaveStarted { ... });
                Debug.Log($"<color=green>[Director] СТАРТ ВОЛНЫ {_currentWaveNumber}!</color>");
                yield return StartCoroutine(SpawnWaveRoutine(currentWave));
                
                // Состояние 3: Ожидание зачистки
                // Пока просто имитируем, что игрок убил всех за 3 секунды
                Debug.Log($"<color=orange>[Director] Все враги выпущены. Ждем зачистки карты...</color>");
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

