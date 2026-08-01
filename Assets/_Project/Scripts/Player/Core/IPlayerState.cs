namespace TpsShooter.Player.Core
{
    public interface IPlayerState
    {
        void Enter();
        void Tick(float deltaTime);
        void Exit();
        void HandleJump();
    }
}