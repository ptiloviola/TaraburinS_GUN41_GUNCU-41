using System;

namespace Infrastructure.Interfaces
{
    public interface IHealth : IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        event Action OnDeath;
    }
}
