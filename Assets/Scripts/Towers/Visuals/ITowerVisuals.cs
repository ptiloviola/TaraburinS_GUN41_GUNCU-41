using System;
using Gameplay.Towers.Behaviors;

namespace Gameplay.Towers.Visuals
{
    public interface ITowerVisuals
    {
        // 1. Инициализация (связываем визуал с боевой логикой)
        void Initialize(WeaponAdapter adapter);

        // 2. Событие для башен ближнего боя (Мечники/Казармы).
        // Срабатывает из Unity Animation Event на нужном кадре замаха.
        event Action OnAttackImpact;
        void TriggerAttackImpact();
    }
}