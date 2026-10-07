using UnityEngine;
using Zenject;
using System;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Infrastructure.Services
{
    public class TimeScaleService : ITimeScaleService, IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        
        private readonly float[] _availableSpeeds = { 1f, 2f, 3f };
        private const float BaseFixedDeltaTime = 0.02f;

        private int _currentIndex = 0;
        private bool _isPaused = false;

        public float CurrentScale => _availableSpeeds[_currentIndex];

        public TimeScaleService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            ApplyScaleToEngine();
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<SignalPauseStateChanged>(OnPauseStateChanged);
            
            Time.timeScale = 1f;
            Time.fixedDeltaTime = BaseFixedDeltaTime;
        }

        public void CycleSpeed()
        {
            _currentIndex = (_currentIndex + 1) % _availableSpeeds.Length;
            
            if (!_isPaused)
            {
                ApplyScaleToEngine();
            }
            
            _signalBus.Fire(new SignalTimeScaleChanged(CurrentScale));
            Gameplay.Tools.GameLogger.Log($"<color=yellow>[TimeScaleService] Скорость игры изменена на {CurrentScale}x</color>");
        }

        private void OnPauseStateChanged(SignalPauseStateChanged signal)
        {
            _isPaused = signal.IsPaused;
            
            if (!_isPaused)
            {
                ApplyScaleToEngine();
            }
        }

        private void ApplyScaleToEngine()
        {
            Time.timeScale = CurrentScale;
            Time.fixedDeltaTime = BaseFixedDeltaTime * CurrentScale;
        }

        public void ResetSpeed()
        {
            _currentIndex = 0;
            
            if (!_isPaused)
            {
                ApplyScaleToEngine();
            }
            
            _signalBus.Fire(new SignalTimeScaleChanged(CurrentScale));
        }
    }
}