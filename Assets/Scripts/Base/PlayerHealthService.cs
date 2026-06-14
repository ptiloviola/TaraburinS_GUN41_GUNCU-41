using UnityEngine;
using Zenject;
using Infrastructure.Signals;
using System;

namespace Gameplay.Base
{
    public class PlayerHealthService : IInitializable
    {
        private readonly SignalBus _signalBus;
        private int _globalLives;

        // Стартовые общие жизни (потом можно вынести в настройки уровня)
        private const int StartingLives = 20;
        public int CurrentLives => _globalLives;

        public PlayerHealthService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _globalLives = StartingLives;
            // Оповещаем UI о стартовых жизнях
            _signalBus.Fire(new SignalBaseDamaged {CurrentLives = _globalLives });
        }

        public void TakeGlobalDamage(int amount)
        {
            if (_globalLives <= 0) return; // Уже проиграли

            _globalLives -= amount;
            _signalBus.Fire(new SignalBaseDamaged { CurrentLives = _globalLives });
            
            Debug.Log($"<color=orange>[PlayerHealthService] Пропущен враг! Осталось глобальных жизней: {_globalLives}</color>");

            if (_globalLives <= 0)
            {
                _signalBus.Fire<SignalGameOver>();
                Debug.Log("<color=red>[PlayerHealthService] ИГРА ОКОНЧЕНА (GAME OVER)!</color>");
            }
        }


    }
}

