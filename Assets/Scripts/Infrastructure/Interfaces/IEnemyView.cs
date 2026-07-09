using UnityEngine;
namespace Infrastructure.Interfaces
{
    // Абстракция графики. Отвязывает логику от конкретной реализации анимаций
    public interface IEnemyView
    {
        void Initialize(Color tintColor);
        void UpdateMoveAnimation(float speed, float rotationY);
        void PlayHitReaction();
        void PlayDeathEffect();
        void SetVisibility(bool isVisible);
    }
}
