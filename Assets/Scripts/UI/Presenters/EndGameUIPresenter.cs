using System;
using Zenject;
using Gameplay.Infrastructure.Signals;
using Gameplay.UI.Views;
using Gameplay.Infrastructure.Services;
using Cysharp.Threading.Tasks;

namespace Gameplay.UI.Presenters
{
    public class EndGameUIPresenter : IInitializable, IDisposable
    {
        private readonly EndGameUIView _view;
        private readonly SignalBus _signalBus;
        private readonly ISceneLoaderService _sceneLoader;
        
        private bool _isWin;

        public EndGameUIPresenter(
            EndGameUIView view, 
            SignalBus signalBus, 
            ISceneLoaderService sceneLoader)
        {
            _view = view;
            _signalBus = signalBus;
            _sceneLoader = sceneLoader;
        }

        public void Initialize()
        {
            _view.Hide();
            
            _view.OnActionClicked += HandleActionClicked;
            
            _signalBus.Subscribe<SignalLevelWon>(OnLevelWon);
            _signalBus.Subscribe<SignalLevelLost>(OnLevelLost);
        }

        public void Dispose()
        {
            _view.OnActionClicked -= HandleActionClicked;
            _signalBus.Unsubscribe<SignalLevelWon>(OnLevelWon);
            _signalBus.Unsubscribe<SignalLevelLost>(OnLevelLost);
        }

        private void OnLevelWon()
        {
            _isWin = true;
            _view.Show("VICTORY!", "CONTINUE");
        }

        private void OnLevelLost()
        {
            _isWin = false;
            _view.Show("DEFEAT", "RETURN TO MAIN MENU");
        }

        private void HandleActionClicked()
        {
            _view.Hide();

            if (_isWin)
            {
                _sceneLoader.LoadSceneAsync("HubScene").Forget(); 
            }
            else
            {
                _sceneLoader.LoadSceneAsync("MainMenuScene").Forget(); 
            }
        }
    }
}