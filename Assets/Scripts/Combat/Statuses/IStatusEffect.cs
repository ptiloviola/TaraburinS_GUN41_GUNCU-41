using Gameplay.Enemies;
using UnityEngine;

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
        
        void OnApply(GameObject target);
        void Tick(float deltaTime);
        void OnRemove();
    }
}