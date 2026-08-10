using UnityEngine;
using TpsShooter.Weapons.Configs;
using Zenject; // Добавь сверху
using TpsShooter.Effects; // Добавь сверху

namespace TpsShooter.Weapons.Core
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        [SerializeField] protected Transform _muzzlePoint;
        public Transform LeftHandGripPoint;

        [Header("Aiming Offsets (ADS)")]
        public Vector3 AimPositionOffset;
        public Vector3 AimRotationOffset;

        [Header("Weapon Configuration")]
        [SerializeField] private WeaponConfig _startingConfig; 

        protected WeaponConfig _config;
        protected int _currentAmmoInClip;
        protected int _currentReserveAmmo;
        protected float _lastFireTime;
        protected float _currentSpread; // Защищенное поле для внутренних расчетов
        public float CurrentSpread => _currentSpread; // Публичный геттер для UI прицела

        public WeaponConfig Config => _config;

        [Inject] protected DecalManager _decalManager;

        private void Awake()
        {
            if (_startingConfig != null)
            {
                Initialize(_startingConfig);
            }
        }

        public virtual void Initialize(WeaponConfig config)
        {
            _config = config;
            _currentAmmoInClip = config.AmmoPerClip;
            _currentReserveAmmo = config.MaxReserveAmmo;
            Debug.Log($"[WeaponBase] Оружие {gameObject.name} инициализировано! Патронов: {_currentAmmoInClip}");
        }

        public void TryFire(Vector3 targetPoint)
        {
            if (_config == null)
            {
                Debug.LogError($"[WeaponBase] ОШИБКА! В пушке {gameObject.name} нет конфига!");
                return;
            }

            if (Time.time - _lastFireTime < _config.FireRate) 
            {
                return; 
            }

            if (_currentAmmoInClip <= 0)
            {
                Debug.LogWarning($"[WeaponBase] Нет патронов в {gameObject.name}!");
                PlayEmptySound();
                return;
            }

            Debug.Log($"[WeaponBase] TryFire прошел все проверки! Стреляем!");
            
            _currentAmmoInClip--;
            _lastFireTime = Time.time;

            PlayFireVfx();
            PlayFireSound();

            PerformFire(targetPoint); 
        }

        public virtual void Reload()
        {
            if (_config == null) return;
            if (_currentAmmoInClip == _config.AmmoPerClip || _currentReserveAmmo <= 0) return;

            int ammoNeeded = _config.AmmoPerClip - _currentAmmoInClip;
            int ammoToReload = Mathf.Min(ammoNeeded, _currentReserveAmmo);

            _currentAmmoInClip += ammoToReload;
            _currentReserveAmmo -= ammoToReload;

            Debug.Log($"[WeaponBase] Перезарядка! В магазине: {_currentAmmoInClip}, в запасе: {_currentReserveAmmo}");
            PlayReloadSound();
        }

        public bool IsClipEmpty() 
        {
            return _currentAmmoInClip <= 0;
        }

        protected abstract void PerformFire(Vector3 targetPoint);

        // --- Заглушки под будущий PoolManager ---
        protected virtual void PlayFireVfx() 
        { 
            // Позже тут будет: PoolManager.Spawn(_config.MuzzleFlashPrefab, _muzzlePoint.position, ...);
        }
        protected virtual void PlayFireSound() { }
        protected virtual void PlayEmptySound() { }
        protected virtual void PlayReloadSound() { }
    }
}