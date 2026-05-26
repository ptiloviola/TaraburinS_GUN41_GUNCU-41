using UnityEngine;

namespace Gameplay.Towers.Visuals
{
    // Это наш "API" для общения логики с графикой
    public interface ITowerVisuals
    {

        // C# Событие, на которое сможет подписаться геймплейный код
        event System.Action OnAttackImpact;
        
        // Метод для инициализации визуала (передаем ссылку на данные, если нужно)
        void Initialize();
        
        // Воспроизвести анимацию выстрела (отдача, эффекты, звук, след выстрела)
        void PlayShootAnimation(Vector3 targetPosition);
        
        // Воспроизвести анимацию строительства/появления башни
        void PlayBuildAnimation();
    }
}