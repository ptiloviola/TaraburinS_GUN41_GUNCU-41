using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VacuumSim.Trash;
using VacuumSim.Pathfinding;

namespace VacuumSim.Spawning
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Связи")]
        [Tooltip("Конфиг с расписанием волн")]
        [SerializeField] private TrashWavesConfig _config;
        
        [Tooltip("Зона, которая ищет чистые точки на полу")]
        [SerializeField] private SpawnArea _spawnArea;
        [Tooltip("Матрица навигации для обновления чистоты")]
        [SerializeField] private PathfindingGrid _grid;

        // Токен для безопасной остановки асинхронных задач при выходе из игры
        private CancellationTokenSource _cts;

        private void Start()
        {
            if (_config == null || _spawnArea == null)
            {
                Debug.LogError("[WaveManager] Не назначены ссылки в инспекторе!");
                return;
            }

            _cts = new CancellationTokenSource();
            
            // Запускаем бесконечный процесс волн и "забываем" (Forget) про него, 
            // так как он работает в фоне
            RunWavesAsync(_cts.Token).Forget();
        }

        private async UniTask RunWavesAsync(CancellationToken token)
        {
            int waveIndex = 1;

            while (!token.IsCancellationRequested)
            {
                foreach (var wave in _config.Waves)
                {
                    Debug.Log($"[WaveManager] Подготовка к Волне {waveIndex}. Ждем {wave.DelayBeforeWave} сек...");
                    
                    // Ждем задержку перед волной
                    await UniTask.Delay(TimeSpan.FromSeconds(wave.DelayBeforeWave), cancellationToken: token);

                    Debug.Log($"[WaveManager] --- ВОЛНА {waveIndex} НАЧАЛАСЬ! Длительность: {wave.Duration} сек ---");
                    
                    // Запускаем спавн всех элементов волны и ждем её полного окончания
                    await SpawnWaveContentAsync(wave, token);
                    
                    waveIndex++;
                }

                if (!_config.LoopWaves) 
                {
                    Debug.Log("[WaveManager] Уровень завершен. Все волны пройдены.");
                    break;
                }
                
                Debug.Log("[WaveManager] Рестарт цикла волн!");
            }
        }

        private async UniTask SpawnWaveContentAsync(TrashWave wave, CancellationToken token)
        {
            // Список задач, чтобы запустить их параллельно
            var spawnRoutines = new List<UniTask>();

            foreach (var spawnTask in wave.SpawnTasks)
            {
                // Добавляем процесс спавна конкретного типа мусора в общий пул задач
                spawnRoutines.Add(ExecuteSpawnTaskAsync(spawnTask, wave.Duration, token));
            }

            // Ждем, пока ВСЕ задачи (бумажки, пыль и т.д.) не закончат спавниться
            await UniTask.WhenAll(spawnRoutines);
        }

        private async UniTask ExecuteSpawnTaskAsync(TrashSpawnTask task, float duration, CancellationToken token)
        {
            if (task.Amount <= 0 || task.TrashType == null) return;

            if (task.Mode == SpawnMode.FixedInterval)
            {
                // Строго через равные промежутки
                float interval = duration / task.Amount;
                for (int i = 0; i < task.Amount; i++)
                {
                    if (token.IsCancellationRequested) return;
                    
                    SpawnTrash(task.TrashType);
                    await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: token);
                }
            }
            else if (task.Mode == SpawnMode.Randomized)
            {
                // Генерируем случайные метки времени в пределах длительности волны
                List<float> spawnTimes = new List<float>();
                for (int i = 0; i < task.Amount; i++)
                {
                    spawnTimes.Add(UnityEngine.Random.Range(0f, duration));
                }
                spawnTimes.Sort(); // Сортируем от меньшего к большему

                float currentTime = 0f;
                foreach (var timeTarget in spawnTimes)
                {
                    if (token.IsCancellationRequested) return;

                    float delayToNext = timeTarget - currentTime;
                    await UniTask.Delay(TimeSpan.FromSeconds(delayToNext), cancellationToken: token);
                    
                    SpawnTrash(task.TrashType);
                    currentTime = timeTarget;
                }
            }
        }

        private void SpawnTrash(TrashType trashType)
        {
            // Делаем несколько попыток найти чистую точку. 
            // Если комната завалена мебелью, луч может часто попадать в столы.
            const int MAX_ATTEMPTS = 10;
            
            for (int i = 0; i < MAX_ATTEMPTS; i++)
            {
                if (_spawnArea.TryGetValidSpawnPoint(out Vector3 point))
                {
                    // Точка найдена! 
                    // Чуть приподнимаем мусор над полом, чтобы он не застрял в текстурах
                    Vector3 spawnPos = point + new Vector3(0, 0.05f, 0);
                    
                    Instantiate(trashType.Prefab, spawnPos, Quaternion.identity);
                    // --- ИНТЕГРАЦИЯ С ИИ ---
                    // Находим ячейку, в которую упал мусор, и говорим роботу, что она грязная!
                    if (_grid != null)
                    {
                        Node dirtyNode = _grid.NodeFromWorldPoint(point);
                        if (dirtyNode != null)
                        {
                            dirtyNode.IsCleaned = false;
                            dirtyNode.HasTrash = true;
                            Debug.Log($"[WaveManager] Мусор испачкал ячейку [{dirtyNode.GridX}, {dirtyNode.GridY}]!");
                        }
                    }
                    return; // Успешно создали, выходим из метода
                }
            }

            Debug.LogWarning($"[WaveManager] Не удалось найти место для {trashType.Title} за {MAX_ATTEMPTS} попыток.");
        }

        private void OnDestroy()
        {
            // Обязательно глушим все асинхронные задачи при удалении объекта
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }
    }
}