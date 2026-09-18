using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Infrastructure.Services
{
    public class PauseService : IPauseService
    {
        private readonly SignalBus _signalBus;
        private bool _isPaused;

        public bool IsPaused => _isPaused;

        public PauseService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void PauseGame()
        {
            if (_isPaused) return;

            _isPaused = true;
            Time.timeScale = 0f;
            
            _signalBus.Fire(new SignalPauseStateChanged(true));
            Debug.Log("<color=cyan>[PauseService] Игра поставлена на паузу.</color>");
        }

        public void ResumeGame()
        {
            if (!_isPaused) return;

            _isPaused = false;
            Time.timeScale = 1f;
            
            _signalBus.Fire(new SignalPauseStateChanged(false));
            Debug.Log("<color=cyan>[PauseService] Игра снята с паузы.</color>");
        }

        public void TogglePause()
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }
}