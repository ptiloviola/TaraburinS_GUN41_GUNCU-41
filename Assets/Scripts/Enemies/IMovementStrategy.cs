using UnityEngine;

namespace Gameplay.Enemies
{
    public interface IMovementStrategy
    {
        // Инициализация стратегии (передаем трансформ врага, чтобы двигать его)
        void Initialize(Transform enemyTransform);
        
        // Обновление движения каждый кадр
        void Tick(float deltaTime);
    }
}


