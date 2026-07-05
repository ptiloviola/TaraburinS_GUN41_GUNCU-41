using UnityEngine;

namespace MeatMushrooms.Mushroom.Contracts
{
    public interface IHasAroma
    {
        float CurrentRadius { get; }
        Transform Transform { get; }
    }
}