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

    public struct SignalTimeScaleChanged
    {
        public float TimeScale;
        public SignalTimeScaleChanged(float timeScale)
        {
            TimeScale = timeScale;
        }
    }
}