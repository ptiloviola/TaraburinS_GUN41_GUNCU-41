using UnityEngine;

namespace TpsShooter.Weapons.Core
{

    public class AssaultRifleWeapon : HitscanWeapon
    {
        private void Update()
        {
            if (_config == null) return;

            if (Time.time - _lastFireTime > _config.FireRate)
            {
                _currentSpread = Mathf.MoveTowards(_currentSpread, _config.BaseSpread, _config.SpreadRecoveryRate * Time.deltaTime);
            }
        }

        protected override void PerformFire(Vector3 targetPoint)
        {
            Vector3 direction = (targetPoint - _muzzlePoint.position).normalized;


            if (_currentSpread > 0f)
            {
                direction += Random.insideUnitSphere * _currentSpread;
                direction.Normalize();
            }


            FireRaycast(direction);


            _currentSpread = Mathf.Min(_currentSpread + _config.SpreadIncreaseRate, _config.MaxSpread);
        }
    }
}