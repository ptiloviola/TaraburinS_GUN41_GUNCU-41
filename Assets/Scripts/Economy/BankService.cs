using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using System;
using Gameplay.Modifiers.Services;
using Gameplay.Modifiers.Enums;
using Gameplay.Combat.Data;

namespace Gameplay.Economy
{
    
    public class BankService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly LevelRuntimeModel _runtimeModel;
        private readonly StatsModifierService _modifierService;
        private int _balance;

        public int CurrentBalance => _balance;

        public BankService(
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
            
            float startingMoneyMultiplier = _modifierService.GetMultiplier(StatType.StartingMoney);
            

            _balance = Mathf.RoundToInt(_runtimeModel.StartingMoney * startingMoneyMultiplier);

            _signalBus.Subscribe<SignalEnemyKilled>(OnEnemyKilled);
            _signalBus.Fire(new SignalBalanceChanged { CurrentBalance = _balance });
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<SignalEnemyKilled>(OnEnemyKilled);
        }

        private void OnEnemyKilled(SignalEnemyKilled signal)
        {
            AddMoney(signal.Reward);
        }

        public void AddMoney(int amount)
        {
            _balance += amount;
            Debug.Log($"<color=yellow>[BankService] Получено {amount} монет. Текущий баланс: {_balance}</color>");
            _signalBus.Fire(new SignalBalanceChanged { CurrentBalance = _balance });
        }

        public bool SpendMoney(int amount)
        {
            if (_balance >= amount)
            {
                _balance -= amount;
                _signalBus.Fire(new SignalBalanceChanged { CurrentBalance = _balance});
                return true;
            }
            return false;
        }
    }
}



