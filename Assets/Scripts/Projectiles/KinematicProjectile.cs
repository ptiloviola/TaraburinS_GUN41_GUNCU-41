using UnityEngine;
using Gameplay.Core;
using Zenject;

// НОВОЕ: Теперь снаряды живут в своем собственном домене!
namespace Gameplay.Projectiles 
{
    // Убираем System.IDisposable, так как метод Dispose мы используем как кастомный
    public class KinematicProjectile : MonoBehaviour
    {
        [Header("Настройки полета")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _hitDistance = 0.2f; 
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 0.5f, 0f);

        private Transform _target;
        private float _damage;
        private IMemoryPool _pool;
        private bool _hasHit; 

        public void Dispose()
        {
            if (_pool != null)
            {
                _pool.Despawn(this); 
            }
            else
            {
                Destroy(gameObject); 
            }
        }

        public void OnDespawned()
        {
            _pool = null;
            _target = null;
        }

        public void OnSpawned(IMemoryPool pool)
        {
            _pool = pool;
        }

        public void Launch(Transform target, float damage, IMemoryPool pool)
        {
            _target = target;
            _damage = damage;
            _pool = pool;
            _hasHit = false; 
        }

        private void Update()
        {
            if (_hasHit) return; 

            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                Dispose();
                return;
            }

            Vector3 aimPosition = _target.position + _targetOffset;

            transform.position = Vector3.MoveTowards(transform.position, aimPosition, _speed * Time.deltaTime);
            transform.LookAt(aimPosition); 

            // ОПТИМИЗАЦИЯ: Используем sqrMagnitude вместо Distance (без вычисления корня)
            float sqrDistanceToTarget = (transform.position - aimPosition).sqrMagnitude;
            
            // Возводим дистанцию попадания тоже в квадрат для честного сравнения
            if (sqrDistanceToTarget <= (_hitDistance * _hitDistance))
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            _hasHit = true; 
            
            // В продакшене этот лог лучше убрать, чтобы не спамить консоль при 100+ выстрелах в секунду
            Debug.Log($"<color=magenta>[Projectile] Ядро попало в {_target.name}. Урон: {_damage}</color>");
            
            var damageable = _target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
            }  
            Dispose(); 
        }

        public class Pool : MonoMemoryPool<KinematicProjectile> {}
    }
}