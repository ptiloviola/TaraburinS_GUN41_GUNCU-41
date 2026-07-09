
namespace Infrastructure.Interfaces
{
    public interface IWeaponView
    {
        void PlayFireAnimation();
        void PlayReloadAnimation(float duration);
        void SetAimState(bool isAiming);
        void PlayBobbing(bool isMoving, bool isAiming);
    }
}