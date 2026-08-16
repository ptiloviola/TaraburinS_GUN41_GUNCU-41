using UnityEngine;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Combat;
using TpsShooter.Effects; // Для VFX и Декалей
using Zenject;          // Для инъекции

namespace TpsShooter.Enemies.Weapons
{
    public class EnemyWeaponController : MonoBehaviour
    {
        private EnemyConfig _config;
        
        [Header("Setup")]
        [Tooltip("Перетащи сюда объект WeaponSocket из руки врага")]
        [SerializeField] private Transform _weaponSocket;
        private GameObject _currentWeaponInstance;

        private Transform _firePoint; 
        private ParticleSystem _muzzleFlash; // Ссылка на вспышку

        // Внедряем глобальные сервисы эффектов
        [Inject] private IVFXService _vfxService;
        [Inject] private DecalManager _decalManager;

        public void Initialize(EnemyConfig config)
        {
            _config = config;

            if (_config.Type == EnemyType.Ranged && _config.WeaponPrefab != null && _weaponSocket != null)
            {
                _currentWeaponInstance = Instantiate(_config.WeaponPrefab, _weaponSocket);
                
                _currentWeaponInstance.transform.localPosition = Vector3.zero;
                _currentWeaponInstance.transform.localRotation = Quaternion.identity;

                _firePoint = _currentWeaponInstance.transform.Find("FirePoint");
                
                if (_firePoint == null)
                {
                    _firePoint = _currentWeaponInstance.transform;
                }

                // Ищем ParticleSystem в префабе оружия (он найдет наш MuzzleFlash)
                _muzzleFlash = _currentWeaponInstance.GetComponentInChildren<ParticleSystem>();
            }
            else
            {
                _firePoint = transform; 
            }
        }

        public void HideWeapon()
        {
            if (_currentWeaponInstance != null)
            {
                _currentWeaponInstance.SetActive(false);
            }
        }

        public void TryFire(PlayerFacade target)
        {
            if (_firePoint == null) return;

            // 1. Проигрываем вспышку из дула
            if (_muzzleFlash != null)
            {
                _muzzleFlash.Play();
            }

            Vector3 targetCenter = target.transform.position + Vector3.up * 1.5f;
            Vector3 fireOrigin = _firePoint.position; 
            
            float distance = Vector3.Distance(fireOrigin, targetCenter);
            
            Vector3 targetVelocity = Vector3.zero;
            if (target.TryGetComponent(out CharacterController cc))
            {
                targetVelocity = cc.velocity;
            }
            
            float timeToHit = distance / _config.ProjectileSpeed;
            Vector3 predictedPoint = targetCenter + (targetVelocity * timeToHit);

            float inaccuracyFactor = Mathf.Clamp01(distance / _config.MaxInaccuracyDistance); 
            Vector3 inaccuracyOffset = Random.insideUnitSphere * (_config.AimInaccuracy * inaccuracyFactor);
            predictedPoint += inaccuracyOffset;

            Vector3 shootDirection = (predictedPoint - fireOrigin).normalized;
            float maxRayDistance = distance * 1.5f;

            // По умолчанию трассер летит на максимальную дистанцию (промах)
            Vector3 tracerEndPoint = fireOrigin + shootDirection * maxRayDistance;

            if (Physics.SphereCast(fireOrigin, 0.35f, shootDirection, out RaycastHit hit, maxRayDistance))
            {
                tracerEndPoint = hit.point;
                IDamageable targetDamageable = hit.collider.GetComponentInParent<IDamageable>();
                
                if (targetDamageable != null)
                {
                    float damage = _config.WeaponStats != null ? _config.WeaponStats.Damage : 15f;
                    targetDamageable.TakeDamage(damage);
                    
                    // СПАВН КРОВИ (учитываем, что враг может попасть по игроку)
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: true);
                }
                else 
                {
                    if (_decalManager != null) _decalManager.SpawnDecal(hit.point, hit.normal, hit.collider.transform);
                    // СПАВН ИСКР/ПЫЛИ
                    _vfxService?.SpawnImpact(hit.point, hit.normal, isEnemy: false);
                }
            }

            // 2. Запускаем глобальный трассер пули
            _vfxService?.SpawnTracer(fireOrigin, tracerEndPoint);

#if UNITY_EDITOR
            Debug.DrawRay(fireOrigin, shootDirection * maxRayDistance, Color.yellow, 0.2f);
#endif
        }
    }
}