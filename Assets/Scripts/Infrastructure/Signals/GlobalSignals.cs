namespace Gameplay.Infrastructure.Signals
{
    public struct SignalPauseStateChanged
    {
        public bool IsPaused;

        public SignalPauseStateChanged(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}