namespace Gameplay.Enemies
{
    public interface IDamageReceiver
    {
        int Health { get; }
        void TakeDamage(int amount);
    }
}
