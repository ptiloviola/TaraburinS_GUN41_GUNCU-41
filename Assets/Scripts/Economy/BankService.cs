using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;
using System;

namespace Gameplay.Economy
{
    // IInitializable и IDisposable нужны, чтобы Zenject сам вызвал Start и OnDestroy для подписок
    public class BankService : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private int _balance;

        // Стартовый капитал
        private const int StartingBalance = 100;
        public int CurrentBalance => _balance;

        public BankService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _balance = StartingBalance;

            // Подписываемся на сигнал убийства врага
            _signalBus.Subscribe<SignalEnemyKilled>(OnEnemyKilled);

            // Сразу оповещаем UI о стартовом балансе
            _signalBus.Fire(new SignalBalanceChanged {CurrentBalance = _balance});
        }

        public void Dispose()
        {
            // Обязательно отписываемся при уничтожении, чтобы не было утечек памяти
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
            // Оповещаем UI, что баланс изменился
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



