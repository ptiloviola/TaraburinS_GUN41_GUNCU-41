using UnityEngine.AI;

namespace Gameplay.Enemies
{
    public interface IMovementStrategy
    {
        void Initialize(NavMeshAgent agent);
        
        void Tick(float deltaTime);
    }
}


