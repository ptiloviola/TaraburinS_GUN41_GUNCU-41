namespace TpsShooter.Enemies.Core
{
    public class EnemyStateMachine
    {
        public IEnemyState CurrentState { get; private set; }

        public void Initialize(IEnemyState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        public void ChangeState(IEnemyState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Tick()
        {
            CurrentState?.Tick();
        }
    }
}