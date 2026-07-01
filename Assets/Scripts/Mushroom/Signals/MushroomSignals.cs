using MeatMushrooms.Mushroom.Contracts;
using UnityEngine;

namespace MeatMushrooms.Mushroom.Signals
{
    public struct MushroomSpawnedSignal
    {
        // Теперь мы передаем не просто GameObject, а конкретные контракты!
        public IEdible EdibleComponent;
        public IHasAroma AromaComponent;
    }

    public struct MushroomDestroyedSignal
    {
        // Волку достаточно знать Transform, чтобы понять, та ли это еда, к которой он бежал
        public Transform DestroyedTransform; 
    }
}