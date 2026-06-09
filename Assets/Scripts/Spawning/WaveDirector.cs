using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Spawning.Data;

namespace Gameplay.Spawning
{
    public class WaveDirector : MonoBehaviour
    {
        [Header("Для теста перетащи сюда SO")]
        [SerializeField] private LevelWavesConfig _testConfig;

        private IWaveProvider _waveProvider;
        private int _currentWaveNumber = 0;

        private void Start()
        {
            // В будущем мы будем инжектить провайдер через Zenject.
            // Сейчас собираем его вручную для быстрого теста.
            _waveProvider = new StaticWaveProvider(_testConfig);
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
                
                // Состояние 2: Спавн отрядов
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
                    
                    yield return new WaitForSeconds(currentSquad.SpawnInterval);
                }
            }
        }

    }
}

