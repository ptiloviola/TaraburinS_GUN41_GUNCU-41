using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MeatMushrooms.Mushroom.Configs;
using MeatMushrooms.Mushroom.Signals;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

namespace MeatMushrooms.Mushroom.Components
{
    public class MushroomSpawner : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly MushroomSpawnerConfig _spawnerConfig;
        private readonly MushroomConfig _mushroomConfig;
        private readonly IInstantiator _instantiator;
        
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _currentMushroomCount = 0;

        int _groundLayerMask = LayerMask.GetMask("Ground");

        [Inject]
        public MushroomSpawner(
            SignalBus signalBus, 
            MushroomSpawnerConfig spawnerConfig, 
            MushroomConfig mushroomConfig, 
            IInstantiator instantiator)
        {
            _signalBus = signalBus;
            _spawnerConfig = spawnerConfig;
            _mushroomConfig = mushroomConfig;
            _instantiator = instantiator;
        }

        public void Initialize()
        {
            // Лог для проверки: вообще запускает ли Zenject наш спавнер?
            Debug.Log("[MushroomSpawner] Сигнал от Zenject получен. Спавнер успешно запущен!");
            
            _signalBus.Subscribe<MushroomDestroyedSignal>(OnMushroomDestroyed);
            SpawnRoutine(_cts.Token).Forget();
        }

        private async UniTaskVoid SpawnRoutine(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Ждем заданный интервал перед попыткой спавна
                await UniTask.Delay(TimeSpan.FromSeconds(_spawnerConfig.SpawnInterval), cancellationToken: token);

                if (_currentMushroomCount >= _spawnerConfig.MaxMushroomsOnMap)
                {
                    continue; // Пропускаем цикл, если грибов уже слишком много
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

            Vector2 randomCircle = Random.insideUnitCircle * _spawnerConfig.SpawnAreaRadius;
            Vector3 rayStartPos = new Vector3(randomCircle.x, 100f, randomCircle.y);

            // Проверка 1: Куда летит луч?
            if (Physics.Raycast(rayStartPos, Vector3.down, out RaycastHit hit, 200f, _groundLayerMask))
            {
                // Если луч во что-то попал, мы увидим имя этого объекта в консоли
                Debug.Log($"[Spawner Debug] 1. Луч попал в объект: '{hit.collider.name}' на позиции {hit.point}");

                // Проверка 2: Есть ли тут NavMesh?
                // Увеличим радиус поиска с 1 метра до 5 метров для теста
                if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
                {
                    validPosition = navHit.position;
                    return true;
                }
                else
                {
                    // Если этот лог сработал, значит земля есть, а NavMesh на ней нет
                    Debug.LogWarning($"[Spawner Debug] 2. В точке {hit.point} под объектом '{hit.collider.name}' НЕ НАЙДЕН запеченный NavMesh!");
                }
            }
            else
            {
                // Если этот лог сработал, значит луч пролетел мимо всей твоей графики в бездну
                Debug.LogWarning($"[Spawner Debug] 1. Луч с позиции {rayStartPos} пролетел мимо и ни во что не попал!");
            }
            
            return false;
        }

        private void SpawnMushroom(Vector3 position)
        {
            GameObject newMushroom = _instantiator.InstantiatePrefab(_mushroomConfig.Prefab, position, Quaternion.identity, null);
            
            if (newMushroom.TryGetComponent(out MushroomEntity entity))
            {
                entity.Init(_mushroomConfig, _signalBus);
                
                // Передаем интерфейсы в сигнал!
                _signalBus.Fire(new MushroomSpawnedSignal 
                { 
                    EdibleComponent = entity.Health, 
                    AromaComponent = entity.Aroma 
                });
            }

            _currentMushroomCount++;
            Debug.Log($"[MushroomSpawner] Гриб вырос! Всего на поляне: {_currentMushroomCount}");
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