using UnityEngine;
using TpsShooter.Weapons.Configs;
using Zenject; 
using TpsShooter.Effects; 
using TpsShooter.Player.Inventory;
using TpsShooter.Audio;

namespace TpsShooter.Weapons.Core
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        [SerializeField] protected Transform _muzzlePoint;
        
        [Header("VFX")]
        [Tooltip("Партикл вспышки из дула")]
        [SerializeField] protected ParticleSystem _muzzleFlash;

        // <--- НОВЫЙ БЛОК: НАСТРОЙКИ ЗВУКА --->
        [Header("Audio Settings")]
        [Tooltip("ID звука выстрела в AudioConfig")]
        [SerializeField] protected string _fireSoundId = "Pistol_Fire";
        [Tooltip("ID звука пустого магазина")]
        [SerializeField] protected string _emptySoundId = "Weapon_Empty";
        [Tooltip("ID звука перезарядки")]
        [SerializeField] protected string _reloadSoundId = "Weapon_Reload";

        public Transform LeftHandGripPoint;

        [Header("Aiming Offsets (ADS)")]
        public Vector3 AimPositionOffset;
        public Vector3 AimRotationOffset;

        [Header("Weapon Configuration")]
        [SerializeField] private WeaponConfig _startingConfig; 

        protected WeaponConfig _config;
        protected int _currentAmmoInClip;
        protected float _lastFireTime;
        protected float _currentSpread; 
        
        public float CurrentSpread => _currentSpread; 
        public WeaponConfig Config => _config;
        public int CurrentAmmoInClip => _currentAmmoInClip;
        public int TotalAmmo => _currentAmmoInClip;

        [Inject] protected DecalManager _decalManager;
        [Inject] protected PlayerInventoryModel _inventoryModel; 
        
        // <--- ДОБАВЛЯЕМ ИНЪЕКЦИЮ СЕРВИСА ЭФФЕКТОВ --->
        [Inject] protected IVFXService _vfxService; 
        [Inject] protected IAudioService _audioService;

        private void Awake()
        {
            if (_startingConfig != null) Initialize(_startingConfig);
        }

        public virtual void Initialize(WeaponConfig config)
        {
            _config = config;
            _currentAmmoInClip = config.AmmoPerClip;
        }

        public void TryFire(Vector3 targetPoint)
        {
            if (_config == null || Time.time - _lastFireTime < _config.FireRate) return;

            if (_currentAmmoInClip <= 0)
            {
                PlayEmptySound();
                return;
            }

            _currentAmmoInClip--;
            _lastFireTime = Time.time;

            PlayFireVfx();
            PlayFireSound();

            PerformFire(targetPoint); 
        }

        public virtual void Reload()
        {
            if (_config == null || _currentAmmoInClip == _config.AmmoPerClip) return;

            int ammoNeeded = _config.AmmoPerClip - _currentAmmoInClip;
            int ammoReceived = _inventoryModel.ConsumeAmmo(_config.WeaponAmmoType, ammoNeeded);

            if (ammoReceived > 0)
            {
                _currentAmmoInClip += ammoReceived;
                PlayReloadSound();
            }
        }

        public bool IsClipEmpty() => _currentAmmoInClip <= 0;

        protected abstract void PerformFire(Vector3 targetPoint);

        // --- Запуск локальной вспышки дула ---
        protected virtual void PlayFireVfx() 
        { 
            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }
        }
        
        protected virtual void PlayFireSound() 
        { 
            if (!string.IsNullOrEmpty(_fireSoundId))
                _audioService?.PlaySFX(_fireSoundId, _muzzlePoint.position);
        }
        
        protected virtual void PlayEmptySound() 
        { 
            if (!string.IsNullOrEmpty(_emptySoundId))
                _audioService?.PlaySFX(_emptySoundId, _muzzlePoint.position);
        }
        
        protected virtual void PlayReloadSound() 
        { 
            if (!string.IsNullOrEmpty(_reloadSoundId))
                _audioService?.PlaySFX(_reloadSoundId, transform.position);
        }
    }
}