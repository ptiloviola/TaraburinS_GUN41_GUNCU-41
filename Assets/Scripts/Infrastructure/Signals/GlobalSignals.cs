namespace Gameplay.Infrastructure.Signals
{
    // Глобальный сигнал изменения состояния паузы
    public struct SignalPauseStateChanged
    {
        public bool IsPaused;

        public SignalPauseStateChanged(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}