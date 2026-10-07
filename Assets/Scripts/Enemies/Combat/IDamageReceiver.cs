namespace Gameplay.Enemies.Combat
{
    public interface IDamageReceiver
    {
        int Health { get; }
        void TakeDamage(int amount);
    }
}
