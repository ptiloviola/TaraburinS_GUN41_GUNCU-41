using System;
using Zenject;
using Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Economy;
using Gameplay.Base;

namespace Gameplay.UI.Presenters
{
    public class BaseUIPresenter : IInitializable, IDisposable
    {
        private readonly BaseUIView _view;
        private readonly SignalBus _signalBus;
        private readonly BankService _bankService;
        private readonly PlayerHealthService _healthService;

        public BaseUIPresenter(
            BaseUIView view, 
            SignalBus signalBus, 
            BankService bankService, 
            PlayerHealthService healthService)
        {
            _view = view;
            _signalBus = signalBus;
            _bankService = bankService;
            _healthService = healthService;
        }

        public void Initialize()
        {
            // 1. Сразу пушим актуальное состояние при загрузке
            _view.SetBalance(_bankService.CurrentBalance);
            _view.SetLives(_healthService.CurrentLives);

            // 2. Подписываемся на обновления
            _signalBus.Subscribe<SignalBalanceChanged>(OnBalanceChanged);
            _signalBus.Subscribe<SignalBaseDamaged>(OnBaseDamaged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<SignalBalanceChanged>(OnBalanceChanged);
            _signalBus.Unsubscribe<SignalBaseDamaged>(OnBaseDamaged);
        }

        private void OnBalanceChanged(SignalBalanceChanged signal)
        {
            _view.SetBalance(signal.CurrentBalance);
        }

        private void OnBaseDamaged(SignalBaseDamaged signal)
        {
            _view.SetLives(signal.CurrentLives);
        }
    }
}