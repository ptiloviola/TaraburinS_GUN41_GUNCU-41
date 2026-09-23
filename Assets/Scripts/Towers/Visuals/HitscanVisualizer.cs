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
                Vector3 aimPoint = targetPosition + Vector3.up * 0.5f;
                _mistStream.transform.LookAt(aimPoint);

                float distance = Vector3.Distance(_mistStream.transform.position, aimPoint);
                var main = _mistStream.main;
                
                main.startLifetime = distance / main.startSpeed.constant;

                _mistStream.Play();
                
                StopAllCoroutines();
                StartCoroutine(StopStreamRoutine());
            }
        }

        private IEnumerator StopStreamRoutine()
        {
            yield return new WaitForSeconds(_feedDuration);
            
            if (_mistStream != null) 
            {
                _mistStream.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }
}