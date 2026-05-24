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
        
        // Воспроизвести анимацию выстрела (отдача, эффекты, звук)
        void PlayShootAnimation();
        
        // Воспроизвести анимацию строительства/появления башни
        void PlayBuildAnimation();
    }
}