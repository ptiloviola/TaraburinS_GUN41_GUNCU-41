using UnityEngine;
using Zenject;
using Infrastructure.Signals;

namespace Gameplay.Base
{
    public class PlayerHealthService : IInitializable
    {
        private readonly SignalBus _signalBus;
        private int _globalLives;

        private const int StartingLives = 20;
        
        public int CurrentLives => _globalLives;

        public PlayerHealthService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _globalLives = StartingLives;
            _signalBus.Fire(new SignalBaseDamaged { CurrentLives = _globalLives });
        }

        public void TakeGlobalDamage(int amount)
        {
            if (_globalLives <= 0) return; 

            _globalLives -= amount;
            _signalBus.Fire(new SignalBaseDamaged { CurrentLives = _globalLives });
            
#if UNITY_EDITOR
            Debug.Log($"<color=orange>[PlayerHealthService] Пропущен враг! Осталось глобальных жизней: {_globalLives}</color>");
#endif

            if (_globalLives <= 0)
            {
                _signalBus.Fire<SignalGameOver>();
#if UNITY_EDITOR
                Debug.Log("<color=red>[PlayerHealthService] ИГРА ОКОНЧЕНА (GAME OVER)!</color>");
#endif
            }
        }
    }
}