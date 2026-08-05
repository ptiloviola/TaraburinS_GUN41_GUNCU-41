using UnityEngine;

namespace TpsShooter.Weapons
{
    public interface IWeapon
    {
        void Fire();
        void Reload();
        // Метод для физического перемещения префаба оружия
        void SetParent(Transform parent, Vector3 localPosition, Vector3 localRotation);
    }
}