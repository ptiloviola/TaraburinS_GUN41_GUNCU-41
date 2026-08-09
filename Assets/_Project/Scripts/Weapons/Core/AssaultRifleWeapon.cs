using UnityEngine;
using TpsShooter.Combat;

namespace TpsShooter.Weapons.Core
{
    public class AssaultRifleWeapon : WeaponBase
    {
        private float _currentSpread = 0f;

        private void Update()
        {
            if (_config == null) return;

            // Восстановление прицела, если с момента последнего выстрела прошло больше времени, чем FireRate
            if (Time.time - _lastFireTime > _config.FireRate)
            {
                _currentSpread = Mathf.MoveTowards(_currentSpread, _config.BaseSpread, _config.SpreadRecoveryRate * Time.deltaTime);
            }
        }

        protected override void PerformFire(Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;

            // 1. Применяем текущий разброс
            if (_currentSpread > 0f)
            {
                direction += Random.insideUnitSphere * _currentSpread;
                direction.Normalize();
            }

            // 2. Пускаем луч
            if (Physics.Raycast(_muzzlePoint.position, direction, out RaycastHit hit, _config.Range, _config.HitMask))
            {
                Debug.Log($"[AssaultRifle] Попали в: {hit.collider.name}");
                
                if (hit.collider.TryGetComponent(out IDamageable target))
                {
                    target.TakeDamage(_config.Damage);
                }
                // Заглушка: Спавн декали
            }

            // 3. Увеличиваем разброс для следующего выстрела (штраф за зажим)
            _currentSpread = Mathf.Min(_currentSpread + _config.SpreadIncreaseRate, _config.MaxSpread);

            // 4. Заглушка для отдачи
            ApplyCameraRecoil();
        }

        private void ApplyCameraRecoil()
        {
            // Здесь мы будем вызывать событие или дергать интерфейс контроллера камеры, 
            // передавая ему _config.RecoilForce, чтобы камеру дернуло вверх.
            // Например: OnRecoilTriggered?.Invoke(_config.RecoilForce);
        }
    }
}