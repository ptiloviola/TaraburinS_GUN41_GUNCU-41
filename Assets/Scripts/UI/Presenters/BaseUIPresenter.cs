using System;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Economy;
using Gameplay.Base;
using Gameplay.Interaction;

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
            _view.SetBalance(_bankService.CurrentBalance);
            _view.SetLives(_healthService.CurrentLives);

            _signalBus.Subscribe<SignalBalanceChanged>(OnBalanceChanged);
            _signalBus.Subscribe<SignalBaseDamaged>(OnBaseDamaged);
            _signalBus.Subscribe<SignalInteractionModeChanged>(OnModeChanged);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<SignalBalanceChanged>(OnBalanceChanged);
            _signalBus.Unsubscribe<SignalBaseDamaged>(OnBaseDamaged);
            _signalBus.Unsubscribe<SignalInteractionModeChanged>(OnModeChanged);
        }

        private void OnBalanceChanged(SignalBalanceChanged signal)
        {
            _view.SetBalance(signal.CurrentBalance);
        }

        private void OnBaseDamaged(SignalBaseDamaged signal)
        {
            _view.SetLives(signal.CurrentLives);
        }

        private void OnModeChanged(SignalInteractionModeChanged signal)
        {
            if (signal.Mode == InteractionMode.TacticalClaim)
            {
                _view.gameObject.SetActive(false); 
            }
            else
            {
                _view.gameObject.SetActive(true); 
            }
}
    }
}