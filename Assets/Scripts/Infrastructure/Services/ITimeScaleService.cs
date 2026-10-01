namespace Gameplay.Infrastructure.Services
{
    public interface ITimeScaleService
    {
        float CurrentScale { get; }
        void CycleSpeed();
        void ResetSpeed();
    }
}