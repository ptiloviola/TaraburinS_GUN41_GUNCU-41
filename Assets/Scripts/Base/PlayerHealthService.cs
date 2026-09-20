using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Levels.Data;

namespace Gameplay.Base
{
    public class PlayerHealthService : IInitializable
    {
        private readonly SignalBus _signalBus;
        private readonly LevelRuntimeModel _runtimeModel;
        private int _globalLives;
        
        public int CurrentLives => _globalLives;

        public PlayerHealthService(SignalBus signalBus, LevelRuntimeModel runtimeModel)
        {
            _signalBus = signalBus;
            _runtimeModel = runtimeModel;
        }

        public void Initialize()
        {
            _globalLives = _runtimeModel.StartingLives;
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
            }
        }
    }
}