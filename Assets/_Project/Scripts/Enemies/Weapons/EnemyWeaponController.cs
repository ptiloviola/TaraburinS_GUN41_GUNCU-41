using UnityEngine;
using TpsShooter.Enemies.Configs;
using TpsShooter.Player;
using TpsShooter.Combat;

namespace TpsShooter.Enemies.Weapons
{
    public class EnemyWeaponController : MonoBehaviour
    {
        private EnemyConfig _config;
        private Transform _firePoint; // Точка, откуда вылетает пуля (дуло)

        public void Initialize(EnemyConfig config)
        {
            _config = config;
            // Пока нет 3D-модели пушки, стреляем из центра капсулы
            _firePoint = transform; 
        }

        public void TryFire(PlayerFacade target)
        {
            // 1. Целимся в центр массы игрока (примерно 1.5м от пола)
            Vector3 targetCenter = target.transform.position + Vector3.up * 1.5f;
            Vector3 fireOrigin = _firePoint.position + Vector3.up * 1.5f;
            
            // 2. Получаем скорость игрока для упреждения
            Vector3 targetVelocity = Vector3.zero;
            if (target.TryGetComponent(out CharacterController cc))
            {
                targetVelocity = cc.velocity;
            }

            // 3. Векторная математика (Формула из ТЗ)
            float distance = Vector3.Distance(fireOrigin, targetCenter);
            float timeToHit = distance / _config.ProjectileSpeed;
            
            Vector3 predictedPoint = targetCenter + (targetVelocity * timeToHit);

            // 4. ИСПРАВЛЕННЫЙ РАЗБРОС: Без магических чисел
            float inaccuracyFactor = Mathf.Clamp01(distance / _config.MaxInaccuracyDistance); 
            Vector3 inaccuracyOffset = Random.insideUnitSphere * (_config.AimInaccuracy * inaccuracyFactor);
            predictedPoint += inaccuracyOffset;

            // Направление выстрела
            Vector3 shootDirection = (predictedPoint - fireOrigin).normalized;

            // Длина луча
            float maxRayDistance = distance * 1.5f;

            // 5. ИСПРАВЛЕННАЯ ФИЗИКА: Используем SphereCast (толстый луч радиусом 0.35f)
            // Это прощает врагу мелкие промахи и делает перестрелку опаснее для игрока
            if (Physics.SphereCast(fireOrigin, 0.35f, shootDirection, out RaycastHit hit, maxRayDistance))
            {
                if (hit.collider.GetComponentInParent<IDamageable>() is IDamageable targetDamageable)
                {
                    float damage = _config.WeaponStats != null ? _config.WeaponStats.Damage : 15f;
                    targetDamageable.TakeDamage(damage);
                    Debug.Log($"<color=red>[EnemyWeapon]</color> Пуля зацепила {hit.collider.name}! Урон: {damage}");
                }
                else
                {
                    Debug.Log($"<color=grey>[EnemyWeapon]</color> Промах. Пуля попала в {hit.collider.name}");
                }
            }

            Debug.DrawRay(fireOrigin, shootDirection * maxRayDistance, Color.yellow, 0.2f);
        }
    }
}