using System;
using Zenject;
using Gameplay.Combat;

namespace Gameplay.Enemies.Visuals
{
    public class GlobalDamageVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly FloatingText.Pool _textPool;
        private readonly DamageVisualSettings _settings;

        public GlobalDamageVisualizer(SignalBus signalBus, FloatingText.Pool textPool, DamageVisualSettings settings)
        {
            _signalBus = signalBus;
            _textPool = textPool;
            _settings = settings;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<DamageReceivedSignal>(OnDamageReceived);
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<DamageReceivedSignal>(OnDamageReceived);
        }

        private void OnDamageReceived(DamageReceivedSignal signal)
        {
            if (_settings == null || _textPool == null) return;

            var floatingText = _textPool.Spawn();
            floatingText.Init(signal.Position, signal.Payload.Amount, _settings.GetColor(signal.Payload.Type), _textPool);
        }
    }
}