using System;

namespace Infrastructure.Interfaces
{
    // Расширяем концепцию урона: теперь у объекта есть здоровье и событие смерти
    public interface IHealth : IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        event Action OnDeath;
    }
}
