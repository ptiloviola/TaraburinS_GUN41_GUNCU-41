using System;
using Zenject;
using TpsShooter.Environment;

namespace TpsShooter.UI.Presenters
{
    public class ExtractionUIPresenter : IInitializable, IDisposable, ITickable
    {
        private readonly ExtractionPoint _model;
        private readonly ExtractionUIView _view;

        public ExtractionUIPresenter(ExtractionPoint model, ExtractionUIView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize()
        {
            _view.Hide();
            
            _model.OnActivated += HandleActivated;
            _model.OnTimeExpired += HandleDeactivated;
            _model.OnPlayerExtracted += HandleDeactivated;
        }

        public void Dispose()
        {
            _model.OnActivated -= HandleActivated;
            _model.OnTimeExpired -= HandleDeactivated;
            _model.OnPlayerExtracted -= HandleDeactivated;
        }

        private void HandleActivated() => _view.Show();
        private void HandleDeactivated() => _view.Hide();

        public void Tick()
        {
            if (_model.IsActive)
            {
                _view.UpdateTime(_model.TimeRemaining);
            }
        }
    }
}