using UnityEngine;
using System;
using Zenject;
using Gameplay.Infrastructure.Signals;


namespace Gameplay.Enemies.Services
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
        }
        private void OnEnemyKilled()
        {
            DecreaseCount("убит");
        }

        private void OnEnemyReachedBase()
        {
            DecreaseCount("прошел на базу");
            Gameplay.Tools.GameLogger.Log($"[Tracker] Враг прошел на базу. Живых: {_aliveCount}");
        }

        private void DecreaseCount(string reason)
        {
            _aliveCount--;
            Gameplay.Tools.GameLogger.Log($"<color=orange>[Tracker] Враг {reason}. Живых: {_aliveCount}</color>");

            if (_aliveCount < 0)
            {
                Gameplay.Tools.GameLogger.LogError("[EnemyTracker] КРИТИЧЕСКАЯ ОШИБКА! Счетчик врагов упал ниже нуля. Кто-то заспавнился в обход EnemyFactory или умер дважды!");
                _aliveCount = 0;
            }

            if (_aliveCount == 0)
            {
                _signalBus.Fire<SignalAllEnemiesCleared>();
                Gameplay.Tools.GameLogger.Log("<color=cyan>[EnemyTracker] Радар чист! Все враги уничтожены.</color>");
            }
        }
    }
}

