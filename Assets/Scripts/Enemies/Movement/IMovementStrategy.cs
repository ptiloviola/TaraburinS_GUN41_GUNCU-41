using UnityEngine.AI;

namespace Gameplay.Enemies.Movement
{
    public interface IMovementStrategy
    {
        void Initialize(NavMeshAgent agent);
        
        void Tick(float deltaTime);

        void UpdateSpeed(float baseSpeed, float multiplier);
    }
}


