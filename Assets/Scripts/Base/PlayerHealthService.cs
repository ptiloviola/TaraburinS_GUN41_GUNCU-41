using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.Combat.Data;
using Gameplay.Modifiers.Services;
using Gameplay.Modifiers.Enums;

namespace Gameplay.Base
{
    public class PlayerHealthService : IInitializable
    {
        private readonly SignalBus _signalBus;
        private readonly LevelRuntimeModel _runtimeModel;
        private readonly StatsModifierService _modifierService;
        private int _globalLives;
        
        public int CurrentLives => _globalLives;

        public PlayerHealthService(
            SignalBus signalBus, 
            LevelRuntimeModel runtimeModel,
            StatsModifierService modifierService)
        {
            _signalBus = signalBus;
            _runtimeModel = runtimeModel;
            _modifierService = modifierService;
        }

        public void Initialize()
        {

            float livesMultiplier = _modifierService.GetMultiplier(StatType.StartingLives);
            
            _globalLives = Mathf.RoundToInt(_runtimeModel.StartingLives * livesMultiplier);
            
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