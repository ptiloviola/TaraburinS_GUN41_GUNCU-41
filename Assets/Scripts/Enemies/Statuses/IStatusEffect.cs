using Gameplay.Enemies.Data;

namespace Gameplay.Enemies.Statuses
{
    public interface IStatusEffect
    {
        string Id { get; }
        StatusType Type { get; } // НОВОЕ: К какой категории относится
        
        float SpeedModifier { get; }
        float DamageTakenModifier { get; }
        bool IsFinished { get; }
        
        // НОВОЕ: Метод для применения резиста ДО старта эффекта
        void ApplyResistance(float durationMultiplier);
        
        void OnApply(EnemyFacade enemy);
        void Tick(float deltaTime);
        void OnRemove();
    }
}