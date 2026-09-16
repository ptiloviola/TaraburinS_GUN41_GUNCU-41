using UnityEngine;
using Gameplay.Towers.Behaviors;
using System.Collections;

namespace Gameplay.Towers.Visuals
{
    public class HitscanVisualizer : MonoBehaviour
    {
        [SerializeField] private WeaponAdapter _weaponAdapter;
        [SerializeField] private ParticleSystem _mistStream;
        
        [Tooltip("Должно совпадать с временем надувания сферы (например, 0.4)")]
        [SerializeField] private float _feedDuration = 0.4f; 

        private void OnEnable()
        {
            if (_weaponAdapter != null) _weaponAdapter.OnShotFired += HandleShot;
        }

        private void OnDisable()
        {
            if (_weaponAdapter != null) _weaponAdapter.OnShotFired -= HandleShot;
        }

        private void HandleShot(Vector3 targetPosition)
        {
            if (_mistStream != null)
            {
                // Приподнимаем точку, чтобы струя не била в пол
                Vector3 aimPoint = targetPosition + Vector3.up * 0.5f;
                _mistStream.transform.LookAt(aimPoint);

                // МАГИЯ МАТЕМАТИКИ: Заставляем струю умирать ровно в центре лужи
                float distance = Vector3.Distance(_mistStream.transform.position, aimPoint);
                var main = _mistStream.main;
                
                // Время = Расстояние / Скорость. (например: 15м / 30м/с = 0.5 сек жизни)
                main.startLifetime = distance / main.startSpeed.constant;

                _mistStream.Play();
                
                // Останавливаем струю ровно в тот момент, когда сфера надулась
                StopAllCoroutines();
                StartCoroutine(StopStreamRoutine());
            }
        }

        private IEnumerator StopStreamRoutine()
        {
            // Ждем, пока сфера надуется
            yield return new WaitForSeconds(_feedDuration);
            
            // Выключаем "кран", но выпущенные частицы долетают до лужи красиво
            if (_mistStream != null) 
            {
                _mistStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}