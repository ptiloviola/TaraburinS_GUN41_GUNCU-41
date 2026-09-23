using Gameplay.Enemies;

namespace Gameplay.Combat.Statuses
{
    public interface IStatusEffect
    {
        string Id { get; }
        StatusType Type { get; }
        
        float SpeedModifier { get; }
        float DamageTakenModifier { get; }
        bool IsFinished { get; }
        
        void ApplyResistance(float durationMultiplier);
        
        // Пока оставляю привязку к EnemyFacade. 
        // В будущем, если захотим вешать статусы на башни, заменю на интерфейс IStatusReceiver.
        void OnApply(EnemyFacade enemy); 
        void Tick(float deltaTime);
        void OnRemove();
    }
}