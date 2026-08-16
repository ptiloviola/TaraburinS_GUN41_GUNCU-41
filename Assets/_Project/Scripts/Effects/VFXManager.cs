using UnityEngine;

namespace TpsShooter.Effects
{
    // Реализуем наш интерфейс IVFXService
    public class VFXManager : MonoBehaviour, IVFXService
    {
        [Header("Tracer Settings")]
        [SerializeField] private BulletTracer _tracerPrefab;
        [SerializeField] private int _tracerPoolSize = 30;
        
        [Tooltip("Скорость полета трассера (м/с)")]
        [SerializeField] private float _tracerSpeed = 150f;

        private BulletTracer[] _tracerPool;
        private int _currentTracerIndex = 0;

        private void Awake()
        {
            if (_tracerPrefab == null)
            {
                Debug.LogError("[VFXManager] Не назначен префаб трассера!");
                return;
            }

            // Предсоздаем все трассеры при старте
            _tracerPool = new BulletTracer[_tracerPoolSize];
            for (int i = 0; i < _tracerPoolSize; i++)
            {
                _tracerPool[i] = Instantiate(_tracerPrefab, transform);
                _tracerPool[i].gameObject.SetActive(false);
            }
        }

        // Вызывается из любого оружия через DI
        public void SpawnTracer(Vector3 startPoint, Vector3 endPoint)
        {
            if (_tracerPool == null || _tracerPool.Length == 0) return;

            BulletTracer tracer = _tracerPool[_currentTracerIndex];
            
            // Физика: Время = Расстояние / Скорость
            float distance = Vector3.Distance(startPoint, endPoint);
            float duration = distance / _tracerSpeed;

            tracer.Simulate(startPoint, endPoint, duration, null);

            _currentTracerIndex = (_currentTracerIndex + 1) % _tracerPoolSize;
        }
    }
}