using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using Gameplay.Infrastructure.Signals;

namespace Gameplay.Spawning.Services
{
    public class WaveTimerService
    {
        private readonly SignalBus _signalBus;
        private bool _isSkipped;

        public WaveTimerService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public async UniTask<float> WaitTimeOrSkipAsync(float duration, CancellationToken ct)
        {
            _isSkipped = false;
            _signalBus.Subscribe<SignalForceStartWave>(OnSkipSignalReceived);

            float timeLeft = duration;
            try
            {
                while (timeLeft > 0f && !_isSkipped)
                {
                    timeLeft -= Time.deltaTime;
                    
                    _signalBus.Fire(new SignalWaveTimerUpdated
                    {
                        TimeLeft = Mathf.Max(0f, timeLeft),
                        Progress = 1f - (timeLeft / duration)
                    });
                    
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }

                return _isSkipped ? timeLeft : 0f;
            }
            finally
            {
                _signalBus.Unsubscribe<SignalForceStartWave>(OnSkipSignalReceived);
                _signalBus.Fire(new SignalWaveTimerUpdated { TimeLeft = 0f, Progress = 1f });
            }
        }

        public async UniTask<bool> WaitConditionOrSkipAsync(Func<bool> condition, CancellationToken ct)
        {
            _isSkipped = false;
            _signalBus.Subscribe<SignalForceStartWave>(OnSkipSignalReceived);

            try
            {
                while (!condition() && !_isSkipped)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
                return _isSkipped;
            }
            finally
            {
                _signalBus.Unsubscribe<SignalForceStartWave>(OnSkipSignalReceived);
            }
        }

        private void OnSkipSignalReceived()
        {
            _isSkipped = true;
        }
    }
}