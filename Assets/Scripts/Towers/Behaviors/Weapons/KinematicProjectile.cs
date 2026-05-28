using UnityEngine;
using Gameplay.Core;
using Zenject;

namespace Gameplay.Towers.Behaviors.Weapons
{
    public class KinematicProjectile : MonoBehaviour, System.IDisposable
    {
        [Header("Настройки полета")]
        [SerializeField] private float _speed = 15f;
        [SerializeField] private float _hitDistance = 0.2f; // На какой дистанции засчитываем попадание

        // НОВОЕ: Смещение цели (чтобы бить в грудь, а не в пятки)
        [SerializeField] private Vector3 _targetOffset = new Vector3(0f, 0.5f, 0f);



        private Transform _target;
        private float _damage;
        private IMemoryPool _pool;
        private bool _hasHit; // Флаг-предохранитель от двойного урона



        public void Dispose()
        {
            if (_pool != null)
            {
                _pool.Despawn(this); // Боевой режим (Zenject)
            }
            else
            {
                Destroy(gameObject); // Тестовый режим (Без Zenject)
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

        // Этот метод будет вызывать пушка сразу после спавна ядра
        public void Launch(Transform target, float damage, IMemoryPool pool)
        {
            _target = target;
            _damage = damage;
            _pool = pool;
            _hasHit = false; // Сбрасываем предохранитель при новом выстреле
        }

        private void Update()
        {
            if (_hasHit) return; // Если уже ударили, ждем исчезновения
            // ЗАЩИТА: Если враг умер от другой башни, пока ядро летело — ядро исчезает
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                Dispose();
                return;
            }

            // НОВОЕ: Вычисляем реальную точку, куда летим (Пятки + Смещение вверх)
            Vector3 aimPosition = _target.position + _targetOffset;

            // 1. Двигаем ядро к цели (математическая кинематика)
            transform.position = Vector3.MoveTowards(transform.position, aimPosition, _speed * Time.deltaTime);
            transform.LookAt(aimPosition); // Поворачиваем носом к цели (полезно для стрел/ракет)

            // 2. Проверяем попадание
            float distanceToTarget = Vector3.Distance(transform.position, aimPosition);
            if (distanceToTarget <= _hitDistance)
            {
                HitTarget();
            }
        }

        private void HitTarget()
        {
            _hasHit = true; // Защелкиваем предохранитель
            // НОВОЕ: Красивый лог попадания
            Debug.Log($"<color=magenta>[Projectile] Математический триггер сработал! Ядро попало в {_target.name}. Урон: {_damage}</color>");
            var damageable = _target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(_damage);
            }  
            Dispose(); // Вернуть снаряд в пул
        }

        // Класс пула для инсталлера Zenject
        public class Pool : MonoMemoryPool<KinematicProjectile> {}



    }
}


