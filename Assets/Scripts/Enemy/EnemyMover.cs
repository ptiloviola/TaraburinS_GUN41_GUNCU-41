using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{


    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMover : MonoBehaviour
    {
        private NavMeshAgent _agent;
        private EnemyConfig _config;
        
        private float _timer;
        private bool _isWandering;

        public void Initialize(EnemyConfig config)
        {
            _config = config;
            _agent = GetComponent<NavMeshAgent>();
            
            // Применяем настройки конфига к агенту
            _agent.speed = _config.moveSpeed;
            
            // Поднимаем агента над землей
            _agent.baseOffset = _config.baseOffset; 
        }

        // Включение режима блуждания
        public void StartWandering()
        {
            _isWandering = true;
            _timer = _config.wanderTimer; // Ставим таймер на максимум, чтобы враг пошел сразу при спавне
        }

        // Выключение (например, при смерти или оглушении)
        public void StopWandering()
        {
            _isWandering = false;
            _agent.ResetPath(); // Очищаем текущий маршрут, чтобы он резко остановился
        }

        private void Update()
        {
            if (!_isWandering) return;

            _timer += Time.deltaTime;

            if (_timer >= _config.wanderTimer)
            {
                SetRandomDestination();
                _timer = 0f;
            }
        }

        private void SetRandomDestination()
        {
            // 1. Берем случайную точку в сфере вокруг текущей позиции
            Vector3 randomDirection = Random.insideUnitSphere * _config.wanderRadius;
            randomDirection += transform.position;

            // 2. Ищем ближайшую валидную точку на самом NavMesh (чтобы не отправить врага за стену или в пропасть)
            // NavMesh.AllAreas означает, что он может ходить по любым запеченным слоям
            if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, _config.wanderRadius, NavMesh.AllAreas))
            {
                _agent.SetDestination(hit.position);
            }
        }

        // Публичный метод для чтения скорости (понадобится Аниматору для вращения нижнего шара)
        public float GetCurrentSpeed()
        {
            return _agent.velocity.magnitude;
        }
    }
}