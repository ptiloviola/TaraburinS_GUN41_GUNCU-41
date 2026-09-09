using UnityEngine;
using Zenject;
using Gameplay.Projectiles.Contracts;

namespace Gameplay.Projectiles 
{
    public class ModularProjectile : MonoBehaviour
    {
        [Header("Настройки")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _hitDistance = 0.2f; 

        private Transform _target;
        private IProjectilePayload _payload; 
        private IFlightStrategy _flightStrategy; 
        private IMemoryPool _pool;
        
        private bool _hasHit; 

        private void Awake()
        {
            _flightStrategy = GetComponent<IFlightStrategy>();
#if UNITY_EDITOR
            if (_flightStrategy == null)
            {
                Debug.LogError($"[ModularProjectile] На {gameObject.name} не висит компонент IFlightStrategy!");
            }
#endif
        }

        public void Launch(Transform target, IProjectilePayload payload, IMemoryPool pool)
        {
            // ЖЕСТКИЙ СБРОС СОСТОЯНИЯ: Очищаем "карму" снаряда перед каждым выстрелом
            _target = target;
            _payload = payload;
            _pool = pool;
            _hasHit = false; 

            _flightStrategy?.Initialize(transform, target);
        }

        private void Update()
        {
            if (_hasHit || _flightStrategy == null) return; 

            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                Dispose();
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
            Dispose(); 
        }

        private void Dispose()
        {
            // Возвращаем в пул (MonoMemoryPool сам сделает SetActive(false))
            if (_pool != null) 
            {
                _pool.Despawn(this); 
            }
            else 
            {
                Destroy(gameObject); 
            }
        }

        // Чистый класс пула. Zenject сам управляет SetActive(true/false)
        public class Pool : MonoMemoryPool<ModularProjectile> {}
    }
}