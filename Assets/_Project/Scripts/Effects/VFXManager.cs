using UnityEngine;

namespace TpsShooter.Effects
{
    public class VFXManager : MonoBehaviour, IVFXService
    {
        [Header("Tracer Settings")]
        [SerializeField] private BulletTracer _tracerPrefab;
        [SerializeField] private int _tracerPoolSize = 30;
        [SerializeField] private float _tracerSpeed = 150f;

        [Header("Impact Settings (Blood)")]
        [SerializeField] private ParticleSystem _bloodPrefab;
        [SerializeField] private int _bloodPoolSize = 15;

        [Header("Impact Settings (Environment)")]
        [SerializeField] private ParticleSystem _sparksPrefab;
        [SerializeField] private int _sparksPoolSize = 15;

        private BulletTracer[] _tracerPool;
        private int _currentTracerIndex = 0;

        private ParticleSystem[] _bloodPool;
        private int _currentBloodIndex = 0;

        private ParticleSystem[] _sparksPool;
        private int _currentSparksIndex = 0;

        private void Awake()
        {
            InitializeTracerPool();
            _bloodPool = InitializeParticlePool(_bloodPrefab, _bloodPoolSize);
            _sparksPool = InitializeParticlePool(_sparksPrefab, _sparksPoolSize);
        }

        private void InitializeTracerPool()
        {
            if (_tracerPrefab == null) return;
            _tracerPool = new BulletTracer[_tracerPoolSize];
            for (int i = 0; i < _tracerPoolSize; i++)
            {
                _tracerPool[i] = Instantiate(_tracerPrefab, transform);
                _tracerPool[i].gameObject.SetActive(false);
            }
        }

        // Универсальный метод для создания пула партиклов
        private ParticleSystem[] InitializeParticlePool(ParticleSystem prefab, int size)
        {
            if (prefab == null) return null;
            ParticleSystem[] pool = new ParticleSystem[size];
            for (int i = 0; i < size; i++)
            {
                pool[i] = Instantiate(prefab, transform);
                // Мы не выключаем сам GameObject, партиклы просто находятся в состоянии "Stop"
            }
            return pool;
        }

        public void SpawnTracer(Vector3 startPoint, Vector3 endPoint)
        {
            if (_tracerPool == null || _tracerPool.Length == 0) return;

            BulletTracer tracer = _tracerPool[_currentTracerIndex];
            float distance = Vector3.Distance(startPoint, endPoint);
            float duration = distance / _tracerSpeed;

            tracer.Simulate(startPoint, endPoint, duration, null);
            _currentTracerIndex = (_currentTracerIndex + 1) % _tracerPoolSize;
        }

        public void SpawnImpact(Vector3 position, Vector3 normal, bool isEnemy)
        {
            if (isEnemy)
            {
                if (_bloodPool == null || _bloodPool.Length == 0) return;
                PlayParticleFromPool(_bloodPool, ref _currentBloodIndex, _bloodPoolSize, position, normal);
            }
            else
            {
                if (_sparksPool == null || _sparksPool.Length == 0) return;
                PlayParticleFromPool(_sparksPool, ref _currentSparksIndex, _sparksPoolSize, position, normal);
            }
        }

        private void PlayParticleFromPool(ParticleSystem[] pool, ref int currentIndex, int poolSize, Vector3 position, Vector3 normal)
        {
            ParticleSystem ps = pool[currentIndex];
            
            // Ставим партикл в точку попадания и разворачиваем "лицом" от поверхности по нормали
            ps.transform.position = position;
            ps.transform.rotation = Quaternion.LookRotation(normal);
            
            ps.Play(true); // true означает, что проиграются и все дочерние партиклы
            
            currentIndex = (currentIndex + 1) % poolSize;
        }
    }
}