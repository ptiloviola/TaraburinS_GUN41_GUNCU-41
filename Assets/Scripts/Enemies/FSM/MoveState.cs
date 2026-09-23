namespace Gameplay.Enemies.FSM
{
    public class MoveState : EnemyStateBase
    {
        private readonly IMovementStrategy _movementStrategy;

        public override EnemyStateType StateType => EnemyStateType.Move;

        public MoveState(EnemyFacade facade, IMovementStrategy movementStrategy) : base(facade)
        {
            _movementStrategy = movementStrategy;
        }

        public override void Enter()
        {
            if (Facade.Agent != null && !Facade.Agent.enabled)
            {
                Facade.Agent.enabled = true;
            }
        }

        public override void Tick(float deltaTime)
        {
            _movementStrategy?.Tick(deltaTime);
        }
    }
}