using UnityEngine;
using TpsShooter.Weapons.Configs;

namespace TpsShooter.Weapons.Core
{
    public interface IWeapon
    {
        void Initialize(WeaponConfig config);
        void TryFire(Vector3 targetPoint);
        void Reload();
        bool IsClipEmpty();
    }
}