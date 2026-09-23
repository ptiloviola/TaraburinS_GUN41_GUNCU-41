using System;
using Zenject;
using Gameplay.Combat;

namespace Gameplay.Enemies.Visuals
{
    // IInitializable и IDisposable - это интерфейсы Zenject, 
    // заменяющие Start() и OnDestroy()
    public class GlobalDamageVisualizer : IInitializable, IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly FloatingText.Pool _textPool;
        private readonly DamageVisualSettings _settings;

        // Zenject сам подставит все три зависимости
        public GlobalDamageVisualizer(SignalBus signalBus, FloatingText.Pool textPool, DamageVisualSettings settings)
        {
            _signalBus = signalBus;
            _textPool = textPool;
            _settings = settings;
        }

        public void Initialize()
        {
            // Подписываемся на сигнал при старте уровня
            _signalBus.Subscribe<DamageReceivedSignal>(OnDamageReceived);
        }

        public void Dispose()
        {
            // Отписываемся при выходе, чтобы не было утечек памяти
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