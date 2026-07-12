using UnityEngine;
namespace Infrastructure.Interfaces
{
    public interface IEnemyView
    {
        void Initialize(Color tintColor);
        void UpdateMoveAnimation(float speed, float rotationY);
        void PlayHitReaction();
        void PlayDeathEffect();
        void SetVisibility(bool isVisible);
    }
}
