namespace TpsShooter.Enemies.Core
{
    public interface IEnemyState
    {
        void Enter();
        void Tick();
        void Exit();
    }
}


