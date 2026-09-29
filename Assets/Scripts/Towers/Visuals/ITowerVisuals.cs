using System;
using Gameplay.Towers.Behaviors;

namespace Gameplay.Towers.Visuals
{
    public interface ITowerVisuals
    {

        void Initialize(WeaponAdapter adapter);

        event Action OnAttackImpact;
        void TriggerAttackImpact();

        void ShowRadius(float currentRadius, float upgradedRadius, float minRadius = 0f);
        void HideRadius();

        void PlayVictoryAnimation();
    }
}