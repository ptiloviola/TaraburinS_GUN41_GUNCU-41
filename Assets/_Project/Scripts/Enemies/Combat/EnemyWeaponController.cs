using UnityEngine;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Combat;
using TpsShooter.Effects;
using Zenject;
using TpsShooter.Audio;
using TpsShooter.Enemies.Core; 

namespace TpsShooter.Enemies.Combat
{
    public class EnemyWeaponController : MonoBehaviour, IEnemyCombatHandler
    {
        private const float TargetCenterYOffset = 1.5f;
        private const float DefaultFallbackDamage = 15f;
        private const float SphereCastRadius = 0.35f;
        private const float MaxRayDistanceMultiplier = 1.5f;

        private RangedEnemyConfig _config; 
        
        [Header("Setup")]
        [SerializeField] private Transform _weaponSocket;
        [Header("Audio")]
        [SerializeField] private string _fireSoundId = "Rifle_Fire";

        private GameObject _currentWeaponInstance;
        private Transform _firePoint; 
        private ParticleSystem _muzzleFlash; 

        [Inject] private IVFXService _vfxService;
        [Inject] private DecalManager _decalManager;
        [Inject] private IAudioService _audioService;

        public void Initialize(EnemyConfig config)
        {
            _config = config as RangedEnemyConfig;

            if (_config != null && _config.WeaponPrefab != null && _weaponSocket != null)
            {
                _currentWeaponInstance = Instantiate(_config.WeaponPrefab, _weaponSocket);
                _currentWeaponInstance.transform.localPosition = Vector3.zero;
                _currentWeaponInstance.transform.localRotation = Quaternion.identity;

                _firePoint = _currentWeaponInstance.transform.Find("FirePoint") ?? _currentWeaponInstance.transform;
                _muzzleFlash = _currentWeaponInstance.GetComponentInChildren<ParticleSystem>();
            }
            else
            {
                _firePoint = transform; 
            }
        }

        public void PerformAttack(PlayerFacade target, EnemyAnimator animator)
        {
            if (_config != null)
            {
                animator?.PlayAttack(_config.ShootAnimState, _config.ShootAnimDuration); 
            }
            TryFire(target);       
        }

        public void OnDeath() => HideWeapon();

        public void HideWeapon()
        {
            if (_currentWeaponInstance != null) _currentWeaponInstance.SetActive(false);
        }

        public void TryFire(PlayerFacade target)
        {
            if (_firePoint == null || _config == null) return;

            if (_muzzleFlash != null) _muzzleFlash.Play();
            if (!string.IsNullOrEmpty(_fireSoundId)) _audioService?.PlaySFX(_fireSoundId, _firePoint.position);

            Vector3 targetCenter = target.transform.position + Vector3.up * TargetCenterYOffset;
            Vector3 fireOrigin = _firePoint.position; 
            float distance = Vector3.Distance(fireOrigin, targetCenter);
            
            Vector3 targetVelocity = Vector3.zero;
            if (target.TryGetComponent(out CharacterController cc)) targetVelocity = cc.velocity;
            

            float timeToHit = distance / _config.ProjectileSpeed;
            Vector3 predictedPoint = targetCenter + (targetVelocity * timeToHit);

            float inaccuracyFactor = Mathf.Clamp01(distance / _config.MaxInaccuracyDistance); 
            Vector3 inaccuracyOffset = Random.insideUnitSphere * (_config.AimInaccuracy * inaccuracyFactor);
            predictedPoint += inaccuracyOffset;

            Vector3 shootDirection = (predictedPoint - fireOrigin).normalized;
            float maxRayDistance = distance * MaxRayDistanceMultiplier;
            Vector3 tracerEndPoint = fireOrigin + shootDirection * maxRayDistance;

            if (Physics.SphereCast(fireOrigin, SphereCastRadius, shootDirection, out RaycastHit hit, maxRayDistance))
            {
                tracerEndPoint = hit.point;
                IDamageable targetDamageable = hit.collider.GetComponentInParent<IDamageable>();
                
                if (targetDamageable != null)
                {
                    float damage = _config.WeaponStats != null ? _config.WeaponStats.Damage : DefaultFallbackDamage;
                    targetDamageable.TakeDamage(damage);
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: true);
                }
                else 
                {
                    if (_decalManager != null) _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: false);
                }
            }
            _vfxService?.SpawnTracer(fireOrigin, tracerEndPoint);
        }
    }
}