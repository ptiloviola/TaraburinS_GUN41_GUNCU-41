using System;
using Zenject;
using Gameplay.UI.Views;
using Gameplay.Infrastructure.Services;
using Gameplay.Infrastructure.Input;
using Gameplay.Infrastructure.Signals;
using Cysharp.Threading.Tasks;

namespace Gameplay.UI.Presenters
{
    public class PauseMenuPresenter : IInitializable, IDisposable
    {
        private readonly PauseMenuView _view;
        private readonly IPauseService _pauseService;
        private readonly IInputService _inputService;
        private readonly ISceneLoaderService _sceneLoader;
        private readonly SignalBus _signalBus;

        private const string MainMenuSceneName = "MainMenu";

        public PauseMenuPresenter(
            PauseMenuView view,
            IPauseService pauseService,
            IInputService inputService,
            ISceneLoaderService sceneLoader,
            SignalBus signalBus)
        {
            _view = view;
            _pauseService = pauseService;
            _inputService = inputService;
            _sceneLoader = sceneLoader;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _view.Hide(); // По умолчанию меню скрыто

            _view.OnResumeClicked += HandleResumeClicked;
            _view.OnMainMenuClicked += HandleMainMenuClicked;
            
            // Подписываемся на кнопку Escape
            _inputService.OnCancelAction += HandleCancelAction;
            
            // Подписываемся на глобальный сигнал изменения паузы
            _signalBus.Subscribe<SignalPauseStateChanged>(OnPauseStateChanged);
        }

        public void Dispose()
        {
            _view.OnResumeClicked -= HandleResumeClicked;
            _view.OnMainMenuClicked -= HandleMainMenuClicked;
            
            if (_inputService != null)
                _inputService.OnCancelAction -= HandleCancelAction;
                
            _signalBus.Unsubscribe<SignalPauseStateChanged>(OnPauseStateChanged);
        }

        private void HandleCancelAction()
        {
            _pauseService.TogglePause();
        }

        private void OnPauseStateChanged(SignalPauseStateChanged signal)
        {
            if (signal.IsPaused)
                _view.Show();
            else
                _view.Hide();
        }

        private void HandleResumeClicked()
        {
            _pauseService.ResumeGame();
        }

        private void HandleMainMenuClicked()
        {
            // КРИТИЧНО: Всегда снимаем игру с паузы перед выходом в меню, 
            // иначе анимации и загрузки сломаются (Time.timeScale останется 0)
            _pauseService.ResumeGame(); 
            _sceneLoader.LoadSceneAsync(MainMenuSceneName).Forget();
        }
    }
}