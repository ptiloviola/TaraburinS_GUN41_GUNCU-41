using UnityEngine;

namespace Gameplay.Enemies
{
    public interface IMovementStrategy
    {
        // Передаем фасад врага, чтобы стратегия могла управлять его жизненным циклом
        void Initialize(EnemyFacade enemy);
        
        // Обновление движения каждый кадр
        void Tick(float deltaTime);
    }
}


