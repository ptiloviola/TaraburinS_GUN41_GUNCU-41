using UnityEngine;
using TpsShooter.Weapons.Configs;

namespace TpsShooter.Weapons
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        [Header("Weapon Data")]
        [SerializeField] protected WeaponConfig _config;

        [Header("IK Sockets (Точки крепления)")]
        [Tooltip("Точка, откуда вылетает пуля")]
        [SerializeField] protected Transform _muzzlePoint; 
        
        [Tooltip("Пустышка, за которую персонаж должен взяться левой рукой")]
        [SerializeField] public Transform LeftHandGripPoint;

        protected int _currentAmmoInClip;
        protected int _currentTotalAmmo;

        public virtual void Initialize()
        {
            _currentAmmoInClip = _config.AmmoPerClip;
            _currentTotalAmmo = _config.MaxAmmo;
        }

        // Реализация интерфейса для физического перемещения
        public void SetParent(Transform parent, Vector3 localPosition, Vector3 localRotation)
        {
            transform.SetParent(parent);
            transform.localPosition = localPosition;
            transform.localEulerAngles = localRotation;
        }

        // Абстрактные методы, которые заставят наследников (Пистолет, Дробовик) реализовать свою логику выстрела
        public abstract void Fire();
        public abstract void Reload();
    }
}