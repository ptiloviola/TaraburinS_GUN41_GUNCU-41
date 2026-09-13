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
            // Подписываемся на события жизни и смерти
            _signalBus.Subscribe<SignalEnemySpawned>(OnEnemySpawned);
            _signalBus.Subscribe<SignalEnemyKilled>(OnEnemyKilled);
            // ИСПРАВЛЕНО: Слушаем правильный сигнал!
            _signalBus.Subscribe<SignalEnemyReachedBase>(OnEnemyReachedBase);
        }

        public void Dispose()
        {
            // Отписываемся, чтобы избежать утечек памяти
            _signalBus.TryUnsubscribe<SignalEnemySpawned>(OnEnemySpawned);
            _signalBus.TryUnsubscribe<SignalEnemyKilled>(OnEnemyKilled);
            // ИСПРАВЛЕНО
            _signalBus.TryUnsubscribe<SignalEnemyReachedBase>(OnEnemyReachedBase);
        }

        private void OnEnemySpawned()
        {
            _aliveCount ++;
            // Можно раскомментить для теста
            Debug.Log($"[Tracker] Враг родился. Живых: {_aliveCount}");
        }
        private void OnEnemyKilled()
        {
            DecreaseCount();
            Debug.Log($"[Tracker] Враг убит. Живых: {_aliveCount}");
        }

        private void OnEnemyReachedBase()
        {
            // У нас враг исчезает, когда бьет базу (enemy.Despawn() в BaseCore), 
            // значит его тоже нужно вычесть из списка живых!
            DecreaseCount();
            Debug.Log($"[Tracker] Враг прошел на базу. Живых: {_aliveCount}");
        }

        private void DecreaseCount()
        {
            _aliveCount--;
            // Защита от багов (на всякий случай, чтобы счетчик не ушел в минус)
            if (_aliveCount < 0) _aliveCount = 0;
            if (_aliveCount == 0)
            {
                // Как только врагов стало 0, кричим об этом на всю игру!
                _signalBus.Fire<SignalAllEnemiesCleared>();
                Debug.Log("<color=cyan>[EnemyTracker] Радар чист! Все враги уничтожены.</color>");
            }
        }
    }
}

