using UnityEngine;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Combat;

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

        public void Initialize(EnemyConfig config)
        {
            _config = config;

            if (_config.Type == EnemyType.Ranged && _config.WeaponPrefab != null && _weaponSocket != null)
            {
                // Сохраняем ссылку на созданный объект в нашу новую переменную
                _currentWeaponInstance = Instantiate(_config.WeaponPrefab, _weaponSocket);
                
                _currentWeaponInstance.transform.localPosition = Vector3.zero;
                _currentWeaponInstance.transform.localRotation = Quaternion.identity;

                _firePoint = _currentWeaponInstance.transform.Find("FirePoint");
                
                if (_firePoint == null)
                {
                    Debug.LogWarning($"На префабе {_currentWeaponInstance.name} нет 'FirePoint'!");
                    _firePoint = _currentWeaponInstance.transform;
                }
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
                _currentWeaponInstance.SetActive(false); // Просто выключаем визуал пушки в руке
            }
        }

        public void TryFire(PlayerFacade target)
        {
            if (_firePoint == null) return;

            Vector3 targetCenter = target.transform.position + Vector3.up * 1.5f;
            Vector3 fireOrigin = _firePoint.position; 
            
            float distance = Vector3.Distance(fireOrigin, targetCenter);
            
            // Расчет упреждения
            Vector3 targetVelocity = Vector3.zero;
            if (target.TryGetComponent(out CharacterController cc))
            {
                targetVelocity = cc.velocity;
            }
            
            float timeToHit = distance / _config.ProjectileSpeed;
            Vector3 predictedPoint = targetCenter + (targetVelocity * timeToHit);

            // Разброс
            float inaccuracyFactor = Mathf.Clamp01(distance / _config.MaxInaccuracyDistance); 
            Vector3 inaccuracyOffset = Random.insideUnitSphere * (_config.AimInaccuracy * inaccuracyFactor);
            predictedPoint += inaccuracyOffset;

            Vector3 shootDirection = (predictedPoint - fireOrigin).normalized;
            float maxRayDistance = distance * 1.5f;

            // Выстрел
            if (Physics.SphereCast(fireOrigin, 0.35f, shootDirection, out RaycastHit hit, maxRayDistance))
            {
                if (hit.collider.GetComponentInParent<IDamageable>() is IDamageable targetDamageable)
                {
                    float damage = _config.WeaponStats != null ? _config.WeaponStats.Damage : 15f;
                    targetDamageable.TakeDamage(damage);
                    Debug.Log($"<color=red>[EnemyWeapon]</color> Попадание! Урон: {damage}");
                }
            }

            Debug.DrawRay(fireOrigin, shootDirection * maxRayDistance, Color.yellow, 0.2f);
        }
    }
}