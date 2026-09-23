using UnityEngine;

namespace Gameplay.Enemies
{
    public interface IMovementStrategy
    {
        void Initialize(EnemyFacade enemy);
        
        void Tick(float deltaTime);
    }
}


