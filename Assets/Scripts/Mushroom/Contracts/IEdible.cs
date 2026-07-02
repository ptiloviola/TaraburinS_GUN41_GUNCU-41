using UnityEngine;

namespace MeatMushrooms.Mushroom.Contracts
{
    public interface IEdible
    {
        float CurrentHealth { get; }
        Transform Transform { get; }
        
        // Теперь метод возвращает количество полученной сытости
        float Consume(float amount); 
    }
}