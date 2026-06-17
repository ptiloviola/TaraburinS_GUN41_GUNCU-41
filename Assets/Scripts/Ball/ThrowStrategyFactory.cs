namespace Bowling.Ball
{
    public class ThrowStrategyFactory
    {
        public static IThrowStrategy CreateStrategy(int strategyIndex)
        {
            return strategyIndex switch
            {
                0 => new AddForceStrategy(),
                _ => new AddForceStrategy()
            };
        }
    }
}


