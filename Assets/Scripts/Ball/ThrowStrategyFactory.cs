namespace Bowling.Ball
{
    public class ThrowStrategyFactory
    {
        public static IThrowStrategy CreateStrategy(int strategyIndex, PhysicsConfig physicsConfig)
        {
            return strategyIndex switch
            {
                0 => new AddForceStrategy(physicsConfig),
                1 => new LinearVelocityStrategy(physicsConfig),
                2 => new MovePositionStrategy(physicsConfig),
                _ => new AddForceStrategy(physicsConfig)
            };
        }
    }
}


