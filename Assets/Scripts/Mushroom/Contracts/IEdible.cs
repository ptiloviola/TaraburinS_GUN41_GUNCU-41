using UnityEngine;

namespace MeatMushrooms.Mushroom.Contracts
{
    public interface IEdible
    {
        float CurrentHealth { get; }
        Transform Transform { get; } // Чтобы волк знал, к каким координатам бежать
        void Consume(float amount);  // Метод откусывания
    }
}