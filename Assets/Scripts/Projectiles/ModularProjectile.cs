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
        private IProjectilePayload _payload; // Бывший IHitEffect
        private IFlightStrategy _flightStrategy; // НОВОЕ: Стратегия полета
        private IMemoryPool _pool;
        private bool _hasHit; 

        private void Awake()
        {
            // Ищем стратегию полета на самом префабе
            _flightStrategy = GetComponent<IFlightStrategy>();
            if (_flightStrategy == null)
            {
                Debug.LogError($"[ModularProjectile] На {gameObject.name} не висит компонент IFlightStrategy!");
            }
        }

        public void Dispose()
        {
            if (_pool != null) _pool.Despawn(this); 
            else Destroy(gameObject); 
        }

        public void OnDespawned()
        {
            _pool = null;
            _target = null;
            _payload = null;
        }

        public void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
        }



        public void Launch(Transform target, IProjectilePayload payload, IMemoryPool pool)
        {
            _target = target;
            _payload = payload;
            _pool = pool;
            _hasHit = false; 

            // НОВОЕ: Говорим стратегии подготовиться к полету
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

            // НОВОЕ: Делегируем полет стратегии. Если она вернула true — мы попали!
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

        public class Pool : MonoMemoryPool<ModularProjectile> {}
    }
}