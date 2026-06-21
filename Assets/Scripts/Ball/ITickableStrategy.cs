namespace Bowling.Ball
{
    public interface ITickableStrategy : IThrowStrategy
    {
        void Tick();
        void FixedTick();
    }
}

