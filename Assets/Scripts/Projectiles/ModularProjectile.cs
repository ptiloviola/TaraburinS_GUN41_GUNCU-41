using System;
using UnityEngine;
using Zenject;
using Gameplay.Projectiles.Contracts;
using Gameplay.Enemies;

namespace Gameplay.Projectiles 
{
    public class ModularProjectile : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _hitDistance = 0.2f; 

        private Transform _target;
        private EnemyFacade _targetEnemy;
        private IProjectilePayload _payload; 
        private IFlightStrategy _flightStrategy; 
        
        private bool _hasHit; 

        public event Action<ModularProjectile> OnDespawnRequested;

        private void Awake()
        {
            _flightStrategy = GetComponent<IFlightStrategy>();
        }

        public void Launch(Transform target, IProjectilePayload payload)
        {
            _target = target;
            _targetEnemy = target.GetComponent<EnemyFacade>();
            _payload = payload;
            _hasHit = false; 

            _flightStrategy?.Initialize(transform, target);
        }

        private void Update()
        {
            if (_hasHit || _flightStrategy == null) return; 

            if (_target == null || (_targetEnemy != null && _targetEnemy.IsDespawned))
            {
                OnDespawnRequested?.Invoke(this);
                return;
            }

            if (_flightStrategy.ExecuteFlight(transform, _target, _speed, _hitDistance))
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            _hasHit = true; 
            
            _payload?.Apply(_target, transform.position);
            
            OnDespawnRequested?.Invoke(this); 
        }

        public class Pool : MonoMemoryPool<ModularProjectile> {}
    }
}