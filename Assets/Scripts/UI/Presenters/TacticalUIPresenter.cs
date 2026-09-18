using System;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Interaction;

namespace Gameplay.UI.Presenters
{
    public class TacticalUIPresenter : IInitializable, IDisposable
    {
        private readonly TacticalUIView _view;
        private readonly SignalBus _signalBus;

        public TacticalUIPresenter(TacticalUIView view, SignalBus signalBus)
        {
            _view = view;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _view.OnStartCombatClicked += HandleStartCombat;
            _signalBus.Subscribe<SignalInteractionModeChanged>(OnModeChanged);
            _signalBus.Subscribe<SignalTacticalClaimsUpdated>(OnClaimsUpdated);

            _view.Hide(); // Прячем по умолчанию
        }

        public void Dispose()
        {
            _view.OnStartCombatClicked -= HandleStartCombat;
            _signalBus.Unsubscribe<SignalInteractionModeChanged>(OnModeChanged);
            _signalBus.Unsubscribe<SignalTacticalClaimsUpdated>(OnClaimsUpdated);
        }

        private void HandleStartCombat()
        {
            _signalBus.Fire<SignalStartCombat>();
        }

        private void OnModeChanged(SignalInteractionModeChanged signal)
        {
            if (signal.Mode == InteractionMode.TacticalClaim) _view.Show();
            else _view.Hide();
        }

        private void OnClaimsUpdated(SignalTacticalClaimsUpdated signal)
        {
            _view.UpdateClaimsText(signal.Available, signal.Max);
        }
    }
}