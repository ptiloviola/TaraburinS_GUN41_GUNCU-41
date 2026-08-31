namespace Gameplay.Enemies.FSM
{
    public abstract class EnemyStateBase : IEnemyState
    {
        protected readonly EnemyFacade Facade;
        
        public abstract EnemyStateType StateType { get; }

        protected EnemyStateBase(EnemyFacade facade)
        {
            Facade = facade;
        }

        public virtual void Enter() { }
        public virtual void Tick(float deltaTime) { }
        public virtual void Exit() { }
    }
}