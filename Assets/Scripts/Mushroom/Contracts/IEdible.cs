using UnityEngine;

namespace MeatMushrooms.Mushroom.Contracts
{
    public interface IEdible
    {
        float CurrentHealth { get; }
        Transform Transform { get; }
        
        float Consume(float amount); 
    }
}