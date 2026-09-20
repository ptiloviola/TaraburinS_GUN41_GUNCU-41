using UnityEngine;
using System;
using Zenject;
using Gameplay.Infrastructure.Signals;


namespace Gameplay.Enemies
{
    public class EnemyTrackerService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private int _aliveCount;
        public int AliveCount => _aliveCount;
        public bool IsMapClear => _aliveCount <= 0;

        public EnemyTrackerService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _aliveCount = 0;
            _signalBus.Subscribe<SignalEnemySpawned>(OnEnemySpawned);
            _signalBus.Subscribe<SignalEnemyKilled>(OnEnemyKilled);
            _signalBus.Subscribe<SignalEnemyReachedBase>(OnEnemyReachedBase);
        }

        public void Dispose()
        {
            _signalBus.TryUnsubscribe<SignalEnemySpawned>(OnEnemySpawned);
            _signalBus.TryUnsubscribe<SignalEnemyKilled>(OnEnemyKilled);
            _signalBus.TryUnsubscribe<SignalEnemyReachedBase>(OnEnemyReachedBase);
        }

        private void OnEnemySpawned()
        {
            _aliveCount ++;
            Debug.Log($"[Tracker] Враг родился. Живых: {_aliveCount}");
        }
        private void OnEnemyKilled()
        {
            DecreaseCount();
            Debug.Log($"[Tracker] Враг убит. Живых: {_aliveCount}");
        }

        private void OnEnemyReachedBase()
        {
            DecreaseCount();
            Debug.Log($"[Tracker] Враг прошел на базу. Живых: {_aliveCount}");
        }

        private void DecreaseCount()
        {
            _aliveCount--;
            if (_aliveCount < 0) _aliveCount = 0;
            if (_aliveCount == 0)
            {
                _signalBus.Fire<SignalAllEnemiesCleared>();
                Debug.Log("<color=cyan>[EnemyTracker] Радар чист! Все враги уничтожены.</color>");
            }
        }
    }
}

