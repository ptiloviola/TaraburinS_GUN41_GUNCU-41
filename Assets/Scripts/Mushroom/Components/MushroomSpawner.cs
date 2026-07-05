using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;
using MeatMushrooms.Environment; // Обязательно для StageConfig
using MeatMushrooms.Core;        // Обязательно для GameSession

namespace MeatMushrooms.Mushroom.Components
{
    public class MushroomSpawner : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly MushroomSpawnerConfig _spawnerConfig;
        private readonly MushroomConfig _mushroomConfig;
        private readonly IInstantiator _instantiator;
        private readonly StageConfig _stageConfig; // Ссылка на глобальный конфиг уровня
        
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _currentMushroomCount = 0;

        int _groundLayerMask = LayerMask.GetMask("Ground");

        // --- ДИНАМИЧЕСКИЕ ПАРАМЕТРЫ ПРОГРЕССИИ ---
        private int _levelBonus => GameSession.CurrentLevel - 1;
        private int _maxMushroomsOnMap => _stageConfig.MushroomCount + (_levelBonus * _stageConfig.MushroomIncrement);
        private float _currentSpawnRadius => (_stageConfig.MapRadius + (_levelBonus * _stageConfig.RadiusIncrement)) * 0.9f;

        [Inject]
        public MushroomSpawner(
            SignalBus signalBus, 
            MushroomSpawnerConfig spawnerConfig, 
            MushroomConfig mushroomConfig, 
            IInstantiator instantiator,
            StageConfig stageConfig) // Инжектим StageConfig через конструктор
        {
            _signalBus = signalBus;
            _spawnerConfig = spawnerConfig;
            _mushroomConfig = mushroomConfig;
            _instantiator = instantiator;
            _stageConfig = stageConfig;
        }

        public void Initialize()
        {
            Debug.Log($"[MushroomSpawner] Запущен! Лимит грибов: {_maxMushroomsOnMap}, Радиус: {_currentSpawnRadius}");
            
            _signalBus.Subscribe<MushroomDestroyedSignal>(OnMushroomDestroyed);
            SpawnRoutine(_cts.Token).Forget();
        }

        private async UniTaskVoid SpawnRoutine(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Берем интервал из старого локального конфига
                await UniTask.Delay(TimeSpan.FromSeconds(_spawnerConfig.SpawnInterval), cancellationToken: token);

                // Используем динамический лимит
                if (_currentMushroomCount >= _maxMushroomsOnMap)
                {
                    continue; 
                }

                if (TryFindSpawnPosition(out Vector3 spawnPos))
                {
                    SpawnMushroom(spawnPos);
                }
            }
        }

        private bool TryFindSpawnPosition(out Vector3 validPosition)
        {
            validPosition = Vector3.zero;

            // Используем динамический радиус поиска
            Vector2 randomCircle = Random.insideUnitCircle * _currentSpawnRadius;
            Vector3 rayStartPos = new Vector3(randomCircle.x, 100f, randomCircle.y);

            if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 200f, _groundLayerMask))
            {
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
                {
                    validPosition = navHit.position;
                    return true;
                }
            }
            
            return false;
        }

        private void SpawnMushroom(Vector3 position)
        {
            GameObject newMushroom = _instantiator.InstantiatePrefab(_mushroomConfig.Prefab, position, Quaternion.identity, null);
            
            if (newMushroom.TryGetComponent(out MushroomEntity entity))
            {
                entity.Init(_mushroomConfig, _signalBus);
                
                _signalBus.Fire(new MushroomSpawnedSignal 
                { 
                    EdibleComponent = entity.Health, 
                    AromaComponent = entity.Aroma 
                });
            }

            _currentMushroomCount++;
            // Можно убрать или оставить лог, если он не спамит
            // Debug.Log($"[MushroomSpawner] Гриб вырос! Всего на поляне: {_currentMushroomCount}");
        }

        private void OnMushroomDestroyed(MushroomDestroyedSignal signal)
        {
            _currentMushroomCount--;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<MushroomDestroyedSignal>(OnMushroomDestroyed);
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}