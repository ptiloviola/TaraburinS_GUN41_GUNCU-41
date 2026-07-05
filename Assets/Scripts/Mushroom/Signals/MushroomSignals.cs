using MeatMushrooms.Mushroom.Contracts;
using UnityEngine;

namespace MeatMushrooms.Mushroom.Signals
{
    public struct MushroomSpawnedSignal
    {
        public IEdible EdibleComponent;
        public IHasAroma AromaComponent;
    }

    public struct MushroomDestroyedSignal
    {
        public Transform DestroyedTransform; 
    }
}